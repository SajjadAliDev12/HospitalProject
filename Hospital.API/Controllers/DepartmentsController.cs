using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Enums;
using Hospital.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DepartmentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public DepartmentsController(ApplicationDbContext context)
        {
            _context = context;
        }
        
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<DepartmentDto>>> GetDepartments([FromQuery] bool? IsDeleted)
        {
            IQueryable<Department> query = _context.Departments.IgnoreQueryFilters().AsNoTracking();
            if(IsDeleted.HasValue)
            {
                query = query.Where(e => e.isDeleted == IsDeleted.Value);
            }
            var departments = await query
            .Select(d => new DepartmentDto
            {
                Id = d.Id,
                Name = d.Name,
                IsDeleted = d.isDeleted,
                StaffCount = d.Employees.Count(e => !e.isDeleted),
                MorningCount = d.Employees.Count(e => !e.isDeleted && e.ShiftType == enShiftType.Morning),
                NightCount = d.Employees.Count(e => !e.isDeleted && e.ShiftType == enShiftType.Night)
            }).ToListAsync();

            return Ok(departments);
        }
        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartmentDto>> GetDepartment(int id)
        {
            var department = await _context.Departments.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == id);
            if (department == null) return NotFound(new { message = "لم يتم العثور على القسم المحدد" });

            return Ok(new DepartmentDto { Id = department.Id, Name = department.Name, IsDeleted = department.isDeleted });
        }
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<ActionResult<DepartmentDto>> PostDepartment(CreateDepartmentDto departmentDto)
        {
            try
            {
                var department = new Department
                {
                    Name = departmentDto.Name,
                    isDeleted = false
                };
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
                var resultDto = new DepartmentDto
                {
                    Id = department.Id,
                    Name = department.Name,
                    IsDeleted = department.isDeleted
                };
                return CreatedAtAction("GetDepartment", new { id = department.Id },resultDto);
            }
            catch (Exception ex)
            {
                return StatusCode((int)HttpStatusCode.InternalServerError,new {message = "حدث خطأ في معالجة البيانات"});
            }
        }
        [HttpDelete("{Id}")]
        [Authorize(Roles ="Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteDepartment(int Id)
        {
            var department = await _context.Departments.FindAsync(Id);
            if (department == null)
                return NotFound(new { message = "لم يتم العثور على القسم المحدد" });
            department.isDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPut("{id}")] 
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<DepartmentDto>> EditDepartment(int id, [FromBody] DepartmentDto departmentDto)
        {
            
            var department = await _context.Departments.IgnoreQueryFilters().FirstOrDefaultAsync(d => d.Id == id); 

    if (department == null)
                return NotFound(new { message = "لم يتم العثور على القسم المحدد" });

            
             department.Name = departmentDto.Name; 
     department.isDeleted = departmentDto.IsDeleted; 

    
     await _context.SaveChangesAsync(); 

    
    return Ok(new DepartmentDto
    {
        Id = department.Id,
        Name = department.Name,
        IsDeleted = department.isDeleted
    });
        }
    }
}
