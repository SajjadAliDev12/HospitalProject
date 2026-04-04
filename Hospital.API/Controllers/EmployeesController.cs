using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Hospital.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        public EmployeesController(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PagedResult<EmployeeSimpleDTO>>> GetEmployees([FromQuery] bool? IsDeleted,[FromQuery] string? searchTerm,[FromQuery] int page = 1,[FromQuery] int pageSize = 20)
        {
            IQueryable<Employee> query = _dbContext.Employees.IgnoreQueryFilters().AsNoTracking();

            if (IsDeleted.HasValue)
                query = query.Where(e => e.isDeleted == IsDeleted.Value);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(e => e.Name.Contains(searchTerm));
            }

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(e => e.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(e => new EmployeeSimpleDTO
                {
                    Id = e.Id,
                    Name = e.Name,
                    BirthDate = e.BirthDate,
                    HireDate = e.HireDate,
                    Gender = e.Gender,
                    PhoneNumber = e.PhoneNumber,
                    IsDeleted = e.isDeleted,
                    DepartmentID = e.DepartmentId
                }).ToListAsync();

            return Ok(new PagedResult<EmployeeSimpleDTO>
            {
                Items = items,
                TotalCount = totalCount,
                PageSize = pageSize,
                CurrentPage = page
            });
        }
        [HttpGet("report-data")]
        public async Task<ActionResult<IEnumerable<EmployeeReportDto>>> GetEmployeesForReport(int? departmentId, enShiftType? shiftType)
        {
            var query = _dbContext.Employees
                .Include(e => e.Department)
                .Include(e => e.JobTitle)
                .AsQueryable();

            // الفلترة حسب القسم (إذا اختار الآدمين قسماً محدداً)
            if (departmentId.HasValue)
            {
                query = query.Where(e => e.DepartmentId == departmentId.Value);
            }

            // الفلترة حسب نوع الدوام (صباحي/خفر/الكل)
            if (shiftType.HasValue)
            {
                query = query.Where(e => e.ShiftType == shiftType.Value);
            }

            var result = await query.Select(e => new EmployeeReportDto
            {
                Name = e.Name,
                JobTitle = e.JobTitle.Title,
                ShiftType = e.ShiftType,
                MorningShiftGroup = e.enMorningGroup,
                NightShiftId = e.NightShiftId,
                DepartmentName = e.Department.Name
            }).ToListAsync();

            return Ok(result);
        }
        [HttpGet("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployeeFullDTO>> GetEmployee(int Id)
        {
            var employee = await _dbContext.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == Id);
            if (employee == null)
                return NotFound(new { message = "لم يتم العثور على الموظف المحدد" });
            return Ok(new EmployeeFullDTO
            {
                Name = employee.Name,
                BirthDate = employee.BirthDate,
                IsDeleted = employee.isDeleted,
                HireDate = employee.HireDate,
                CertificateType = employee.CertificateType,
                DepartmentId = employee.DepartmentId,
                Id = employee.Id,
                JobStatus = employee.JobStatus,
                LeaveBalance = employee.LeaveBalance,
                JobTitleId = employee.JobTitleId,
                PhoneNumber = employee.PhoneNumber,
                Address = employee.Address,
                ShiftType = employee.ShiftType,
                Gender = employee.Gender,
                LeaveCardNumber = employee.LeaveCardNumber,
                enMorningGroup = employee.enMorningGroup,
                NightShiftId = employee.NightShiftId
            });
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult> PostEmployee([FromBody] CreateEmployeeDto dto)
        {
            
            if (!await _dbContext.Departments.AnyAsync(d => d.Id == dto.DepartmentId))
                return BadRequest(new { message = "لم يتم العثور على القسم المحدد" });

            if (!await _dbContext.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
                return BadRequest(new { message = "لم يتم العثور على العنوان الوظيفي المحدد" });
            var employee = new Employee
            {
                Name = dto.Name,
                BirthDate = dto.BirthDate,
                HireDate = dto.HireDate,
                DepartmentId = dto.DepartmentId,
                JobTitleId = dto.JobTitleId,
                ShiftType = (enShiftType)dto.ShiftType,
                Gender = (enGender)dto.Gender,
                CertificateType = (enCertificate)dto.CertificateType,
                LeaveBalance = dto.LeaveBalance,
                Address = dto.Address,
                PhoneNumber = dto.PhoneNumber,
                JobStatus = (enJobStatus)dto.JobStatus,
                isDeleted = false,
                LeaveCardNumber = dto.LeaveCardNumber,
                enMorningGroup = dto.enMorningGroup,
                NightShiftId = dto.NightShiftId
            };

            try
            {
                _dbContext.Employees.Add(employee);
                await _dbContext.SaveChangesAsync();

                
                return CreatedAtAction(nameof(GetEmployee), new { id = employee.Id }, new { employee.Id, employee.Name });
            }
            catch (Exception)
            {
                return StatusCode(500, new {message = "حدث خطأ اثناء معالجة البيانات"});
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> PutEmployee(int id, [FromBody] EmployeeFullDTO dto)
        {
            if (id != dto.Id) return BadRequest(new { message = "الرقم الوظيفي غير متطابق" });
            var employee = await _dbContext.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == id);
            if (employee == null) return NotFound(new { message = "لم يتم العثور على الموظف المحدد" });
            if (!await _dbContext.Departments.AnyAsync(d => d.Id == dto.DepartmentId))
                return BadRequest(new { message = "لم يتم العثور على القسم المحدد" });
            if (!await _dbContext.JobTitles.AnyAsync(j => j.Id == dto.JobTitleId))
                return BadRequest(new { message = "لم يتم العثور على العنوان الوظيفي المحدد" });
            employee.Name = dto.Name;
            employee.BirthDate = dto.BirthDate;
            employee.HireDate = dto.HireDate;
            employee.DepartmentId = dto.DepartmentId;
            employee.JobTitleId = dto.JobTitleId;
            employee.ShiftType = (enShiftType)dto.ShiftType;
            employee.Gender = (enGender)dto.Gender;
            employee.CertificateType = (enCertificate)dto.CertificateType;
            employee.LeaveBalance = dto.LeaveBalance;
            employee.Address = dto.Address;
            employee.PhoneNumber = dto.PhoneNumber;
            employee.JobStatus = (enJobStatus)dto.JobStatus;
            employee.isDeleted = dto.IsDeleted; 
            employee.LeaveCardNumber = dto.LeaveCardNumber;
            employee.enMorningGroup = dto.enMorningGroup;
            employee.NightShiftId = dto.NightShiftId;


            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(dto); 
            }
            catch (Exception)
            {
                return StatusCode(500, new {message = "حدث خطأ اثناء معالجة البيانات"});
            }
        }
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteEmployee(int id)
        {
            var employee = await _dbContext.Employees.FindAsync(id);
            if (employee == null) return NotFound(new { message = "لم يتم العثور على الموظف المحدد" });

            employee.isDeleted = true;

            try
            {
                await _dbContext.SaveChangesAsync();
                return Ok(new { message = $"تم حذف الموظف رقم {id} بنجاح" });
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "حدث خطأ اثناء معالجة البيانات" });
            }
        }
        [HttpGet("Search")]
        public async Task<ActionResult<IEnumerable<EmployeeLookupDto>>> SearchEmployees([FromQuery] string term)
        {
            if (string.IsNullOrWhiteSpace(term)) return Ok(new List<EmployeeLookupDto>());

            var results = await _dbContext.Employees
                .AsNoTracking()
                .Where(e => !e.isDeleted && e.Name.Contains(term))
                .Take(10)
                .Select(e => new EmployeeLookupDto
                {
                    Id = e.Id,
                    Name = e.Name
                })
                .ToListAsync();

            return Ok(results);
        }
    }
}

