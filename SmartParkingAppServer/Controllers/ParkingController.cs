using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SmartParkingAppServer.Data;
using SmartParkingAppServer.Helpers;
using SmartParkingAppServer.Models;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.Extensions.Options;

namespace SmartParkingAppServer.Controllers
{
    [Route("api/[controller]")]
    public class ParkingController : Controller
    {
        private readonly UserManager<ParkingUser> userManager;
        private readonly AppSettings appSettings;
        private readonly AppIdentityDbContext db;

        public ParkingController(UserManager<ParkingUser> uM,
            IOptions<AppSettings> apS,
            AppIdentityDbContext _db)
        {
            userManager = uM;
            appSettings = apS.Value;
            db = _db;
        }


        [HttpPost("[action]")]
        //[Authorize(Policy = "RequireLoggedIn", Roles = "CommonUser")]
        public async Task<IActionResult> EnterParking([FromBody] ParkingViewModel spvm)
        {
            ActiveSession session = db.ActiveSessions
                .FirstOrDefault(s => s.CarPlateNumber == spvm.CarPlateNumber);

            if (db.ActiveSessions.Count() >= appSettings.ParkingCapacity || session != null)
                return BadRequest(new ResponseModel
                {
                    Message = "Can't enter parking. Try later.",
                    Succeded = false,
                    Data = null
                });
            ParkingUser usr = db.Users.FirstOrDefault(u => u.CarPlateNumber == spvm.CarPlateNumber);
            ActiveSession lastSession = db.ActiveSessions.OrderBy(s => s.TicketNumber).FirstOrDefault();
            PastSession pastLastSession = db.PastSessions.OrderBy(s => s.TicketNumber).FirstOrDefault();


            session = new ActiveSession
            {
                CarPlateNumber = spvm.CarPlateNumber,
                EntryDt = DateTime.Now,
                TicketNumber = Guid.NewGuid().ToString(),
                User = usr
            };

            await db.ActiveSessions.AddAsync(session);
            await db.SaveChangesAsync();

            session.User = null;
            return Ok(new ResponseModel
            {
                Message = "OK",
                Succeded = true,
                Data = session
            });
        }


        [HttpGet("[action]")]
        //[Authorize(Policy = "RequireLoggedIn", Roles = "CommonUser")]
        public IActionResult GetRemainingCost([FromQuery] string userName)
        {
            DateTime currentDt = DateTime.Now;
            ActiveSession session = db.ActiveSessions.FirstOrDefault(s => s.User.UserName == userName);
            if (session == null)
                return BadRequest(new ResponseModel
                {
                    Message = "Can't find session with supplied Username",
                    Succeded = false,
                    Data = null
                });

            double diff = (currentDt - (session.PaymentDt ?? session.EntryDt)).TotalMinutes;

            decimal amount = GetCost(diff);
            return Ok(new ResponseModel
            {
                Message = "Ok",
                Succeded = true,
                Data = amount
            });
        }


        [HttpPost("[action]")]
        //[Authorize(Policy = "RequireLoggedIn", Roles = "CommonUser")]
        public async Task<IActionResult> PayForParking([FromBody] ParkingViewModel pkvm)
        {
            DateTime currentDt = DateTime.Now;
            ActiveSession session = db.ActiveSessions.FirstOrDefault(s => s.CarPlateNumber == pkvm.CarPlateNumber);
            double diff = (currentDt - (session.PaymentDt ?? session.EntryDt)).TotalMinutes;
            decimal amount = GetCost(diff);
            session.TotalPayment = (session.TotalPayment ?? 0) + amount;
            session.PaymentDt = DateTime.Now;
            db.ActiveSessions.Update(session);
            await db.SaveChangesAsync();

            return Ok(new ResponseModel
            {
                Message = "Ok",
                Succeded = true,
                Data = null
            });
        }


        [HttpPost("[action]")]
        //[Authorize(Policy = "RequireLoggedIn", Roles = "CommonUser")]
        public async Task<IActionResult> TryLeaveParkingByCarPlateNumber([FromBody] ParkingViewModel pkvm)
        {
            ActiveSession session = db.ActiveSessions.FirstOrDefault(s => s.CarPlateNumber == pkvm.CarPlateNumber);
            if (session == null)
                return BadRequest(new ResponseModel
                {
                    Message = "Can't find session with supplied CarPlateNumber",
                    Succeded = false,
                    Data = null
                });

            DateTime currentDt = DateTime.Now;

            if (session.PaymentDt != null)
            {
                if ((currentDt - session.PaymentDt.Value).TotalMinutes <= appSettings.FreeLeavePeriod)
                {
                    await CompleteSession(session, currentDt);
                    return Ok(new ResponseModel
                    {
                        Message = "Seccussfully leaved",
                        Succeded = true,
                        Data = null
                    });
                }
                else
                {
                    session = null;
                    return BadRequest(new ResponseModel
                    {
                        Message = "You must pay before leaving parking",
                        Succeded = false,
                        Data = null
                    });
                }
            }
            else
            {
                // No payment, within free leave period -> allow exit
                if ((currentDt - session.EntryDt).TotalMinutes <= appSettings.FreeLeavePeriod)
                {
                    session.TotalPayment = 0;
                    await CompleteSession(session, currentDt);
                    return Ok(new ResponseModel
                    {
                        Message = "Success",
                        Succeded = true,
                        Data = null
                    });
                }
                else
                {
                    session.TotalPayment = GetCost((currentDt - session.EntryDt).TotalMinutes - appSettings.FreeLeavePeriod);
                    session.PaymentDt = currentDt;
                    await CompleteSession(session, currentDt);
                    return Ok(new ResponseModel
                    {
                        Message = "Successfully leaved",
                        Succeded = true,
                        Data = null
                    });
                }
            }
        }

        [HttpGet("[action]")]
        //[Authorize(Policy = "RequireLoggedIn", Roles = "CommonUser")]
        public IActionResult GetPastSessionsForUser([FromQuery] long userId)
        {
            IEnumerable<PastSession> query = db.PastSessions.Where(t => t.User.Id == userId);
            return Ok(new ResponseModel
            {
                Message = "Successfull",
                Succeded = true,
                Data = query
            });
        }


        [HttpPost("[action]")]
        //[Authorize(Policy = "RequireLoggedIn", Roles = "CommonUser")]
        public async Task<IActionResult> TryLeaveParking([FromBody] ParkingViewModel pkvm)
        {
            DateTime currentDt = DateTime.Now;
            ActiveSession session = db.ActiveSessions.FirstOrDefault(s => s.CarPlateNumber == pkvm.CarPlateNumber);
            if (session == null)
                return BadRequest(new ResponseModel
                {
                    Message = "Can't find parking session with supplied ticket number",
                    Succeded = false,
                    Data = null
                });

            double diff = (currentDt - (session.PaymentDt ?? session.EntryDt)).TotalMinutes;
            if (diff <= appSettings.FreeLeavePeriod)
            {
                session.TotalPayment = 0;
                await CompleteSession(session, currentDt);
                return Ok(new ResponseModel
                {
                    Message = "Successfully leaved",
                    Succeded = true,
                    Data = null
                });
            }
            else
            {
                session = null;
                return BadRequest(new ResponseModel
                {
                    Message = "You must pay for parking before leav",
                    Succeded = false,
                    Data = null
                });
            }

        }
        [HttpGet("[action]")]
        //[Authorize(Policy = "RequireLoggedIn", Roles = "CommonUser")]
        public IActionResult GetActiveSessionForUser([FromQuery] string username)
        {
            ActiveSession session = db.ActiveSessions.FirstOrDefault(s => s.User.UserName == username);

            if (session != null)
            {
                return Ok(new ResponseModel
                {
                    Message = "Ok",
                    Succeded = true,
                    Data = session
                });
            }
            else
            {
                return BadRequest(new ResponseModel
                {
                    Message = "Can't find active session for user with username: " + username + ".",
                    Succeded = false,
                    Data = null
                });
            }
        }



        [HttpGet("[action]")]
        //[Authorize(Policy = "RequireLoggedIn", Roles = "CommonUser")]
        public IActionResult GetCompletedSessionsForUser([FromQuery] string userName)
        {
            ParkingUser usr = db.Users.FirstOrDefault(u => u.UserName == userName);
               
            IList<PastSession> sessions = db.PastSessions.Where(s => s.CarPlateNumber == usr.CarPlateNumber).ToList();

            return Ok(new ResponseModel
            {
                Message = "Seccessful",
                Succeded = true,
                Data = sessions.ToList()
            });
        }




        [HttpGet("[action]")]
        //[Authorize(Roles = "Owner")]
        public IActionResult GetSessionsInPeriod([FromQuery] DateTime since, [FromQuery] DateTime until)
        {
            List<object> allSessions = new List<object>();

            IEnumerable<PastSession> pastSessions = db.PastSessions.Where(s => s.EntryDt >= since && s.ExitDt <= until);
            allSessions.AddRange(pastSessions);
            IEnumerable<ActiveSession> actSessions = db.ActiveSessions.Where(s => s.EntryDt >= since);
            allSessions.AddRange(actSessions);

            return Ok(new ResponseModel
            {
                Message = "Successful",
                Succeded = true,
                Data = allSessions
            });

        }


        [HttpGet("[action]")]
        //[Authorize(Roles = "Owner")]
        public IActionResult GetPayedSessionsInPeriod([FromQuery]DateTime since, [FromQuery]DateTime until)
        {
            List<object> result = new List<object>();
            IEnumerable<PastSession> fromPast = db.PastSessions.Where(s => s.PaymentDt >= since && s.PaymentDt <= until);

            result.AddRange(fromPast);

            IQueryable<ActiveSession> activeQuery = db.ActiveSessions.Where(s => s.PaymentDt != null);
            IEnumerable<ActiveSession> active = activeQuery.Where(s => s.PaymentDt >= since && s.PaymentDt <= until);

            result.AddRange(active);

            return Ok(new ResponseModel
            {
                Message = "Successful",
                Succeded = true,
                Data = result
            });
        }


        [HttpGet("[action]")]
        //[Authorize(Roles = "Owner")]
        public IActionResult GetActiveSesstionsForOwner()
        {
            return Ok(new ResponseModel
            {
                Message = "Seccessful",
                Succeded = true,
                Data = db.ActiveSessions.ToList()
            });
        }



        [HttpGet("[action]")]
        //[Authorize(Roles = "Owner")]
        public IActionResult GetPastSesstionsForOwner()
        {
            return Ok(new ResponseModel
            {
                Message = "Seccessful",
                Succeded = true,
                Data = db.PastSessions.ToList()
            });
        }


        [HttpGet("[action]")]
        //[Authorize(Roles = "Owner")]
        public IActionResult GetPercentageofOccupiedSpace()
        {
            double taken = db.ActiveSessions.Count();
            double rate = taken / appSettings.ParkingCapacity;
            rate *= 100;

            return Ok(new ResponseModel
            {
                Message = "Seccessful",
                Succeded = true,
                Data = rate
            });
        }

        [HttpGet("[action]")]
        //[Authorize(Roles = "Owner")]
        public IActionResult GetTariffs()
        {
            return Ok(new ResponseModel
            {
                Message = "Seccessful",
                Succeded = true,
                Data = db.Tariffs.ToList()
            });
        }



        private async Task<int> CompleteSession(ActiveSession session, DateTime currentDt)
        {
            session.ExitDt = currentDt;

            db.ActiveSessions.Remove(session);
            PastSession pSession = new PastSession
            {
                EntryDt = session.EntryDt,
                PaymentDt = session.PaymentDt,
                ExitDt = session.ExitDt,
                TotalPayment = session.TotalPayment,
                CarPlateNumber = session.CarPlateNumber,
                TicketNumber = session.TicketNumber,
                User = session.User,
                Finished = true
            };
            db.PastSessions.Add(pSession);
            return await db.SaveChangesAsync();
        }
        private decimal GetCost(double diffInMinutes)
        {
            Tariff tariff = db.Tariffs.FirstOrDefault(t => t.Minutes >= diffInMinutes) ?? db.Tariffs.OrderBy(t =>t.TariffId).FirstOrDefault();
            return tariff.Rate;
        }
    }
}
