using Hospital.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class TransferLogDto
    {
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = null!;
        public int OldDepartmentId { get; set; }
        public int NewDepartmentId { get; set; }
        public enShiftType OldShiftType { get; set; }
        public enShiftType NewShiftType { get; set; }
        public DateOnly TransferDate { get; set; }
        public string AdOrderNumber { get; set; } = null!;
        public string OldDepartmentName { get; set; } = null!;
        public string NewDepartmentName { get; set; } = null!;
    }

    public class CreateTransferLogDto
    {
        public int EmployeeId { get; set; }
        public int NewDepartmentId { get; set; }
        public enShiftType NewShiftType { get; set; }
        public DateOnly TransferDate { get; set; }
        public string AdOrderNumber { get; set; } = null!;
    }
    
}
