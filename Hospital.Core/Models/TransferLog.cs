using Hospital.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.Models
{
    public class TransferLog
    {
       
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public  Employee Employee { get; set; } = null!;
        public int OldDepartmentId { get; set; }
        public Department OldDepartment { get; set; } = null!;
        public int NewDepartmentId { get; set; }
        public Department NewDepartment { get; set; } = null!;
        public enShiftType OldShiftType { get; set; }
        public enShiftType NewShiftType { get;set; }
        public DateOnly TransferDate {  get; set; }
        public string AdOrderNumber { get; set; } = null!;
    }
}
