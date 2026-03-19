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
        public string EmployeeName { get; set; }
        public int OldDepartmentId { get; set; }
        public int NewDepartmentId { get; set; }
        public int OldShiftType { get; set; }
        public int NewShiftType { get; set; }
        public DateOnly TransferDate { get; set; }
        public string AdOrderNumber { get; set; }
    }

    public class CreateTransferLogDto
    {
        public int EmployeeId { get; set; }
        public int NewDepartmentId { get; set; }
        public int NewShiftType { get; set; }
        public DateOnly TransferDate { get; set; }
        public string AdOrderNumber { get; set; }
    }
}
