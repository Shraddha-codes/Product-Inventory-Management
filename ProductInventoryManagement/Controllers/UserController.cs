using log4net;
using log4net.Config;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.JsonWebTokens;
using ProductInventoryManagement.Database;
using ProductInventoryManagement.Models;
using ProductInventoryManagement.Services;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace ProductInventoryManagement.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly AuthService _authService;
        private readonly ILog _log;
        public UserController(ApplicationDbContext context, AuthService authService)
        {
            _context = context;
            _authService = authService;


            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));
            ILog _logger1 = LogManager.GetLogger("UserController");
            _log = _logger1;
        }

        [HttpPost("signup")]
        public async Task<IActionResult> Signup([FromBody] SignupModel model)
        {
            _log.Error("Signup --> Starts");
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState); 
            }

            var existingUser = await _context.UserDetails.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (existingUser != null)
            {
                return Conflict("Username already exists. Please choose a different username.");
            }

            using var hmac = new HMACSHA512();
            var user = new UserDetails
            {
                Username = model.Username,
                Password = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(model.Password))),
                HashKey = Convert.ToBase64String(hmac.Key),
                Role = model.Role ?? "User" 
            };

            _context.UserDetails.Add(user);
            await _context.SaveChangesAsync();
            _log.Error("Signup --> Ends");
            return CreatedAtAction(nameof(Login), new { username = model.Username });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            _log.Error("Login --> Starts");
            var user = await _context.UserDetails.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null) return Unauthorized("User not found");

            using var hmac = new HMACSHA512(Convert.FromBase64String(user.HashKey));
            var hashedPassword = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(model.Password)));

            if (hashedPassword != user.Password) return Unauthorized("Invalid password");

            var token = _authService.GenerateJwtToken(user);
            _log.Error("Login --> Ends");
            return Ok(new { Token = token , role = user.Role});
        }

    }
}
