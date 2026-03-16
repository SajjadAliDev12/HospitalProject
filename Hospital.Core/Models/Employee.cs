using Hospital.Core.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.Models
{
    public class Employee
    {
        [Required]
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly HireDate { get; set; }
        public int DepartmentId { get; set; }
        public enShiftType ShiftType { get; set; }
        public int JobTitle { get; set; }
        public enGender Gender { get; set; }
        public int CertificateType { get; set; }
        public int LeaveBalance { get; set; }
        [MaxLength(500)]
        public string Address { get; set; }
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        public enJobStatus JobStatus { get; set; }
        public bool isDeleted { get; set; } = false;
        public virtual Department Department { get; set; }
    }
}
