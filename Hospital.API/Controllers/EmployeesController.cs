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
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<IEnumerable<EmployeeSimpleDTO>>> GetEmployees([FromQuery] bool? IsDeleted)
        {
            IQueryable<Employee> query =  _dbContext.Employees.AsNoTracking();
            if(IsDeleted.HasValue)
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

        }
    }
