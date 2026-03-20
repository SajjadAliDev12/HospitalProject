using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hospital.Core.DTOs
{
    public class RegisterDTO
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public int? EmployeeId { get; set; }
    }
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; }

        [Required]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 رموز")]
        public string NewPassword { get; set; }

        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة غير متطابقة")]
        public string ConfirmPassword { get; set; }

    }
    public class AdminResetDto
    {
        [Required]
        public string UserName { get; set; } 

        [Required]
        [MinLength(6, ErrorMessage = "كلمة المرور الجديدة قصيرة جداً")]
        public string NewPassword { get; set; }
    }
    public class  UserViewDTO
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string FullName { get; set; }  
        public string Role { get; set; }      
        public int? EmployeeId { get; set; }  
        public string? EmployeeName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
