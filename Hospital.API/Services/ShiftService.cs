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

            // حساب الفرق بالأيام
            DateTime start = setting.ShiftReferenceDate.ToDateTime(TimeOnly.MinValue);
            DateTime end = targetDate.ToDateTime(TimeOnly.MinValue);

            int daysDifference = (end - start).Days;

            // منطق الـ Modulo 4
            // +4 لضمان عدم الحصول على قيمة سالبة في حال كان التاريخ المطلوب قبل المرجعي
            int teamIndex = ((daysDifference % 4) + 4) % 4;

            // نتيجتنا هي (0, 1, 2, 3) ونحن نريد الفرق (1, 2, 3, 4)
            return teamIndex + 1;
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
