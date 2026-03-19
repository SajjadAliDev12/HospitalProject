using Hospital.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class EmployeeFullDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly HireDate { get; set; }
        public int DepartmentId { get; set; }
        public int ShiftType { get; set; }
        public int JobTitleId { get; set; }
        public int Gender { get; set; }
        public int CertificateType { get; set; }
        public int LeaveBalance { get; set; }
        public string Address { get; set; }
        public string PhoneNumber { get; set; }
        public int JobStatus { get; set; }
        public bool IsDeleted { get; set; }
    }
}
