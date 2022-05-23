
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ParkingSystem.Models
{




    public partial class Booking: IValidatableObject
    {
        [Key]
        public int Bid { get; set; }
        public string UserId { get; set; }

        [Required]
        [Display(Name ="Vehicle Type")]
        public string VehicleType { get; set; }

        [Required]
        [Display(Name = "Start DateTime")]
        public DateTime StartDateTime { get; set; }

        [Required]
        [Display(Name = "End DateTime")]
        public DateTime EndDateTime { get; set; }

        [Required]
        [Display(Name = "Bill Amount")]
        public double BillAmount { get; set; }

        
        [ForeignKey("slot")]
        public int Sid { get; set; }

        [Display(Name = "Slot")]
        public virtual Slot slot { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            List<ValidationResult> results = new List<ValidationResult>();

            if (StartDateTime < DateTime.Now)
            {
                results.Add(new ValidationResult("Start date and time must be greater than current time", new[] { "StartDateTime" }));
            }

            if (EndDateTime <= StartDateTime)
            {
                results.Add(new ValidationResult("EndDateTime must be greater that StartDateTime", new[] { "EndDateTime" }));
            }

            return results;
        }
    }
}



