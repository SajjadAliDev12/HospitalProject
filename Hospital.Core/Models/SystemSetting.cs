using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.Models
{
    public class SystemSetting
    {
        public int Id { get; set; }
        public DateOnly ShiftReferenceDate { get; set; }
    }
}
