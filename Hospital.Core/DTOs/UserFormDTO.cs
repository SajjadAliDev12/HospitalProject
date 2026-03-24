using System.ComponentModel.DataAnnotations;

namespace Hospital.Core.DTOs
{
    public class UserFormDTO
    {
        public string? Id { get; set; }

        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [StringLength(50, MinimumLength = 3)]
        public string UserName { get; set; }

        [Required(ErrorMessage = "الاسم الكامل مطلوب")]
        public string FullName { get; set; }


        public string? Password { get; set; }

        [Required(ErrorMessage = "يجب تحديد صلاحية للمستخدم")]
        public string Role { get; set; }

        // يسمح بقيمة null إذا كان الحساب غير مرتبط بموظف معين
        public int? EmployeeId { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;
    }
}