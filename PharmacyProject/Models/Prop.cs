using System.ComponentModel.DataAnnotations;
namespace PharmacyProject.Models
{
    public class Prop
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "يرجى إدخال اسم الدواء")]
        [Display(Name = "Medicine Name")] // هذا الاسم اللي رح يظهر في الـ HTML Label
        public string Name { get; set; } = "";

        [Display(Name = "Description")]
        public string Details { get; set; } = "";

        [Required]
        [Range(0.1, 1000, ErrorMessage = "السعر يجب أن يكون بين 0.1 و 1000")]
        [DataType(DataType.Currency)]
        public double Price { get; set; } // غيرنا لـ decimal لدقة الأسعار
    }
}
