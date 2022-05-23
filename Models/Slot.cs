using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.Models
{
    public class Slot
    {
        [Key]
        [Display(Name = "Slot Id")]
        [Required]
        public int Sid { get; set; }
        [Display(Name = "Slot Number")]
        [Required]
        public string SlotNumber { get; set; }
   
        [Required]
        [Display(Name = "Slot Type")]
        public string SlotType { get; set; }

    }
}
