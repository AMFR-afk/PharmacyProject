using System.ComponentModel.DataAnnotations;

namespace PharmacyProject.Models
{
    public class ClientProp
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "الاسم الأول مطلوب")]
        public string Name { get; set; } = "";

        [Required(ErrorMessage = "اسم العائلة مطلوب")]
        public string Surname { get; set; } = "";

        [Required]
        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; } = "";

        [Required]
        [EmailAddress(ErrorMessage = "صيغة الإيميل غير صحيحة")]
        public string Email { get; set; } = "";

        
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [MinLength(6, ErrorMessage = "كلمة المرور يجب أن تكون 6 خانات على الأقل")]
        public string Password { get; set; } = "";

        public string Role { get; set; } = "USER";// تعيين الدور الافتراضي كـ "User"
    }
}