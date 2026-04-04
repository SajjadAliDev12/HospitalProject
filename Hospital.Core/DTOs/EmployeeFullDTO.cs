using Hospital.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class EmployeeFullDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [JsonPropertyName("birthDate")]
        public DateOnly BirthDate { get; set; }
        [JsonPropertyName("hireDate")]
        public DateOnly HireDate { get; set; }
        public int DepartmentId { get; set; }
        public enShiftType ShiftType { get; set; }
        public int JobTitleId { get; set; }
        public enGender Gender { get; set; }
        public enCertificate CertificateType { get; set; }
        public int LeaveBalance { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public enJobStatus JobStatus { get; set; }
        public bool IsDeleted { get; set; }
        public int? LeaveCardNumber { get; set; }
        public int? NightShiftId { get; set; }
        public enMorningShifts? enMorningShifts { get; set; }
    }
}
