using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{

    public class LeaveFullDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public int SubEmployeeId { get; set; }
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int LeaveType { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateLeaveDto
    {
        public int EmployeeId { get; set; }
        public int SubEmployeeId { get; set; }
        public int Duration { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public int LeaveType { get; set; }
    }
}