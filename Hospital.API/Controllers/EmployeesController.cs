using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Models;
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
        public async Task<ActionResult<IEnumerable<EmployeeSimpleDTO>>> GetEmployees([FromQuery] bool? IsDeleted)
        {
            IQueryable<Employee> query = _dbContext.Employees.IgnoreQueryFilters().AsNoTracking();
            if (IsDeleted.HasValue)
            {
                query = query.Where(e => e.isDeleted == IsDeleted.Value);
            }
            var Employees = await query.Select(e => new EmployeeSimpleDTO
            {
                Id = e.Id,
                Name = e.Name,
                BirthDate = e.BirthDate,
                IsDeleted = e.isDeleted,
                DepartmentID = e.DepartmentId,
                Gender = e.Gender,
                PhoneNumber = e.PhoneNumber,
                HireDate = e.HireDate,
            }).ToListAsync();
            return Ok(Employees);
        }

        [HttpGet("{Id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<EmployeeSimpleDTO>> GetEmployee(int Id)
        {
            var employee = await _dbContext.Employees.IgnoreQueryFilters().FirstOrDefaultAsync(e => e.Id == Id);
            if (employee == null)
                return NotFound("Employee Not Found");
            return Ok(new EmployeeSimpleDTO { Id = employee.Id , BirthDate = employee.BirthDate , Gender = employee.Gender , DepartmentID = employee.DepartmentId , HireDate = employee.HireDate , IsDeleted = employee.isDeleted , Name = employee.Name , PhoneNumber = employee.PhoneNumber});
        }
    }
}

