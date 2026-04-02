using Hospital.API.Data;
using Hospital.API.Services;
using Hospital.Core.DTOs;
using Hospital.Core.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ShiftsController : ControllerBase
    {
        private readonly IShiftService _shiftService;
        private readonly ApplicationDbContext _context;

        public ShiftsController(IShiftService shiftService, ApplicationDbContext context)
        {
            _shiftService = shiftService;
            _context = context;
        }

        [HttpGet("calculate")]
        public async Task<IActionResult> GetShift(DateOnly date)
        {
            var team = await _shiftService.GetCurrentShiftDetail(date);
            return Ok(new
            {
                Date = date,
                TeamId = team.Id,
                SupervisorName = team.Supervisor?.Name ?? "لم يحدد"
            });
        }
        [HttpGet("settings")]
        public async Task<ActionResult<SystemSettingDto>> GetSettings()
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync();
            if (setting == null) return NotFound();

            return Ok(new SystemSettingDto
            {
                Id = setting.Id,
                ShiftReferenceDate = setting.ShiftReferenceDate
            });
        }
        [HttpPut("settings")]
        public async Task<IActionResult> UpdateReferenceDate(DateOnly newDate)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync();
            if (setting == null) _context.SystemSettings.Add(new SystemSetting { ShiftReferenceDate = newDate });
            else setting.ShiftReferenceDate = newDate;

            await _context.SaveChangesAsync();
            return Ok();
        }
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTeam(int id, NightShiftTeam teamDto)
        {
            // التأكد أن هذا المسؤول غير محجوز لخفرة أخرى
            var duplicate = await _context.NightShiftTeams
                .AnyAsync(t => t.Id != id && t.SupervisorId == teamDto.SupervisorId && teamDto.SupervisorId != null);

            if (duplicate)
            {
                return BadRequest("هذا المسؤول معين مسبقاً لخفارة أخرى.");
            }

            var team = await _context.NightShiftTeams.FindAsync(id);
            if (team == null) return NotFound();

            team.SupervisorId = teamDto.SupervisorId;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
