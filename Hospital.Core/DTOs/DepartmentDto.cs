using Hospital.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class DepartmentDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsDeleted { get; set; }
        public int StaffCount { get; set; }
        public int MorningCount { get; set; }
        public int NightCount { get; set; }

        public int? ManagerId { get; set; }
        public string? ManagerName { get; set; } 
        public DateOnly? ManagerStartDate { get; set; }
        public string? ManagerOrderNumber { get; set; }
    }
}
