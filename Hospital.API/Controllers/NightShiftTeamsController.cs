using Hospital.API.Data;
using Hospital.Core.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NightShiftTeamsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        public NightShiftTeamsController(ApplicationDbContext context) => _context = context;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NightShiftTeamDto>>> GetTeams()
        {
            var teams = await _context.NightShiftTeams
                .Include(t => t.Supervisor)
                .Select(t => new NightShiftTeamDto
                {
                    Id = t.Id,
                    SupervisorId = t.SupervisorId,
                    SupervisorName = t.Supervisor != null ? t.Supervisor.Name : "لم يحدد"
                }).ToListAsync();

            return Ok(teams);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> UpdateTeam(int id, [FromBody] NightShiftTeamDto teamDto)
        {
            if (id != teamDto.Id) return BadRequest(new { message = "رقم فريق الخفر خاطئ" });

            var team = await _context.NightShiftTeams.FindAsync(id);
            if (team == null) return NotFound(new { message = "لم يتم العثور على فريق الخفر المحدد" });

            if (teamDto.SupervisorId.HasValue
                && !await _context.Employees.AnyAsync(e => e.Id == teamDto.SupervisorId.Value))
                return BadRequest(new { message = "لم يتم العثور على الموظف المسؤول المحدد" });

            // التأكد أن هذا المسؤول غير معين لخفارة أخرى (نفس قاعدة ShiftsController)
            var duplicate = await _context.NightShiftTeams
                .AnyAsync(t => t.Id != id && t.SupervisorId == teamDto.SupervisorId && teamDto.SupervisorId != null);
            if (duplicate) return BadRequest(new { message = "هذا المسؤول معين مسبقاً لخفارة أخرى." });

            try
            {
                team.SupervisorId = teamDto.SupervisorId;
                await _context.SaveChangesAsync();
                return Ok(teamDto);
            }
            catch (Exception)
            {
                return StatusCode(500, new { message = "حدث خطأ أثناء تحديث البيانات" });
            }
        }
    }
}
