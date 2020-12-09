using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SmartParkingAppServer.Models
{
    public class ResponseModel
    {
        public string Message { get; set; }
        public bool Succeded { get; set; }
        public object Data { get; set; }
    }
}
