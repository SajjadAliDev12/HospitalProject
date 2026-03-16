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
        [Required]
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(100)]
        public string Name { get; set; }
        public bool isDeleted { get; set; } = false;
        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
