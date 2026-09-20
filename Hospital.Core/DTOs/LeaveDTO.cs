using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Core.Enums;

namespace Hospital.Core.DTOs
{

    public class LeaveFullDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!; // جديد
        public int SubEmployeeId { get; set; }
        public string SubEmployeeName { get; set; } = null!; // جديد
        public int Duration { get; set; }
        public int CurrentBalance { get; set; } // جديد: لعرض الرصيد المتبقي في القائمة
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public enLeaveType LeaveType { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateLeaveDto
    {
        public int EmployeeId { get; set; }
        public int SubEmployeeId { get; set; }
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public enLeaveType LeaveType { get; set; }
    }
}
