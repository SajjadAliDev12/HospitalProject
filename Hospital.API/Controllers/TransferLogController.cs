using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Hospital.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TransferLogController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransferLogController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<TransferLogDto>>> GetTransferLogs([FromQuery] int? employeeId)
        {
            IQueryable<TransferLog> query = _context.TransferLogs.AsNoTracking();

            if (employeeId.HasValue)
                query = query.Where(t => t.EmployeeId == employeeId);

            var logs = await query.Select(t => new TransferLogDto
            {
                Id = t.Id,
                EmployeeId = t.EmployeeId,
                EmployeeName = t.Employee.Name, 
                OldDepartmentId = t.OldDepartmentId,
                NewDepartmentId = t.NewDepartmentId,
                OldShiftType = (int)t.OldShiftType,
                NewShiftType = (int)t.NewShiftType,
                TransferDate = t.TransferDate,
                AdOrderNumber = t.AdOrderNumber
            }).ToListAsync();

            return Ok(logs);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<TransferLogDto>> PostTransfer(CreateTransferLogDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null) return NotFound(new { message = "لم يتم العثور على الموظف المحدد" });
            if (!await _context.Departments.AnyAsync(d => d.Id == dto.NewDepartmentId))
                return BadRequest(new { message = "لم يتم العثور على القسم المحدد" });
            var transferLog = new TransferLog
            {
                EmployeeId = dto.EmployeeId,
                OldDepartmentId = employee.DepartmentId, 
                NewDepartmentId = dto.NewDepartmentId,
                OldShiftType = employee.ShiftType,      
                NewShiftType = (enShiftType)dto.NewShiftType,
                TransferDate = dto.TransferDate,
                AdOrderNumber = dto.AdOrderNumber
            };
            employee.DepartmentId = dto.NewDepartmentId;
            employee.ShiftType = (enShiftType)dto.NewShiftType;

            try
            {
                _context.TransferLogs.Add(transferLog);
                await _context.SaveChangesAsync();
                var resultDto = new TransferLogDto
                {
                    Id = transferLog.Id,
                    EmployeeId = transferLog.EmployeeId,
                    TransferDate = transferLog.TransferDate,
                    AdOrderNumber = transferLog.AdOrderNumber
                };

                return CreatedAtAction(nameof(GetTransferLogs), new { employeeId = transferLog.EmployeeId }, resultDto);
            }
            catch (Exception)
            {
                return StatusCode(500, new {message = "حدث خطأ أثناء معالجة البيانات"});
            }
        }
    }
}
