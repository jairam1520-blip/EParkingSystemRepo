using Microsoft.AspNetCore.Mvc.Rendering;

namespace ParkingSystem.Utility
{
    public class Helper
    {
        public static string Admin = "Admin";
        public static string Customer = "Customer";

        public static string TwoWheeler = "Two Wheeler";
        public static string FourWheeler = "Four Wheeler";
        public static List<SelectListItem> GetRolesForDropDown()
        {
            return new List<SelectListItem>
            {
               
                new SelectListItem{Value=Helper.Customer,Text=Helper.Customer },
                new SelectListItem{Value=Helper.Admin,Text=Helper.Admin },

            };
        }
        public static List<SelectListItem> GetVehicleType()
        {
            return new List<SelectListItem>
            {
                new SelectListItem{Value=Helper.FourWheeler,Text=Helper.FourWheeler },
                new SelectListItem{Value=Helper.TwoWheeler,Text=Helper.TwoWheeler }

            };
        }
    }
}
