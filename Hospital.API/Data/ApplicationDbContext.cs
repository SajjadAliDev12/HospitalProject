using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Hospital.Core.Models;


namespace Hospital.API.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Leave> Leaves { get; set; }
        public DbSet<AuditLog> AuditLogs { get; set; }
        public DbSet<TransferLog> TransferLogs { get; set; }
        public DbSet<Absent> Absents { get; set; }
        public DbSet<JobTitle> JobTitles { get; set; }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            builder.Entity<Employee>().HasIndex(e => e.Id).IsUnique();
            builder.Entity<Department>().HasIndex(d => d.Id).IsUnique();
            builder.Entity<Leave>().HasIndex(l => l.Id).IsUnique();
            builder.Entity<AuditLog>().HasIndex(A => A.Id).IsUnique();
            builder.Entity<TransferLog>().HasIndex(B => B.Id).IsUnique();
            builder.Entity<Absent>().HasIndex(C => C.Id).IsUnique();
            builder.Entity<JobTitle>().HasIndex(D => D.Id).IsUnique();

            builder.Entity<Employee>().HasQueryFilter(e => !e.isDeleted);
            builder.Entity<Department>().HasQueryFilter(e => !e.isDeleted);
            builder.Entity<Leave>().HasQueryFilter(e => !e.isDeleted);
            builder.Entity<Absent>().HasQueryFilter(e => !e.isDeleted);
            
            builder.Entity<Employee>().HasOne(e => e.Department).WithMany(d =>  d.Employees).HasForeignKey(e => e.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            builder.Entity<Leave>().HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<Absent>().HasOne(e => e.Employee).WithMany().HasForeignKey(e => e.EmployeeId).OnDelete(DeleteBehavior.Cascade);
            builder.Entity<TransferLog>().HasOne(t => t.Employee).WithMany().HasForeignKey(t => t.EmployeeId).OnDelete(DeleteBehavior.NoAction);
            builder.Entity<Leave>().HasOne(l => l.SubEmployee).WithMany().HasForeignKey(l => l.SubEmployeeId).OnDelete(DeleteBehavior.NoAction);
        }
    }
}
