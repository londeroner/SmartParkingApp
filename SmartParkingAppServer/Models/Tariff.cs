using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingAppServer.Models
{
    public class Tariff
    {
        [Key]
        public int TariffId { get; set; }
        public int Minutes { get; set; }
        public decimal Rate { get; set; }
    }
}
