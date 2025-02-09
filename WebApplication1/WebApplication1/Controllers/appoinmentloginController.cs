
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Formats.Asn1;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WebApplication1.Data;

namespace webapplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppoinmentLoginController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;

        public AppoinmentLoginController(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        /*[HttpPost("LoginWithOtp")]
        public IActionResult LoginWithOtp([FromBody] AppoinmentLoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.email))
            {
                return BadRequest("Invalid login request");
            }

            // Check if the user exists in the database
            var user = _context.MED_PATIENTS_DETAILS.FirstOrDefault(u => u.MPD_EMAIL == login.email);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.MPD_EMAIL),
        new Claim(ClaimTypes.Role, "patient")
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new

            {
                Message = "Login successful",
                Role = "patient",
                Token = tokenString,
                Email = login.email,
                PatientCode = user.MPD_PATIENT_CODE,
                Name = user.MPD_PATIENT_NAME,
                Contact = user.MPD_MOBILE_NO
            });
        }*/


        [HttpPost("LoginWithOtp")]
        public IActionResult LoginWithOtp([FromBody] AppoinmentLoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.contact))
            {
                return BadRequest("Invalid login request");
            }

            // Check if the user exists in the database
            var user = _context.MED_PATIENTS_DETAILS.FirstOrDefault(u => u.MPD_MOBILE_NO == login.contact);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            var claims = new[]
            {
         new Claim(ClaimTypes.Name, user.MPD_MOBILE_NO),
         new Claim(ClaimTypes.Role, "patient")
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Message = "Login successful",
                Role = "patient",
                Token = tokenString,
                Email = user.MPD_EMAIL,
                PatientCode = user.MPD_PATIENT_CODE,
                Name = user.MPD_PATIENT_NAME,
                Contact = user.MPD_MOBILE_NO
            });
        }
        [HttpPost("userlists")]
        public IActionResult UserLists([FromBody] AppoinmentLoginModel login)
        {
           
            if (login == null || string.IsNullOrEmpty(login.contact))
            {
                return BadRequest("Invalid credentials.");
            }

            // Query the database for the user with the given contact
            var users = _context.MED_PATIENTS_DETAILS
                .Where(u => u.MPD_MOBILE_NO == login.contact)
                .ToList();

          
            if (users == null || users.Count == 0)
            {
                return NotFound("No users found with the given contact.");
            }

           
            return Ok(users);
        }









        /*[HttpPost("CheckUserExists")]
        public IActionResult CheckUserExists([FromBody] AppoinmentLoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.email))
            {
                return BadRequest("Email is required");
            }

            // Check if the user exists in the database
            var userExists = _context.MED_PATIENTS_DETAILS.Any(u => u.MPD_EMAIL == login.email);

            if (!userExists)
            {
                return NotFound("User not registered");
            }

            return Ok("User found");
        }*/
        [HttpPost("CheckUserExists")]
        public IActionResult CheckUserExists([FromBody] AppoinmentLoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.contact))
            {
                return BadRequest("contact is required");
            }

            // Check if the user exists in the database
            var userExists = _context.MED_PATIENTS_DETAILS.Any(u => u.MPD_MOBILE_NO == login.contact);

            if (!userExists)
            {
                return NotFound("User not registered");
            }

            return Ok("User found");
        }

        [HttpPost("patient-login-contact")]
        public async Task<IActionResult> Appoinmentlogin(AppoinmentLoginModel login)
        {
            // Validate input
            if (string.IsNullOrEmpty(login.contact))
            {
                return BadRequest(new { message = "Contact number is required." });
            }

            // Check if the user exists in the database
            var user = await _context.MED_PATIENTS_DETAILS
                                      .FirstOrDefaultAsync(u => u.MPD_MOBILE_NO == login.contact);

            if (user == null)
            {
                return Unauthorized(new { message = "Invalid contact number." });
            }

            // Generate a 6-digit OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Construct the SMS API URL
            string smsApiUrl = $"https://esystems.cdl.lk/Backend/SMSGateway/api/SMS/DTSSendMessage?mobileNo={login.contact}&message=Your OTP is {otp}";

            // Send the OTP via the SMS API
            using (var httpClient = new HttpClient())
            {
                var response = await httpClient.GetAsync(smsApiUrl);
                if (!response.IsSuccessStatusCode)
                {
                    return StatusCode((int)response.StatusCode, new { message = "Failed to send OTP." });
                }
            }

            // Return the OTP in the response for verification (REMOVE IN PRODUCTION)
            return Ok(new
            {
                otpcode = otp,
                message = "OTP sent successfully."
            });
        }


        /* [HttpPost("patient-login-api")]
         public async Task<IActionResult> loginpatient(AppoinmentLoginModel login)
         {


             if (string.IsNullOrEmpty(login.patientcode))
             {



                 return BadRequest("Invalid credentials");
             }


             var user = _context.MED_PATIENTS_DETAILS.FirstOrDefault(u => u.MPD_PATIENT_CODE == login.patientcode);

             if (user == null)
             {
                 return Unauthorized("Invalid credentials");
             }

             // Generate JWT token
             var tokenHandler = new JwtSecurityTokenHandler();
             var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

             var claims = new[]
             {
          new Claim(ClaimTypes.Name, user.MPD_MOBILE_NO),
          new Claim(ClaimTypes.Role, "patient")
     };

             var tokenDescriptor = new SecurityTokenDescriptor
             {
                 Subject = new ClaimsIdentity(claims),
                 Expires = DateTime.UtcNow.AddHours(1),
                 SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
             };

             var token = tokenHandler.CreateToken(tokenDescriptor);
             var tokenString = tokenHandler.WriteToken(token);

             return Ok(new
             {
                 Message = "Login successful",
                 Role = "patient",
                 Token = tokenString,
                 Email = user.MPD_EMAIL,
                 PatientCode = user.MPD_PATIENT_CODE,
                 Name = user.MPD_PATIENT_NAME,
                 Contact = user.MPD_MOBILE_NO
             });





         }*/


        [HttpPost("patient-login-api")]
        public async Task<IActionResult> loginpatient(AppoinmentLoginModel login)
        {
            if (string.IsNullOrEmpty(login.patientcode))
            {
                return BadRequest("Invalid credentials");
            }

            // Use asynchronous database call
            var user = await _context.MED_PATIENTS_DETAILS
                .FirstOrDefaultAsync(u => u.MPD_PATIENT_CODE == login.patientcode);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]);

            var claims = new[]
            {
        new Claim(ClaimTypes.Name, user.MPD_MOBILE_NO),
        new Claim(ClaimTypes.Role, "patient")
    };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Message = "Login successful",
                Role = "patient",
                Token = tokenString,
                Email = user.MPD_EMAIL,
                PatientCode = user.MPD_PATIENT_CODE,
                Name = user.MPD_PATIENT_NAME,
                Contact = user.MPD_MOBILE_NO
            });
        }








        [HttpPost("Login")]
        public IActionResult Login([FromBody] AppoinmentLoginModel login)
        {
            if (login == null || string.IsNullOrEmpty(login.email) || string.IsNullOrEmpty(login.password))
            {
                return BadRequest("Invalid login request");
            }

            // Check if the user exists
            var user = _context.MED_PATIENTS_DETAILS
                        .FirstOrDefault(u => u.MPD_EMAIL == login.email && u.MPD_PASSWORD == login.password);

            if (user == null)
            {
                return Unauthorized("Invalid credentials");
            }

            // Generate the JWT token
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:SecretKey"]); // Secret key from config

            // Add claims (e.g., email)
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.MPD_EMAIL),
                // Optionally add role claim here if available
                new Claim(ClaimTypes.Role, "patient") // Replace 'patient' with user role from DB if needed
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(1), // Token expiration
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return Ok(new
            {
                Message = "Login successful",
                Role = "patient", // Change to actual role if dynamic
                Token = tokenString,
                Email = login.email,
                PatientCode = user.MPD_PATIENT_CODE
            });
        }
    }

    // Model class definition
    public class AppoinmentLoginModel
    {
        public string? email { get; set; }
        public string? password { get; set; }

        public string? contact { get; set; }


        public string? patientcode { get;set; }
    }
}
