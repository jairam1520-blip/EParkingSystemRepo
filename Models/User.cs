using System.ComponentModel.DataAnnotations;

namespace ParkingSystem.Models
{
    public class User
    {
        public User(string Name, string Email)
        {
          
            this.Name = Name;
            this.Email = Email;
        }
       
        public string Name { get; set; }
        [Key]
        public string Email { get; set; }
       
    }
}
