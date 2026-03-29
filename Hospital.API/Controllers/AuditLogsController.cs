using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AuditLogsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AuditLogsController(ApplicationDbContext context)
        {  _context = context; }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var query = _context.AuditLogs.Include(a => a.User).AsNoTracking();

            var totalRecords = await query.CountAsync();
            var totalPages = (int)Math.Ceiling(totalRecords / (double)pageSize);

            // 1. جلب البيانات الخام من قاعدة البيانات أولاً (بدون switch)
            var rawLogs = await query
                .OrderByDescending(a => a.Date)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // 2. الآن نقوم بعملية الـ Mapping في الذاكرة (Client-side) حيث يعمل الـ switch بشكل مثالي
            var logs = rawLogs.Select(a => new AuditLogDTO
            {
                Id = a.Id,
                Date = a.Date,
                UserId = a.UserId,
                UserName = a.User?.UserName ?? "غير معروف",
                ActionType = a.Type switch
                {
                    enAuditType.Add => "إضافة",
                    enAuditType.Edit => "تعديل",
                    enAuditType.Delete => "حذف",
                    enAuditType.Transfer => "نقل إداري",
                    enAuditType.Absent => "تسجيل غياب",
                    enAuditType.Leave => "إجازة",
                    _ => a.Type.ToString()
                },
                EntityName = a.EntityName switch
                {
                    "Employee" => "موظف",
                    "Department" => "قسم",
                    "Absent" => "غياب",
                    "TransferLog" => "سجل نقل",
                    "Leave" => "إجازة",
                    _ => a.EntityName
                },
                RecordId = a.RecordId
            }).ToList();

            return Ok(new { Items = logs, TotalPages = totalPages, CurrentPage = page });
        }
    }
}
