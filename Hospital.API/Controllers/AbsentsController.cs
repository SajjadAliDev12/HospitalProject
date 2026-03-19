using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AbsentsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public AbsentsController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<IEnumerable<AbsentFullDto>>> GetAbsents([FromQuery] int? employeeId ,[FromQuery] bool? IsDeleted)
        {
            IQueryable<Absent> query = _context.Absents.IgnoreQueryFilters().AsNoTracking();

            if (employeeId.HasValue)
                query = query.Where(a => a.EmployeeId == employeeId);
            if(IsDeleted.HasValue)
            {
                query = query.Where(a => a.isDeleted == IsDeleted.Value);
            }

            var absents = await query.Select(a => new AbsentFullDto
            {
                Id = a.Id,
                EmployeeId = a.EmployeeId,
                Date = a.Date,
                IsDeleted = a.isDeleted
            }).ToListAsync();

            return Ok(absents);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AbsentFullDto>> GetAbsent(int id)
        {
            var absent = await _context.Absents.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (absent == null) return NotFound(new {message = "لم يتم العثور على السجل المحدد"});

            return Ok(new AbsentFullDto
            {
                Id = absent.Id,
                EmployeeId = absent.EmployeeId,
                Date = absent.Date,
                IsDeleted = absent.isDeleted
            });
        }
        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<AbsentFullDto>> PostAbsent(CreateAbsentDto dto)
        {
            if (!await _context.Employees.AnyAsync(e => e.Id == dto.EmployeeId))
                return BadRequest(new { message = "الموظف غير موجود" });

            var absent = new Absent
            {
                EmployeeId = dto.EmployeeId,
                Date = dto.Date,
                isDeleted = false
            };

            try
            {
                await _context.Absents.AddAsync(absent);
                await _context.SaveChangesAsync();

                var resultDto = new AbsentFullDto
                {
                    Id = absent.Id,
                    EmployeeId = absent.EmployeeId,
                    Date = absent.Date,
                    IsDeleted = absent.isDeleted
                };

                return CreatedAtAction(nameof(GetAbsent), new { id = absent.Id }, resultDto);
            }
            catch (Exception)
            {
                return StatusCode(500,new { message = "حدث خطأ أثناء تسجيل الغياب" });
            }
        }
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Manager")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult> PutAbsent(int id, [FromBody] AbsentFullDto dto)
        {
            if (id != dto.Id) return BadRequest(new {message = "رقم السجل غير متطابق"});

            var absent = await _context.Absents.IgnoreQueryFilters().FirstOrDefaultAsync(a => a.Id == id);
            if (absent == null) return NotFound(new {message = "لم يتم العثور على السجل المحدد"});

            absent.Date = dto.Date;
            absent.isDeleted = dto.IsDeleted;

            await _context.SaveChangesAsync();
            return Ok(dto);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> DeleteAbsent(int id)
        {
            var absent = await _context.Absents.FindAsync(id);
            if (absent == null) return NotFound(new { message = "لم يتم العثور على السجل المحدد" });

            absent.isDeleted = true;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
