using Hospital.API.Data;
using Hospital.Core.Models;
using Microsoft.EntityFrameworkCore;

namespace Hospital.API.Services
{
    public interface IShiftService
    {
        Task<int> GetTeamIdByDate(DateOnly targetDate);
        Task<NightShiftTeam> GetCurrentShiftDetail(DateOnly targetDate);
    }

    public class ShiftService : IShiftService
    {
        private readonly ApplicationDbContext _context;

        public ShiftService(ApplicationDbContext context) => _context = context;

        public async Task<int> GetTeamIdByDate(DateOnly targetDate)
        {
            var setting = await _context.SystemSettings.FirstOrDefaultAsync();
            if (setting == null) throw new Exception("لم يتم ضبط التاريخ المرجعي للنظام.");

            return ShiftCalculator.GetTeamId(setting.ShiftReferenceDate, targetDate);
        }

        public async Task<NightShiftTeam> GetCurrentShiftDetail(DateOnly targetDate)
        {
            int teamId = await GetTeamIdByDate(targetDate);
            return await _context.NightShiftTeams
                .Include(t => t.Supervisor)
                .FirstOrDefaultAsync(t => t.Id == teamId);
        }
    }
}
