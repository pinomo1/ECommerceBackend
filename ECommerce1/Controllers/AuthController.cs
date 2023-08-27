using ECommerce1.Models;
using ECommerce1.Models.ViewModels;
using ECommerce1.Services;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Web;

namespace ECommerce1.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AuthUser> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        private readonly TokenGenerator tokenGenerator;
        private readonly RoleGenerator roleGenerator;

        private readonly AccountDbContext accountDbContext;
        private readonly ResourceDbContext resourceDbContext;

        private readonly IConfiguration configuration;

        public readonly IValidator<SellerCredentials> selRegVal;
        public readonly IValidator<UserCredentials> userRegVal;
        public readonly IValidator<StaffCredentials> staffRegVal;
        public readonly IValidator<LoginCredentials> logVal;

        public readonly IEmailSender emailSender;

        public AuthController(UserManager<AuthUser> _userManager,
            RoleManager<IdentityRole> _roleManager,
            TokenGenerator _tokenGenerator,
            RoleGenerator _roleGenerator,
            AccountDbContext _accountDbContext,
            ResourceDbContext _resourceDbContext,
            IConfiguration _configuration,
            IValidator<SellerCredentials> _selRegVal,
            IValidator<UserCredentials> _userRegVal,
            IValidator<StaffCredentials> _staffRegVal,
            IValidator<LoginCredentials> _logVal,
            IEmailSender _emailSender
            )
        {
            this.userManager = _userManager;
            this.roleManager = _roleManager;
            this.tokenGenerator = _tokenGenerator;
            this.roleGenerator = _roleGenerator;
            this.accountDbContext = _accountDbContext;
            this.resourceDbContext = _resourceDbContext;
            this.configuration = _configuration;
            this.selRegVal = _selRegVal;
            this.userRegVal = _userRegVal;
            this.staffRegVal = _staffRegVal;
            this.logVal = _logVal;
            this.emailSender = _emailSender;
        }

        /// <summary>
        /// Login
        /// </summary>
        /// <param name="loginDto"></param>
        /// <param name="rememberMe"></param>
        /// <returns></returns>
        [HttpPost("login")]
        public async Task<ActionResult<AuthenticationResponse>> LoginAsync(LoginCredentials loginDto, bool rememberMe = true)
        {
            ValidationResult result = await logVal.ValidateAsync(loginDto);
            if (!result.IsValid)
            {
                return BadRequest(new { error_message = result.Errors[0].ErrorMessage });
            }

            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                return BadRequest(new
                {
                    error_message = "Invalid username or password"
                });
            }
            if (!await userManager.CheckPasswordAsync(user, loginDto.Password)) return BadRequest(new
            {
                error_message = "Invalid username or password"
            });
            if (!await userManager.IsEmailConfirmedAsync(user))
            {
                // await ResendEmailAsync(loginDto.Email);
                return BadRequest(new
                {
                    error_message = "Email is not confirmed"
                });
            }
            string role = (await userManager.GetRolesAsync(user))[0];
            var accessToken = tokenGenerator.GenerateAccessToken(user, role);
            var refreshToken = tokenGenerator.GenerateRefreshToken();
            accountDbContext.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.Now.Add(rememberMe ? tokenGenerator.Options.RefreshExpiration : tokenGenerator.Options.RefreshExpirationShort),
                AppUserId = user.Id
            });
            await accountDbContext.SaveChangesAsync();

            var response = new AuthenticationResponse
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };

            return Ok(response);
        }

        /// <summary>
        /// Registrate as a user
        /// </summary>
        /// <param name="registrationDto"></param>
        /// <returns></returns>
        [HttpPost("userreg")]
        public async Task<ActionResult<AuthenticationResponse>> UserRegistrationAsync(UserCredentials registrationDto)
        {
            ValidationResult result = await userRegVal.ValidateAsync(registrationDto);
            if (!result.IsValid)
            {
                return BadRequest(new { error_message = result.Errors[0].ErrorMessage });
            }
            if(await userManager.FindByEmailAsync(registrationDto.Email) != null)
            {
                return BadRequest(new { error_message = "User with such e-mail address does already exist" });
            }
            if(userManager.Users.Any(u => u.PhoneNumber == registrationDto.PhoneNumber))
            {
                return BadRequest(new { error_message = "User with such phone number does already exist" });
            }

            AuthUser? user = new()
            {
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber,
                UserName = registrationDto.Email,
            };

            IdentityResult createResult = await userManager.CreateAsync(user, registrationDto.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { error_message = createResult.Errors.ElementAt(0) });
            }

            AuthUser authUser = await userManager.FindByNameAsync(user.UserName);
            IdentityRole authRole = await roleManager.FindByNameAsync("User");
            if(authRole == null)
            {
                await roleGenerator.AddDefaultRoles(roleManager);
                authRole = await roleManager.FindByNameAsync("User");
            }

            IdentityResult addRole = await userManager.AddToRoleAsync(authUser, authRole.Name);
            if (!addRole.Succeeded)
            {
                await userManager.DeleteAsync(authUser);
                return BadRequest(new
                {
                    error_message = "Unidentified error"
                });
            }

            Profile profile = new()
            {
                AuthId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                FirstName = registrationDto.FirstName,
                LastName = registrationDto.LastName,
            };

            await resourceDbContext.Profiles.AddAsync(profile);
            await resourceDbContext.SaveChangesAsync();
            
            await SendEmailAsync(authUser);
            
            return Ok("Finalize registration by confirming email");
        }

        /// <summary>
        /// Change password
        /// </summary>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        [HttpPost("changepassword")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(string oldPassword, string newPassword)
        {
            AuthUser? user = await userManager.FindByIdAsync(User.FindFirstValue(ClaimTypes.NameIdentifier));
            if (user == null)
            {
                return BadRequest(new
                {
                    error_message = "User not found"
                });
            }
            if (!await userManager.CheckPasswordAsync(user, oldPassword))
            {
                return BadRequest(new
                {
                    error_message = "Invalid password"
                });
            }
            if (oldPassword == newPassword)
            {
                return BadRequest(new
                {
                    error_message = "New password must be different from old one"
                });
            }
            if (!await userManager.CheckPasswordAsync(user, oldPassword)) return BadRequest(new
            {
                error_message = "Invalid password"
            });
            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var result = await userManager.ResetPasswordAsync(user, token, newPassword);
            if (!result.Succeeded)
            {
                return BadRequest(result.Errors);
            }
            return Redirect("/opersucc.html");
        }

        [HttpPost("forgotpassword")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            AuthUser? user = await userManager.FindByEmailAsync(email);
            if(user == null)
            {
                return BadRequest(new
                {
                    error_message = "User not found"
                });
            }
            string validChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
            string validDigits = "0123456789";
            string validOthers = "#?!@$%^&*-_";
            Random random = new Random();
            char[] chars = new char[16];
            for (int i = 0; i < 7; i++)
            {
                chars[i] = validChars[random.Next(0, validChars.Length)];
            }
            for (int i = 0; i < 7; i++)
            {
                chars[7 + i] = validDigits[random.Next(0, validDigits.Length)];
            }
            for (int i = 0; i < 2; i++)
            {
                chars[14 + i] = validOthers[random.Next(0, validOthers.Length)];
            }
            string newPassword = new string(chars);
            string code = await userManager.GeneratePasswordResetTokenAsync(user);
            await userManager.ResetPasswordAsync(user, code, newPassword);
            await emailSender.SendEmailAsync(user.Email, "Reset password",
                $"You requested password reset, your new password: {newPassword}");
            return Ok();
        }

        /// <summary>
        /// Resend email confirmation
        /// </summary>
        /// <param name="email">email</param>
        /// <returns></returns>
        [HttpGet("resendemail")]
        public async Task<IActionResult> ResendEmailAsync(string email)
        {
            AuthUser? user = await userManager.FindByEmailAsync(email);
            if (user == null)
            {
                return BadRequest(new
                {
                    error_message = "Not logged in"
                });
            }
            if (await userManager.IsEmailConfirmedAsync(user))
            {
                return BadRequest(new
                {
                    error_message = "Email is already confirmed"
                });
            }
            await SendEmailAsync(user);
            return Ok();
        }
        
        /// <summary>
        /// Send email
        /// </summary>
        /// <param name="user">AuthUser</param>
        /// <returns></returns>
        [NonAction]
        public async Task<IActionResult> SendEmailAsync(AuthUser user)
        {
            string code = await userManager.GenerateEmailConfirmationTokenAsync(user);
            await emailSender.SendEmailAsync(user.Email, "Confirm your email",
                $"Confirm your email change by clicking on the link: <a href=\"{configuration["Links:Site"]}api/auth/confirmemail?userId={user.Id}&code={HttpUtility.UrlEncode(code)}\">Confirm your email</a>");
            return Ok();
        }

        /// <summary>
        /// Confirm email address
        /// </summary>
        /// <param name="userId">Auth user's id</param>
        /// <param name="code">Generated code</param>
        /// <returns></returns>
        [HttpGet("confirmemail")]
        public async Task<IActionResult> ConfirmEmailAsync(string userId, string code)
        {
            if (userId == null || code == null)
            {
                return BadRequest(new
                {
                    error_message = "Somethin is null"
                });
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new
                {
                    error_message = "No user found"
                });
            }

            var result = await userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return Redirect("/opersucc.html");
            }
            else
            {
                return BadRequest(new
                {
                    error_message = "Counldn't confirm email"
                });
            }
        }

        /// <summary>
        /// Send link to email to change email
        /// </summary>
        /// <param name="newEmail"></param>
        /// <returns></returns>
        [HttpGet("changemail")]
        [Authorize]
        public async Task<IActionResult> ChangeEmailAsync(string newEmail)
        {
            if (!Regex.IsMatch(newEmail, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z"))
            {
                return BadRequest(new
                {
                    error_message = "Not email"
                });
            }
            string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null)
            {
                return BadRequest(new
                {
                    error_message = "Not logged in"
                });
            }
            AuthUser? user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest(new
                {
                    error_message = "Not logged in"
                });
            }
            string oldEmail = user.Email;

            if (oldEmail == newEmail)
            {
                return Ok(new
                {
                    error_message = "New email must be different from old one"
                });
            }

            string code = await userManager.GenerateChangeEmailTokenAsync(user, newEmail);
            await emailSender.SendEmailAsync(newEmail, "Confirm your email change",
                $"Confirm your email change by clicking on the link: <a href=\"{configuration["Links:Site"]}api/auth/mailchanged?userId={user.Id}&newmail={HttpUtility.UrlEncode(newEmail)}&code={HttpUtility.UrlEncode(code)}\">Confirm email</a>");
            await emailSender.SendEmailAsync(user.Email, "Email change",
                $"A request was sent to change your email address. If it was not you, quickly log into the account and change the password. Otherwise, ignore this message");
            return Ok();
        }

        /// <summary>
        /// Change email given link from email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="newmail"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("mailchanged")]
        public async Task<IActionResult> MailChangeAsync(string userId, string newmail, string code)
        {
            if (!Regex.IsMatch(newmail, @"\A(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*@(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?)\Z"))
            {
                return BadRequest(new
                {
                    error_message = "Not email"
                });
            }
            if (userId == null || code == null)
            {
                return BadRequest(new
                {
                    error_message = "Wrong URL"
                });
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new
                {
                    error_message = "Not logged in"
                });
            }

            await userManager.ChangeEmailAsync(user, newmail, code);

            AUser? aUser = await resourceDbContext.Profiles.FirstOrDefaultAsync(x => x.AuthId == user.Id);
            aUser ??= await resourceDbContext.Sellers.FirstOrDefaultAsync(x => x.AuthId == user.Id);
            aUser ??= await resourceDbContext.Staffs.FirstOrDefaultAsync(x => x.AuthId == user.Id);
            if (aUser == null)
            {
                return BadRequest(new
                {
                    error_message = "Not logged in"
                });
            }

            aUser.Email = newmail;
            await resourceDbContext.SaveChangesAsync();

            return Redirect("/opersucc.html");
        }

        /// <summary>
        /// Send link to email to change phone number
        /// </summary>
        /// <param name="phone"></param>
        /// <returns></returns>
        [HttpGet("changephone")]
        [Authorize]
        public async Task<IActionResult> ChangePhoneAsync(string phone)
        {
            string id = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (id == null)
            {
                return BadRequest(new
                {
                    error_message = "Not logged in"
                });
            }
            AuthUser? user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return BadRequest(new
                {
                    error_message = "Not logged in"
                });
            }
            string email = user.Email;
            if (!Regex.IsMatch(phone, @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$"))
            {
                return BadRequest(new
                {
                    error_message = "Not a phone number"
                });
            }

            if (user.PhoneNumber == phone)
            {
                return Ok(new
                {
                    error_message = "New phone number must be different from old one"
                });
            }

            string code = await userManager.GenerateChangePhoneNumberTokenAsync(user, phone);
            await emailSender.SendEmailAsync(email, "Confirm your phone number change",
                $"Confirm your phone number change by clicking on the link: <a href=\"{configuration["Links:Site"]}api/auth/phonechanged?userId={user.Id}&phone={HttpUtility.UrlEncode(phone)}&code={HttpUtility.UrlEncode(code)}\">Confirm phone number</a>");
            return Ok();
        }

        /// <summary>
        /// Change phone number given link from email
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="phone"></param>
        /// <param name="code"></param>
        /// <returns></returns>
        [HttpGet("phonechanged")]
        public async Task<IActionResult> PhoneChangeAsync(string userId, string phone, string code)
        {
            if(!Regex.IsMatch(phone, @"^[\+]?[(]?[0-9]{3}[)]?[-\s\.]?[0-9]{3}[-\s\.]?[0-9]{4,6}$"))
            {
                return BadRequest(new
                {
                    error_message = "Not a phone number"
                });
            }

            if (userId == null || code == null)
            {
                return BadRequest(new
                {
                    error_message = "Bad URL"
                });
            }

            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return BadRequest(new
                {
                    error_message = "Not logged in"
                });
            }

            await userManager.ChangePhoneNumberAsync(user, phone, code);

            AUser? aUser = await resourceDbContext.Profiles.FirstOrDefaultAsync(x => x.AuthId == user.Id);
            aUser ??= await resourceDbContext.Sellers.FirstOrDefaultAsync(x => x.AuthId == user.Id);
            aUser ??= await resourceDbContext.Staffs.FirstOrDefaultAsync(x => x.AuthId == user.Id);
            if (aUser == null)
            {
                return BadRequest();
            }

            aUser.PhoneNumber = phone;
            await resourceDbContext.SaveChangesAsync();

            return Redirect("/opersucc.html");
        }

        /// <summary>
        /// Registrate as a staff/admin
        /// </summary>
        /// <param name="registrationDto"></param>
        /// <returns></returns>
        [HttpPost("staffreg")]
        public async Task<ActionResult<AuthenticationResponse>> StaffRegistrationAsync(StaffCredentials registrationDto)
        {
            ValidationResult result = await staffRegVal.ValidateAsync(registrationDto);
            if (!result.IsValid)
            {
                return BadRequest(new { error_message = result.Errors[0].ErrorMessage });
            }
            if (await userManager.FindByEmailAsync(registrationDto.Email) != null)
            {
                return BadRequest(new
                {
                    error_message = "User with such e-mail address does already exist"
                });
            }
            if (userManager.Users.Any(u => u.PhoneNumber == registrationDto.PhoneNumber))
            {
                return BadRequest(new
                {
                    error_message = "User with such phone number does already exist"
                });
            }

            AuthUser? user = new()
            {
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber,
                UserName = registrationDto.Email
            };

            IdentityResult createResult = await userManager.CreateAsync(user, registrationDto.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { error_message = createResult.Errors.ElementAt(0) });
            }

            AuthUser authUser = await userManager.FindByNameAsync(user.UserName);
            IdentityRole authRole = await roleManager.FindByNameAsync("Admin");
            if (authRole == null)
            {
                await roleGenerator.AddDefaultRoles(roleManager);
                authRole = await roleManager.FindByNameAsync("Admin");
            }

            IdentityResult addRole = await userManager.AddToRoleAsync(authUser, authRole.Name);
            if (!addRole.Succeeded)
            {
                await userManager.DeleteAsync(authUser);
                return BadRequest(new
                {
                    error_message = "Unexpected error"
                });
            }

            Staff profile = new()
            {
                AuthId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                DisplayName = registrationDto.DisplayName
            };

            await resourceDbContext.Staffs.AddAsync(profile);
            await resourceDbContext.SaveChangesAsync();

            await SendEmailAsync(authUser);
            
            return Ok("Finalize registration by confirming email");
        }


        /// <summary>
        /// Registrate as a seller
        /// </summary>
        /// <param name="registrationDto"></param>
        /// <returns></returns>
        [HttpPost("sellerreg")]
        public async Task<ActionResult<AuthenticationResponse>> SellerRegistrationAsync(SellerCredentials registrationDto)
        {
            ValidationResult result = await selRegVal.ValidateAsync(registrationDto);
            if (!result.IsValid)
            {
                return BadRequest(new { error_message = result.Errors[0].ErrorMessage });
            }
            if (await userManager.FindByEmailAsync(registrationDto.Email) != null)
            {
                return BadRequest(new
                {
                    error_message = "User with such e-mail address does already exist"
                });
            }
            if (userManager.Users.Any(u => u.PhoneNumber == registrationDto.PhoneNumber))
            {
                return BadRequest(new
                {
                    error_message = "User with such phone number does already exist"
                });
            }

            AuthUser? user = new()
            {
                Email = registrationDto.Email,
                PhoneNumber = registrationDto.PhoneNumber,
                UserName = registrationDto.Email
            };

            IdentityResult createResult = await userManager.CreateAsync(user, registrationDto.Password);
            if (!createResult.Succeeded)
            {
                return BadRequest(new { error_message = createResult.Errors.ElementAt(0) });
            }

            AuthUser authUser = await userManager.FindByNameAsync(user.UserName);
            IdentityRole authRole = await roleManager.FindByNameAsync("Seller");
            if (authRole == null)
            {
                await roleGenerator.AddDefaultRoles(roleManager);
                authRole = await roleManager.FindByNameAsync("Seller");
            }

            IdentityResult addRole = await userManager.AddToRoleAsync(authUser, authRole.Name);
            if (!addRole.Succeeded)
            {
                await userManager.DeleteAsync(authUser);
                return BadRequest(new
                {
                    error_message = "Unexpected error"
                });
            }

            Seller profile = new()
            {
                AuthId = user.Id,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                CompanyName = registrationDto.CompanyName,
                ProfilePhotoUrl = "https://itstep-ecommerce.azurewebsites.net/images/default.png"
            };

            await resourceDbContext.Sellers.AddAsync(profile);
            await resourceDbContext.SaveChangesAsync();

            await SendEmailAsync(authUser);
            
            return Ok("Finalize registration by confirming email");
        }


        /// <summary>
        /// Get new access token using refresh token
        /// </summary>
        /// <param name="oldRefreshToken"></param>
        /// <returns></returns>
        [HttpGet("refresh/{oldRefreshToken}")]
        public async Task<ActionResult<AuthenticationResponse>> RefreshAsync(string oldRefreshToken)
        {
            RefreshToken? token = await accountDbContext.RefreshTokens.FindAsync(oldRefreshToken);

            if (token == null)
                return BadRequest(new
                {
                    error_message = "Invalid refresh token"
                });

            accountDbContext.RefreshTokens.Remove(token);

            if (token.ExpiresAt < DateTime.Now)
                return BadRequest(new
                {
                    error_message = "Refresh token expired"
                });

            AuthUser? user = await userManager.FindByIdAsync(token.AppUserId);
            string role = (await userManager.GetRolesAsync(user))[0];
            string accessToken = tokenGenerator.GenerateAccessToken(user, role);
            string refreshToken = tokenGenerator.GenerateRefreshToken();

            accountDbContext.RefreshTokens.Add(new RefreshToken
            {
                Token = refreshToken,
                ExpiresAt = DateTime.Now.Add(tokenGenerator.Options.RefreshExpiration),
                AppUserId = user.Id
            });
            accountDbContext.SaveChanges();

            AuthenticationResponse response = new()
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
            return Ok(response);
        }

        /// <summary>
        /// Logout user
        /// </summary>
        /// <param name="refreshToken"></param>
        /// <returns></returns>
        [HttpGet("logout/{refreshToken}")]
        public async Task<IActionResult> LogoutAsync(string refreshToken)
        {
            var token = accountDbContext.RefreshTokens.Find(refreshToken);
            if (token != null)
            {
                accountDbContext.RefreshTokens.Remove(token);
                await accountDbContext.SaveChangesAsync();
            }
            return NoContent();
        }

        /// <summary>
        /// Logout all user sessions
        /// </summary>
        /// <returns></returns>
        [HttpGet("logoutall")]
        public async Task<IActionResult> LogoutAllAsync()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var tokens = accountDbContext.RefreshTokens.Where(t => t.AppUserId == userId);
            if (tokens != null)
            {
                accountDbContext.RefreshTokens.RemoveRange(tokens);
                await accountDbContext.SaveChangesAsync();
            }
            return NoContent();
        }

        /// <summary>
        /// Delete a user (only admin can do that)
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,User")]
        public async Task<IActionResult> DeleteAsync(string id)
        {
            AuthUser authUser = await userManager.FindByIdAsync(id);
            Profile? profile = await resourceDbContext.Profiles.FirstOrDefaultAsync(p => p.AuthId == id);
            Seller? seller = await resourceDbContext.Sellers.FirstOrDefaultAsync(s => s.AuthId == id);
            Staff? staff = await resourceDbContext.Staffs.FirstOrDefaultAsync(s => s.AuthId == id);
            if (authUser != null)
            {
                if(User.IsInRole("User") && User.FindFirstValue(ClaimTypes.NameIdentifier) != authUser.Id)
                {
                    return BadRequest(new
                    {
                        error_message = "You can only delete your own account"
                    });
                }
                await userManager.DeleteAsync(authUser);
            }
            else
            {
                return BadRequest(new
                {
                    error_message = "No such account"
                });
            }
            if(profile != null)
            {
                resourceDbContext.Profiles.Remove(profile);
                await resourceDbContext.SaveChangesAsync();
            }
            if (seller != null)
            {
                resourceDbContext.Sellers.Remove(seller);
                await resourceDbContext.SaveChangesAsync();
            }
            if (staff != null)
            {
                resourceDbContext.Staffs.Remove(staff);
                await resourceDbContext.SaveChangesAsync();
            }
            return NoContent();
        }
    }
}
