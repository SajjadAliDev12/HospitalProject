using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class SystemSettingDto
    {
        public int Id { get; set; }
        public DateOnly ShiftReferenceDate { get; set; }
    }

    // لنقل بيانات فريق الخفر بدون تعقيد الموظف
    public class NightShiftTeamDto
    {
        public int Id { get; set; }
        public int? SupervisorId { get; set; }
        public string? SupervisorName { get; set; } // لعرض الاسم فقط في القائمة
    }
}
