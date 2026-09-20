using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class JobTitleDTO
    {
        public string Title { get; set; } = null!;
    }

    public class JobTitleViewDTO
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
    }
}
