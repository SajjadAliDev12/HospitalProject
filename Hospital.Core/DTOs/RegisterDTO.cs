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
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        public string UserName { get; set; } = null!;
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 رموز")]
        public string Password { get; set; } = null!;
        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        public string FullName { get; set; } = null!;
        public int? EmployeeId { get; set; }
    }
    public class ChangePasswordDto
    {
        [Required]
        public string OldPassword { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب ألا تقل عن 6 رموز")]
        public string NewPassword { get; set; } = null!;

        [Compare("NewPassword", ErrorMessage = "كلمة المرور الجديدة غير متطابقة")]
        public string ConfirmPassword { get; set; } = null!;

    }
    public class AdminResetDto
    {
        [Required]
        public string UserName { get; set; } = null!;

        [Required]
        [MinLength(6, ErrorMessage = "كلمة المرور الجديدة قصيرة جداً")]
        public string NewPassword { get; set; } = null!;
    }
    public class  UserViewDTO
    {
        public string Id { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string FullName { get; set; } = null!;
        public string Role { get; set; } = null!;
        public int? EmployeeId { get; set; }  
        public string? EmployeeName { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
