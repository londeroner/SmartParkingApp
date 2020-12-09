using System;
using System.ComponentModel.DataAnnotations;

namespace SmartParkingAppServer.Models
{
    public class RefreshTokenModel
    {
        [Key]
        public int Id { get; set; }


        // The clientid, where it comes from
        [Required]
        public string ClientId { get; set; }


        // Value of the token
        [Required]
        public string Value { get; set; }


        // Token creation date
        [Required]
        public DateTime CreatedDate { get; set; }


        // The userid it was issued to
        [Required]
        public long UserId { get; set; }



        [Required]
        public DateTime LastModifiedDate { get; set; }


        [Required]
        public DateTime ExpiryTime { get; set; }
    }
}
