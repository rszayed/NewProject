using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Store.API.Data;
using Store.API.DTO;
using Store.API.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text.RegularExpressions;


namespace Store.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
       // private static List<User> UserList = new List<User>();
        private readonly AppSettings _appSettings;
        private readonly DataContext _context;
      

        public AuthController(IOptions< AppSettings>  _appSettings, DataContext context )
        {
           this._appSettings = _appSettings.Value;
            _context = context;
            
        }
        

        [HttpPost("login")]
        public async Task<IActionResult> Login (DTOLogin model)
        {
          //throw new Exception("Api Says NOOOOO!");
            var user =await _context.Users.FirstOrDefaultAsync(x=>x.Username== model.Username);
            if (user == null)
            {
                return BadRequest("UserName Or Password was Invalid");
            }
            var match = CheckPassword(model.Password!, user);

            if (!(bool)match)
            {
                return BadRequest("UserName Or Password was Invalid");
            }
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = System.Text.Encoding.ASCII.GetBytes(this._appSettings.Secret!);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] { new Claim("id", user.Username!) }),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials( new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha512Signature )
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var encrypterToken = tokenHandler.WriteToken(token);

            //return Ok(new {token= encrypterToken, username=user.Username});
           return Ok(new
           {
               token = tokenHandler.WriteToken(token)
           });
        }

        private object CheckPassword(string password, User user)
        {
            bool result;
            using (HMACSHA512? hmac = new HMACSHA512(user.PasswordSalt!))
            {
                var compute = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password!));
                result = compute.SequenceEqual(user.PasswordHash!);
            }
            return result;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(DTORegister model )
        {
          
          var dbuser=await _context.Users.FirstOrDefaultAsync(x=>x.Username == model.Username);
            if (dbuser !=null) return Conflict("записан уже");

            if (model.ConfirmPassword!=model.Password)return Conflict("порол не провильный") ;

            var user = new User {Username= model.Username};
            if (model.ConfirmPassword == model.Password)
            {
                using (HMACSHA512? hmac = new HMACSHA512())
                {
                    user.PasswordSalt = hmac.Key;
                    user.PasswordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(model.Password!));
                }
            }
            else if (model.Username == user.Username) return Ok(user.Username);
            //var user = new User { Username = model.Username };
            //if(model.Username== user.Username) { }



            var CreateUser = await _context.Users.AddAsync(user);
            var SaveUser =await _context.SaveChangesAsync();
            //return Ok(user);
            return StatusCode(201);
        }

    }
        
    
}

