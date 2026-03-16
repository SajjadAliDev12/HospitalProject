using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hospital.Core.Enums;

namespace Hospital.Core.Models
{
    public class AuditLog
    {
        [Required]
        [Key]
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; }
        public enAuditType Type { get; set; }

    }
}
