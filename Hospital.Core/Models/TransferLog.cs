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
        [Required]
        [Key]
        public long Id { get; set; }
        public int EmployeeId { get; set; }
        public virtual Employee Employee { get; set; }
        public int OldDepartmentId { get; set; }
        public int NewDepartmentId { get; set; }
        public virtual Department Department { get; set; }
        public enShiftType OldShiftType { get; set; }
        public enShiftType NewShiftType { get;set; }
        public DateOnly TransferDate {  get; set; }
        public string AdOrderNumber { get; set; }
    }
}
