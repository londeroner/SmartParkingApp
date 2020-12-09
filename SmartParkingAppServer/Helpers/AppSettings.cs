using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingAppServer.Helpers
{
    public class AppSettings
    {
        //Properties for JWT Token
        public string Site { get; set; }
        public string Audience { get; set; }
        public string ExpireTime { get; set; }
        public string Secret { get; set; }
        public string OwnerSecret { get; set; }


        // Token Refresh Properties
        public string ClientId { get; set; }


        public int ParkingCapacity { get; set; }
        public int FreeLeavePeriod { get; set; }
    }
}
