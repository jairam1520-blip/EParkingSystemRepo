using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using ParkingSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using ParkingSystem.Models.EmailModels;
using Microsoft.AspNetCore.Authorization;

namespace EParkingSystem.Controllers
{
    public class AdminController : Controller
    {
        UserManager<ApplicationUser> _userManager;
        SignInManager<ApplicationUser> _signInManager;
        RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender;

        public AdminController(ApplicationDbContext db, UserManager<ApplicationUser> _userManager,
            SignInManager<ApplicationUser> _signInManager, RoleManager<IdentityRole> _roleManager,IEmailSender emailSender)
        {
            _db = db;
            this._userManager = _userManager;
            this._signInManager = _signInManager;
            this._roleManager = _roleManager;
            _emailSender = emailSender;
        }

        //Admins welcome page
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult Index()
        {

           
            return View();
        }
        //Display all bookings
        [HttpGet]
        [Authorize(Roles ="Admin")]
        public IActionResult ViewAllBooking()
        {
            var bookings = _db.Bookings.Include(s => s.slot).ToList();
            return View(bookings);
        }
        
        //Display all Users
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ViewAllUsers()
        {
            var users = (from user in _db.Users select new User(user.Name, user.Email)).ToList();

            return View(users);
        }

        //Display all slots
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult ViewAllSlots()
        {

            var slots = _db.Slots;
            return View(slots);
        }


        //Create slot form
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateSlot()
        {
            return View();
        }
        
        //Saves new slot to database
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateSlot(Slot slot)
        {
            if (ModelState.IsValid)
            {
                _db.Slots.Add(slot);
                _db.SaveChanges();
                return RedirectToAction("ViewAllSlots");
            }
            return NotFound();
        }


        //Edit slot form
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult EditSlot(int id)
        {
            var slot = _db.Slots.Find(id);
            return View(slot);
        }


        //Saves updated slot to database
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult EditSlotPost(Slot slot)
        {
            if (ModelState.IsValid)
            {
                _db.Slots.Update(slot);
                _db.SaveChanges();
                return RedirectToAction("ViewAllSlots");
            }
            return NotFound();
        }
        
        //Delete slot confirmation alert
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteSlot(int id)
        {
            var slot = _db.Slots.Find(id);
            return View(slot);
        }

        //Delete the slot from database
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteSlotPost(Slot slot)
        {

            var slotfetched = _db.Slots.Find(slot.Sid);
            if(slotfetched != null)
            {
                _db.Remove(slotfetched);
                _db.SaveChanges();
                return RedirectToAction("ViewAllSlots");

            }
            return NotFound();
        }


        //Delete booking confirmation alert
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteBooking(int id)
        {
            var booking = _db.Bookings.Find(id);
            var slot = _db.Slots.Find(booking.Sid);
            booking.slot = slot;

            if (booking != null)
            {
                return View(booking);
            }
            return NotFound();

        }

        //Delete booking from database
     
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteBookingPost(Booking b)
        {
            var booking = _db.Bookings.Find(b.Bid);

            if (booking != null)
            {
                _db.Bookings.Remove(booking);
                _db.SaveChanges();
                return RedirectToAction("ViewAllBooking");
            }

            return NotFound();


        }
        //Display all messages sent by customers
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public IActionResult GeAllMessages()
        {
            var messeges = _db.ContactUsModel;
            return View(messeges);
        }

        //Deletes the message sent by customer
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult DeleteMessagePost(ContactUsModel model)
        {
            var message = _db.ContactUsModel.Find(model.Id);
            if (message != null)
            {
                _db.ContactUsModel.Remove(message);
                _db.SaveChanges();
                return RedirectToAction("GeAllMessages");
            }
            return NotFound();
        }


        //Sends reply to customer in the form of email
        [Authorize(Roles = "Admin")]
        public IActionResult Reply(ContactUsModel model)
        {
            var msgFetched = _db.ContactUsModel.Find(model.Id);
            var emailTo = msgFetched.Email;
            string subject = "From E-Parking System team";
            string body = "Dear customer," + Environment.NewLine +
                "Thankyou for contacting us,how can we help you?";




            var message = new Message(new String[] { emailTo }, subject, body);
            _emailSender.SendEmail(message);
            return RedirectToAction("Index");
        }
    }
}
