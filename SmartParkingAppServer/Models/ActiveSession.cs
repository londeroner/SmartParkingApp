using System;
using System.ComponentModel.DataAnnotations;

namespace SmartParkingAppServer.Models
{
    public class ActiveSession
    {
        [Key]
        public int SessionId { get; set; }

        // Date and time of arriving at the parking
        public DateTime EntryDt { get; set; }


        // Date and time of payment for the parking
        public DateTime? PaymentDt { get; set; }


        // Date and time of exiting the parking
        public DateTime? ExitDt { get; set; }


        // Total cost of parking
        public decimal? TotalPayment { get; set; }


        // Plate number of the visitor's car
        public string CarPlateNumber { get; set; }


        // Issued printed ticket
        public string TicketNumber { get; set; }

        public ParkingUser User { get; set; }
        public bool Finished { get; set; }
    }
}
