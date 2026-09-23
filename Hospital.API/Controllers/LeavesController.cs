using Hospital.API.Data;
using Hospital.API.Services;
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
    public class LeavesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public LeavesController(ApplicationDbContext context)
        {
            _context = context;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> GetLeaves(
    [FromQuery] string? employeeName,
    [FromQuery] string? subEmployeeName,
    [FromQuery] DateTime? date,
    [FromQuery] bool? IsDeleted,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 15)
        {
            var query = _context.Leaves
                .Include(l => l.Employee)
                .Include(l => l.SubEmployee)
                .IgnoreQueryFilters()
                .AsNoTracking();

            // فلاتر البحث
            if (!string.IsNullOrEmpty(employeeName))
                query = query.Where(l => l.Employee.Name.Contains(employeeName));

            if (!string.IsNullOrEmpty(subEmployeeName))
                query = query.Where(l => l.SubEmployee.Name.Contains(subEmployeeName));

            if (date.HasValue)
                query = query.Where(l => l.StartDate.Date <= date.Value.Date && l.EndDate.Date >= date.Value.Date);

            if (IsDeleted.HasValue)
                query = query.Where(l => l.isDeleted == IsDeleted.Value);

            var totalRecords = await query.CountAsync();

            var items = await query
                .OrderByDescending(l => l.StartDate)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(l => new LeaveFullDto
                {
                    Id = l.Id,
                    EmployeeId = l.EmployeeId,
                    EmployeeName = l.Employee.Name,
                    SubEmployeeId = l.SubEmployeeId,
                    SubEmployeeName = l.SubEmployee.Name,
                    Duration = l.Duration,
                    CurrentBalance = l.Employee.LeaveBalance, // جلب الرصيد الحالي
                    StartDate = l.StartDate,
                    EndDate = l.EndDate,
                    LeaveType = l.LeaveType,
                    IsDeleted = l.isDeleted
                }).ToListAsync();

            return Ok(new PagedResult<LeaveFullDto>
            {
                Items = items,
                TotalCount = totalRecords,
                PageSize = pageSize,
                CurrentPage = page
            });
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<LeaveFullDto>> GetLeave(int id)
        {
            var leave = await _context.Leaves.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null) return NotFound(new {message = "لم يتم العثور على الأجازة المحددة"});

            return Ok(new LeaveFullDto
            {
                Id = leave.Id,
                EmployeeId = leave.EmployeeId,
                SubEmployeeId = leave.SubEmployeeId,
                Duration = leave.Duration,
                StartDate = leave.StartDate,
                EndDate = leave.EndDate,
                LeaveType = leave.LeaveType,
                IsDeleted = leave.isDeleted
            });
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<LeaveFullDto>> PostLeave(CreateLeaveDto dto)
        {
            var employee = await _context.Employees.FindAsync(dto.EmployeeId);
            if (employee == null) return BadRequest(new { message = "لم يتم العثور على الموظف المحدد" });

            if (!await _context.Employees.AnyAsync(e => e.Id == dto.SubEmployeeId))
                return BadRequest(new { message = "لم يتم العثور على الموظف البديل" });
            if (!LeaveBalanceCalculator.CanDeduct(employee.LeaveBalance, dto.Duration))
                return BadRequest(new { message = "رصيد الأجازات غير كافي" });
            if (dto.EmployeeId == dto.SubEmployeeId)
                return BadRequest(new { message = "لا يمكن للموظف أن يكون بديلاً لنفسه" });
            var leave = new Leave
            {
                EmployeeId = dto.EmployeeId,
                SubEmployeeId = dto.SubEmployeeId,
                Duration = dto.Duration,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                LeaveType = (enLeaveType)dto.LeaveType,
                isDeleted = false
            };
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                employee.LeaveBalance = LeaveBalanceCalculator.ApplyDeduction(employee.LeaveBalance, dto.Duration);

                _context.Leaves.Add(leave);

                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var resultDto = new LeaveFullDto
                {
                    Id = leave.Id,
                    EmployeeId = leave.EmployeeId,
                    Duration = leave.Duration,
                    EndDate = leave.EndDate,
                    IsDeleted = leave.isDeleted,
                    LeaveType = leave.LeaveType,
                    StartDate = leave.StartDate,
                    SubEmployeeId = leave.SubEmployeeId
                };
                return CreatedAtAction(nameof(GetLeave), new { id = leave.Id }, resultDto);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "حدث خطأ اثناء معالجة البيانات" });
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> PutLeave(int id, [FromBody] LeaveFullDto dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "رقم الاجازة خاطئ" });

            var leave = await _context.Leaves.IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == id);
            if (leave == null) return NotFound(new { message = "لم يتم العثور على الأجازة المحددة" });

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                int oldDuration = leave.Duration;
                int newDuration = dto.Duration;

                if (oldDuration != newDuration)
                {
                    var employee = await _context.Employees.FindAsync(leave.EmployeeId);
                    if (employee != null)
                    {
                        int difference = newDuration - oldDuration;
                        if (difference > 0) 
                        {
                            if (!LeaveBalanceCalculator.CanDeduct(employee.LeaveBalance, difference))
                                return BadRequest(new { message = "رصيد الاجازات غير كافي" });
                            employee.LeaveBalance = LeaveBalanceCalculator.ApplyDurationDelta(employee.LeaveBalance, oldDuration, newDuration);
                        }
                        else 
                        {
                            employee.LeaveBalance = LeaveBalanceCalculator.ApplyDurationDelta(employee.LeaveBalance, oldDuration, newDuration);
                        }
                    }
                }
                leave.Duration = dto.Duration;
                leave.StartDate = dto.StartDate;
                leave.EndDate = dto.EndDate;
                leave.SubEmployeeId = dto.SubEmployeeId;
                leave.LeaveType = (enLeaveType)dto.LeaveType;
                leave.isDeleted = dto.IsDeleted;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return Ok(dto);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, new { message = "حدث خطأ أثناء تحديث البيانات" });
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteLeave(int id)
        {
            var leave = await _context.Leaves.FindAsync(id);
            if (leave == null) return NotFound(new { message = "لم يتم العثور على الأجازة المحددة" });

            leave.isDeleted = true;
            Employee? E =await _context.Employees.FindAsync(leave.EmployeeId);
            if (E == null) return NotFound(new { message = "لم يتم العثور على الموظف صاحب الأجازة" });
            E.LeaveBalance = LeaveBalanceCalculator.Restore(E.LeaveBalance, leave.Duration);
            await _context.SaveChangesAsync();
            return Ok(new { message = "تم حذف الأجازة بنجاح" });
        }
    }
}
