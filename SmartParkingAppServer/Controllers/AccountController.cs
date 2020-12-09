using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SmartParkingAppServer.Data;
using SmartParkingAppServer.Helpers;
using SmartParkingAppServer.Models;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.Extensions.Options;

namespace SmartParkingAppServer.Controllers
{
    [Route("api/[controller]")]
    public class AccountController : Controller
    {
        private readonly UserManager<ParkingUser> userManager;
        private readonly SignInManager<ParkingUser> signinManager;
        private readonly AppSettings appSettings;
        private readonly AppIdentityDbContext db;

        public AccountController(UserManager<ParkingUser> uM,
            SignInManager<ParkingUser> sM,
            AppIdentityDbContext _db,
            IOptions<AppSettings> apS
            )
        {
            userManager = uM;
            signinManager = sM;
            appSettings = apS.Value;
            db = _db;
        }


        /* REGISTER */
        #region REGISTER
        /***************************************************************************************************************/
        [HttpPost("[action]")]
        public async Task<IActionResult> Register([FromBody] RegisterViewModel formdata)
        {
            List<string> errorList = new List<string>();

            ParkingUser user = new ParkingUser
            {
                Email = "",
                PhoneNumber = formdata.Phone,
                PhoneNumberConfirmed = true,
                UserName = formdata.Username,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true,
                CarPlateNumber = formdata.CarPlateNumber
            };

            // Try to create user
            IdentityResult result = await userManager.CreateAsync(user, formdata.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "CommonUser");

                ResponseModel ret = new ResponseModel
                {
                    Message = "Register Successful",
                    Succeded = true,
                    Data = null
                };
                return Ok(ret);
            }
            else
            {
                foreach (IdentityError error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                    errorList.Add(error.Description);
                }
            }

            return BadRequest(new JsonResult(errorList));
        }


        [HttpPost("[action]")]
        public async Task<IActionResult> RegisterOwner([FromBody] RegisterOwnerViewModel formdata)
        {
            List<string> errorList = new List<string>();

            if (formdata.Secret != appSettings.OwnerSecret)
            {
                return BadRequest(new ResponseModel
                {
                    Message = "Invalid details supplied",
                    Succeded = false,
                    Data = null
                });
            }

            ParkingUser user = new ParkingUser
            {
                PhoneNumber = formdata.Phone,
                UserName = formdata.Username,
                SecurityStamp = Guid.NewGuid().ToString(),
                EmailConfirmed = true
            };


            // Try to create user
            IdentityResult result = await userManager.CreateAsync(user, formdata.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "Owner");

                ResponseModel ret = new ResponseModel
                {
                    Message = "Ok",
                    Succeded = true,
                    Data = null
                };
                return Ok(ret);
            }

            return BadRequest(new ResponseModel
            {
                Message = "Invalid details supplied",
                Succeded = false,
                Data = null
            });
        }
        #endregion REGISTER
        /***************************************************************************************************************/




        /* LOGIN */
        #region LOGIN
        /***************************************************************************************************************/
        [HttpPost("[action]")]
        public async Task<IActionResult> Login([FromBody] LoginModel formData)
        {
            ParkingUser usr = await userManager.FindByNameAsync(formData.Username);
            IList<string> roles = await userManager.GetRolesAsync(usr);

            if (usr != null && await userManager.CheckPasswordAsync(usr, formData.Password))
            {
                JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
                SecurityToken t = GenerateAccessToken(formData.Username, usr.Id, roles, tokenHandler);
                RefreshTokenModel rt = CreateRefreshToken(usr.Id);
                await db.RefreshTokenModels.AddAsync(rt);
                await db.SaveChangesAsync();

                return Ok(new ResponseModel
                {
                    Message = "Ok",
                    Succeded = true,
                    Data = new
                    {
                        acces_token = tokenHandler.WriteToken(t),
                        refresh_token = rt.Value,
                        expiration = t.ValidTo,
                        username = usr.UserName,
                        userrole = roles.FirstOrDefault(),
                        car_plate_number = usr.CarPlateNumber,
                        phone_number = usr.PhoneNumber
                    }
                });
            }
            else
            {
                return BadRequest(new ResponseModel
                {
                    Message = "Invalid details supplied",
                    Succeded = true,
                    Data = null
                });
            }
        }
        /***************************************************************************************************************/
        #endregion

        [HttpPost("[action]")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestData data)
        {
            RefreshTokenModel rt = db.RefreshTokenModels.FirstOrDefault(
                t => t.ClientId == appSettings.ClientId && t.Value == data.RefreshToken);

            if (rt == null)
            {
                return Unauthorized(new ResponseModel
                {
                    Message = "Invalid refresh token",
                    Succeded = false,
                    Data = null
                });
            }

            if (rt.ExpiryTime < DateTime.UtcNow)
            {
                return Unauthorized(new ResponseModel
                {
                    Message = "The validity of the token has ended",
                    Succeded = false,
                    Data = null
                });
            }

            ParkingUser user = userManager.Users.FirstOrDefault(u => u.Id == rt.UserId);
            RefreshTokenModel rtNew = CreateRefreshToken(user.Id);

            db.RefreshTokenModels.Remove(rt);
            await db.RefreshTokenModels.AddAsync(rtNew);
            await db.SaveChangesAsync();

            IList<string> roles = await userManager.GetRolesAsync(user);
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();
            SecurityToken tk = GenerateAccessToken(user.UserName, user.Id, roles, tokenHandler);

            return Ok(new ResponseModel
            {
                Message = "Ok",
                Succeded = true,
                Data = new
                {
                    acces_token = tokenHandler.WriteToken(tk),
                    refresh_token = rt.Value,
                    expiration = tk.ValidTo,
                    username = user.UserName,
                    userrole = roles.FirstOrDefault()
                }
            });
        }


        private RefreshTokenModel CreateRefreshToken(long userId)
        {
            double rtLifetime = Convert.ToDouble(appSettings.ExpireTime);

            RefreshTokenModel rt = new RefreshTokenModel
            {
                ClientId = appSettings.ClientId,
                CreatedDate = DateTime.UtcNow,
                ExpiryTime = DateTime.UtcNow.AddMinutes(rtLifetime),
                UserId = userId,
                Value = Guid.NewGuid().ToString("N")
            };

            return rt;
        }


        public SecurityToken GenerateAccessToken(string userIdentifier, long userId, IList<string> roles, JwtSecurityTokenHandler tokenHandler)
        {
            // Сonfiguring json web token
            SymmetricSecurityKey key = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(appSettings.Secret));
            double tokenExpiryTime = Convert.ToDouble(appSettings.ExpireTime);

            DateTime expTime = DateTime.UtcNow;

            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, userIdentifier),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Role, roles.FirstOrDefault()),
                    new Claim("LoggedIn", expTime.ToString())
                }),

                SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
                Issuer = appSettings.Site,
                Audience = appSettings.Audience,
                Expires = expTime.AddMinutes(tokenExpiryTime)
            };


            // Generate token
            SecurityToken token = tokenHandler.CreateToken(tokenDescriptor);
            return token;
        }



        [HttpPost("[action]")]
        public async Task<IActionResult> Logout([FromBody] LogoutModel lOutModel)
        {
            ParkingUser usr = db.Users.FirstOrDefault(u => u.UserName == lOutModel.username);

            if (usr == null)
            {
                return BadRequest(new ResponseModel
                {
                    Message = "Cannot find specified user",
                    Succeded = false,
                    Data = null
                });
            }


            var refreshTokens = db.RefreshTokenModels.Where(rtm => rtm.UserId == usr.Id);
            db.RefreshTokenModels.RemoveRange(refreshTokens);
            await db.SaveChangesAsync();

            return BadRequest(new ResponseModel
            {
                Message = "Ok",
                Succeded = true,
                Data = null
            });
        }
    }
}