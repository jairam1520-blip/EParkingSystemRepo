using ParkingSystem.Models;

using ParkingSystem.Utility;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

using MailKit.Net.Smtp;
using ParkingSystem.Models.EmailModels;
using MimeKit;
using System.Net.Mail;
using Microsoft.AspNetCore.Authorization;

namespace EParkingSystem.Controllers
{
    public class CustomerController : Controller
    {
        //Dependency injected
        UserManager<ApplicationUser> _userManager;
        SignInManager<ApplicationUser> _signInManager;
        RoleManager<IdentityRole> _roleManager;

        private readonly ApplicationDbContext _db;
        private readonly IEmailSender _emailSender; 
        


        public CustomerController(ApplicationDbContext db, UserManager<ApplicationUser> _userManager,
            SignInManager<ApplicationUser> _signInManager, RoleManager<IdentityRole> _roleManager,IEmailSender emailSender)
        {
            _db = db;
            this._userManager = _userManager;
            this._signInManager = _signInManager;
            this._roleManager = _roleManager;
            _emailSender = emailSender;
            
        }



        

        //Getting slots
        public IQueryable<Slot> GetTwoWheelerSlot()
        {
            return _db.Slots.Where(x => x.SlotType == "Two Wheeler");
        }

        //Welcome page for customer
        [HttpGet]
        public IActionResult CustomerHomePage()
        {
            return View();
        }



        //View All Booking made by currently logged in user
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public IActionResult MyBooking()
        {
            var bookings = _db.Bookings.Where(x => x.UserId == _userManager.GetUserId(User)).Include(s => s.slot);
            return View(bookings);
        }


       


        //New booking form
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public IActionResult NewBooking()
        {
            //Validation for new booking date and time
            //flag to indicate error in booking model
            if (TempData["DateTimeError"] != null)
            {
                ViewBag.spanError = "Invalid date time choosed!";
                ViewBag.DateTimeError = "You can book parking for minimum one hour!";
            }

            //used in view
            ViewBag.UserId = _userManager.GetUserId(User);
            return View();
        }

        //Receives the vehicle type,and slot time of booking and sends value to check for available slots
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult NewBookingPost(Booking model)
        {

            return RedirectToAction("CheckAvailibility", model);
        }

        //Receives values from NewBookingPost and and renders available and booked slots
        [HttpGet]
        [Authorize(Roles = "Customer")]
        public IActionResult CheckAvailibility(Booking model)
        {


           //Passing booking values to another controller to confirm the booking
            TempData["VehicleType"] = model.VehicleType;
            TempData["StartTime"] = model.StartDateTime;
            TempData["EndTime"] = model.EndDateTime;
            TempData["UserId"] = model.UserId;
            TimeSpan span = model.EndDateTime.Subtract(model.StartDateTime);

            //Slot start time and end time validation
            if ( model.StartDateTime.Date==DateTime.Now.Date && model.StartDateTime.Hour== DateTime.Now.Hour && model.StartDateTime.Minute == DateTime.Now.Minute)
            {
                //validation for difference in start and end time of booking
                if(span > TimeSpan.Zero)
                {
                    if (model.VehicleType == Helper.FourWheeler)
                    {
                        //used by view
                        ViewBag.SlotPresent = _db.Slots.Where(x => x.SlotType == "Four Wheeler");
                    }
                    else if (model.VehicleType == Helper.TwoWheeler)
                    {
                        //used by view
                        ViewBag.SlotPresent = _db.Slots.Where(x => x.SlotType == "Two Wheeler");
                    }

                    //used by view
                    ViewBag.VehicleType = model.VehicleType;


                    //fetching all the bookings made within the time slot choosed by user
                    var overlappedBooking = _db.Bookings.Where((x => x.StartDateTime == model.StartDateTime || x.EndDateTime == model.EndDateTime || (model.StartDateTime > x.StartDateTime && model.EndDateTime <= x.EndDateTime) ||
                    (model.StartDateTime >= x.StartDateTime && model.StartDateTime < x.EndDateTime && x.VehicleType == model.VehicleType)));


                    //fetching the slot ids of unavailable slots
                    var overlappedSlotId = new List<int>();
                    foreach (var slot in overlappedBooking)
                    {
                        overlappedSlotId.Add((int)slot.Sid);
                    }

                    //used by view
                    ViewBag.overlappedSlotId = overlappedSlotId;

                    return View(new Slot());
                }
                else
                {
                    //flag to indicate error in booking model
                    TempData["DateTimeError"] = 1;
                    return RedirectToAction("NewBooking");
                }
              
            }
            else
            {
                //flag to indicate error in booking model
                TempData["DateTimeError"] = 1;
                return RedirectToAction("NewBooking");
            }





        }





        //Confirm the booking by selecting available slots and sends confirmation email to user
        [HttpPost]
        [Authorize(Roles = "Customer")]
        public IActionResult CheckAvailibility(Slot selectedSlot)
        {
            //Receiving booking data from CheckAvailability using TempData
            var startTime = (DateTime)TempData["StartTime"];
            var endTime = (DateTime)TempData["EndTime"];


            Booking confirmedBooking = new Booking();
            confirmedBooking.Sid = selectedSlot.Sid;
            confirmedBooking.StartDateTime = startTime;
            confirmedBooking.EndDateTime = endTime;
            confirmedBooking.UserId = _userManager.GetUserId(User);
            confirmedBooking.VehicleType = (string)TempData["VehicleType"];


            //calculating bill amount for the time
            var span = endTime.Subtract(startTime);
            var minutes = span.TotalMinutes;


            confirmedBooking.BillAmount = Math.Truncate(minutes * 0.167);

            //save booking in db
            _db.Bookings.Add(confirmedBooking);
            _db.SaveChanges();

            //sending request to SendEmail controller to send confirmation email to user
            return RedirectToAction("SendEmail", new RouteValueDictionary(confirmedBooking));
        }
        

        //Get contact us form
        [Authorize(Roles ="Customer")]
        [HttpGet]
        public IActionResult ContactUs()
        {
            ContactUsModel model = new ContactUsModel();
            return View(model);
        }


        //save message in db
        [Authorize(Roles = "Customer")]
        public IActionResult ContactUsPost(ContactUsModel model)
        {
            model.Id = 0;
            if (ModelState.IsValid)
            {
                _db.ContactUsModel.Add(model);
                _db.SaveChanges();

                return View();
            }
            ModelState.AddModelError(string.Empty, "Please enter valid details");
            return RedirectToAction("ContactUs",model);
        }


        //Sends the email to customer on successfull booking
        public async Task<IActionResult> SendEmail(Booking booking)
        {
            // create email message
          
            var user = await _userManager.GetUserAsync(User);
            string emailTo = user.Email;

            var startTime = booking.StartDateTime;
            var endTime = booking.EndDateTime;
            var vehicleType = booking.VehicleType;
            var name = user.Name;

            var billAmount = booking.BillAmount;
            var slots = from s in _db.Slots where s.Sid == booking.Sid select s.SlotNumber;
            var slot = slots.FirstOrDefault();


            string subject = "Booking Confirmed";
            string body = "Congratulation your booking is confirmed." +Environment.NewLine +
                "Name:"+name+ Environment.NewLine +
                "Vehicle Type:" +vehicleType + Environment.NewLine +
                "Slot Number:"+slot+Environment.NewLine +
                "Start Time:" + startTime+ Environment.NewLine +
                "End Time:" +endTime+ Environment.NewLine +
                "Bill Amount:" +billAmount+ Environment.NewLine +
                "Thankyou for using our service." ;


          
          

            var message = new Message(new String[] {emailTo},subject,body);
            _emailSender.SendEmail(message);
            
            
            return RedirectToAction("MyBooking");
        }

        
    }
}
