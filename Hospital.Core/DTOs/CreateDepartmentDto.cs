using Hospital.Core.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class CreateDepartmentDto
    {
        public string Name { get; set; }
        public int? ManagerId { get; set; }
        public DateOnly? ManagerStartDate { get; set; }
        public string? ManagerOrderNumber { get; set; }
    }
}
