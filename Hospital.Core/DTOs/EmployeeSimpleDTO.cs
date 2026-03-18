using Hospital.Core.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class EmployeeSimpleDTO
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateOnly BirthDate { get; set; }
        public DateOnly HireDate { get; set; }
        public enGender Gender { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsDeleted { get; set; } = false;
        public int DepartmentID { get; set; }
    }
}
