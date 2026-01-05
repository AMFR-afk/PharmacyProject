using System.ComponentModel.DataAnnotations;

namespace PharmacyProject.Models
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "الإيميل مطلوب")]
        [EmailAddress]
        public string Email { get; set; }

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        public string Password { get; set; }
    }
}