using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.Models
{
    public class Department
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; }

        public bool isDeleted { get; set; } = false;
        public int? ManagerId { get; set; }
        public Employee? Manager { get; set; }

        public DateOnly? ManagerStartDate { get; set; }

        [MaxLength(100)]
        public string? ManagerOrderNumber { get; set; }

        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
