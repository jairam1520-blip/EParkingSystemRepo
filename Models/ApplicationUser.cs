using Microsoft.AspNetCore.Identity;

namespace ParkingSystem.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string Name { get; set; }
    }
}