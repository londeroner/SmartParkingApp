using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace SmartParkingAppServer.Models
{
    public class ParkingUser : IdentityUser<long>
    {
        public string Name { get; set; }
        public string CarPlateNumber { get; set; }
        public IList<ActiveSession> UActiveSessions { get; set; }
        public IList<PastSession> UPastSessions { get; set; }
    }
}
