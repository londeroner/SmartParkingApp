using System.ComponentModel.DataAnnotations;

namespace SmartParkingAppServer.Models
{
    public class RegisterViewModel
    {
        [Required]
        [Display(Name = "Username")]
        public string Username { get; set; }


        [Required]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required]
        public string CarPlateNumber { get; set; }

        [Required]
        public string Phone { get; set; }
    }
}
