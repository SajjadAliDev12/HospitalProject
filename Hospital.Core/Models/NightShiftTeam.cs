using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.Models
{
    public class NightShiftTeam
    {
        public int Id { get; set; }
        public int? SupervisorId { get; set; }
        public Employee? Supervisor {  get; set; }
        
    }
}
