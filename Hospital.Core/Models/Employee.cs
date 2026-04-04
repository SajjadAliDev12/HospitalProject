using Hospital.Core.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.Models
{
    public class Employee
    {
        
        public int Id { get; set; }
        [Required]
        [MaxLength(250)]
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly HireDate { get; set; }
        public int DepartmentId { get; set; }
        public enShiftType ShiftType { get; set; }
        public int JobTitleId { get; set; }
        public JobTitle JobTitle {  get; set; }
        public enGender Gender { get; set; }
        public enCertificate CertificateType { get; set; }
        public int LeaveBalance { get; set; }
        [MaxLength(500)]
        public string Address { get; set; }
        [MaxLength(20)]
        public string PhoneNumber { get; set; }
        public enJobStatus JobStatus { get; set; }
        public bool isDeleted { get; set; } = false;
        public int? LeaveCardNumber { get; set; }
        public Department Department { get; set; }
        public Collection<Leave> Leaves { get; set; }
        public Collection<Absent> Absents { get; set; }
        public enMorningShifts? enMorningGroup { get; set; }
        public int? NightShiftId { get; set; }
        public NightShiftTeam? nightShift {  get; set; }
    }
}
