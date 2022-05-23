using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.Models
{
    public class ContactUsModel
    {
        [Key]
        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is required!")]
        public string Name { get; set; } 
        
        [Required(ErrorMessage = "Email is required!")]
        [EmailAddress]
        public string Email { get; set; }

        [RegularExpression("^(?!0+$)(\\+\\d{1,3}[- ]?)?(?!0+$)\\d{10,15}$", ErrorMessage = "Please enter valid phone no.")]
        public string PhoneNumber { get; set; } 

        [Required(ErrorMessage ="Please enter your message!")]
        public string Message { get; set; }


    }
}
