using Hospital.Core.Enums;
using Hospital.Core.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;


namespace Hospital.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options , IHttpContextAccessor httpContextAccessor) : base(options) { _httpContextAccessor = httpContextAccessor; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<TransferLog> TransferLogs { get; set; }
        public DbSet<Absent> Absents { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        public DbSet<NightShiftTeam> NightShiftTeams { get; set; }
        public DbSet<SystemSetting> SystemSettings { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            
            builder.Entity<Employee>().HasQueryFilter(e => !e.isDeleted);
            builder.Entity<Department>().HasQueryFilter(e => !e.isDeleted);
            builder.Entity<Leave>().HasQueryFilter(e => !e.isDeleted);
            builder.Entity<Absent>().HasQueryFilter(e => !e.isDeleted);
            
            builder.Entity<Employee>().HasOne(e => e.Department).WithMany(d =>  d.Employees).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Leave>().HasOne(e => e.Employee).WithMany(l => l.Leaves).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Absent>().HasOne(e => e.Employee).WithMany(s => s.Absents).HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<TransferLog>().HasOne(t => t.Employee).WithMany().HasForeignKey(t => t.EmployeeId).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Leave>().HasOne(l => l.SubEmployee).WithMany().HasForeignKey(l => l.SubEmployeeId).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<TransferLog>().HasOne(t => t.OldDepartment).WithMany().HasForeignKey(t => t.OldDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<TransferLog>().HasOne(t => t.NewDepartment).WithMany().HasForeignKey(t => t.NewDepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<NightShiftTeam>().HasOne(t => t.Supervisor).WithMany().HasForeignKey(t => t.SupervisorId).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Employee>().HasOne(e => e.nightShift).WithMany().HasForeignKey(e=>e.NightShiftId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Department>().HasOne(d=>d.Manager).WithMany().HasForeignKey(d=>d.ManagerId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<NightShiftTeam>().HasData(
    new NightShiftTeam { Id = 1, SupervisorId = null },
    new NightShiftTeam { Id = 2, SupervisorId = null },
    new NightShiftTeam { Id = 3, SupervisorId = null },
    new NightShiftTeam { Id = 4, SupervisorId = null }
);
        }
        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "System";

            // 1. التقاط العمليات قبل الحفظ
            var entries = ChangeTracker.Entries()
                .Where(e => e.Entity is not AuditLog &&
                           (e.State == EntityState.Added || e.State == EntityState.Modified || e.State == EntityState.Deleted))
                .ToList();

            // قائمة مؤقتة لتخزين معلومات التدقيق قبل كتابتها
            var auditEntries = new List<AuditEntry>();

            foreach (var entry in entries)
            {
                auditEntries.Add(new AuditEntry
                {
                    Entry = entry,
                    UserId = userId,
                    Date = DateTime.Now,
                    EntityName = entry.Entity.GetType().Name,
                    // تحديد نوع العملية
                    Type = entry.Entity switch
                    {
                        TransferLog => enAuditType.Transfer,
                        Leave => enAuditType.Leave,
                        Absent => enAuditType.Absent,
                        _ => entry.State switch
                        {
                            EntityState.Added => enAuditType.Add,
                            EntityState.Modified => enAuditType.Edit,
                            EntityState.Deleted => enAuditType.Delete,
                            _ => enAuditType.Add
                        }
                    }
                });
            }

            // 2. الحفظ الأول: لجلب المعرفات الحقيقية (الـ IDs) من قاعدة البيانات
            var result = await base.SaveChangesAsync(cancellationToken);

            // 3. الحفظ الثاني: تحديث سجلات التدقيق بالمعرفات الحقيقية
            if (auditEntries.Any())
            {
                foreach (var auditEntry in auditEntries)
                {
                    var auditLog = new AuditLog
                    {
                        Date = auditEntry.Date,
                        UserId = auditEntry.UserId,
                        EntityName = auditEntry.EntityName,
                        Type = auditEntry.Type,
                        // الآن نأخذ الـ ID الحقيقي بعد أن قام SQL Server بتوليده
                        RecordId = auditEntry.Entry.Properties.FirstOrDefault(p => p.Metadata.IsPrimaryKey())?.CurrentValue?.ToString()
                    };
                    AuditLogs.Add(auditLog);
                }
                // حفظ سجلات التدقيق فقط
                await base.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        // كلاس داخلي بسيط للمساعدة في نقل البيانات بين المرحلتين
        private class AuditEntry
        {
            public Microsoft.EntityFrameworkCore.ChangeTracking.EntityEntry Entry { get; set; }
            public string UserId { get; set; }
            public string EntityName { get; set; }
            public DateTime Date { get; set; }
            public enAuditType Type { get; set; }
        }
    }
}
