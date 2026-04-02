using Hospital.API.Data;
using Hospital.Core.DTOs;
using Hospital.Core.Models;
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
        public async Task<IActionResult> UpdateTeam(int id, NightShiftTeam team)
        {
            if (id != team.Id) return BadRequest();
            _context.Entry(team).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
