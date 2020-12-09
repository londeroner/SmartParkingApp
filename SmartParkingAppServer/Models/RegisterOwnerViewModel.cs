using System.ComponentModel.DataAnnotations;

namespace SmartParkingAppServer.Models
{
    public class RegisterOwnerViewModel
    {
        [Required]
        public string Phone { get; set; }


        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string Secret { get; set; }
    }
}
