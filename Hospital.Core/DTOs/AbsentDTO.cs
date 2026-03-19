using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class AbsentFullDto
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
        public bool IsDeleted { get; set; }
    }

    public class CreateAbsentDto
    {
        public int EmployeeId { get; set; }
        public DateTime Date { get; set; }
    }
}
