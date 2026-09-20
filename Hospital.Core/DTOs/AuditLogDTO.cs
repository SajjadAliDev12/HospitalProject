using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class AuditLogDTO
    {
        public long Id { get; set; }
        public DateTime Date { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string ActionType { get; set; } = null!;
        public string EntityName { get; set; } = null!;
        public string RecordId { get; set; } = null!;
    }
}
