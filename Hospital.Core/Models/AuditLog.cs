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
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string? UserId { get; set; }
        public enAuditType Type { get; set; }
        public string EntityName { get; set; } 
        public string RecordId { get; set; }   

        public virtual ApplicationUser User { get; set; }
    }
}
