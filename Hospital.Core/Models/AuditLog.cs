using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.Models
{
    public class AuditLog
    {
        [Required]
        [Key]
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public int UserId { get; set; }
        public string Type { get; set; }

    }
}
