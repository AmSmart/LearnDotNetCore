using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using LearnDotNetCore.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using MailKit.Net.Smtp;
using MimeKit;
// For more information on enabling MVC for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace LearnDotNetCore.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<IdentityUser> userManager;
        private readonly SignInManager<IdentityUser> signInManager;
        private readonly ILogger<AccountController> logger;
        private readonly IWebHostEnvironment webHostEnvironment;

        public AccountController(UserManager<IdentityUser> userManager,
            SignInManager<IdentityUser> signInManager, ILogger<AccountController> logger, IWebHostEnvironment webHostEnvironment)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.logger = logger;
            this.webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Register()
        {
            ViewBag.Title = "Register";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser() { Email = model.Email, UserName = model.Email };
                var result = await userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await SendEmailConfirmationLink(user);

                    if (signInManager.IsSignedIn(User) && User.IsInRole("Administrator"))
                        return RedirectToAction("ListUsers", "Admin");

                    //Error view is used to pass a message here
                    ViewBag.Title = "Registration Successful";
                    ViewBag.Message = "Your registration has been completed succesfully," +
                        " kindly confirm your email to sign in";
                    return View("Error");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError("", error.Description);
                }                


            }
            ViewBag.Title = "Register";
            return View(model);
        }

        private async Task SendEmailConfirmationLink(IdentityUser user)
        {
            var token = await userManager.GenerateEmailConfirmationTokenAsync(user);
            var confirmationLink = Url.Action("ConfirmEmail", "Account",
                new { UserId = user.Id, Token = token }, Request.Scheme);

            SendToEmail(user.Email, confirmationLink);
        }

        private void SendToEmail(string email, string confirmationLink)
        {
            
            //logger.Log(LogLevel.Warning, confirmationLink);

            MimeMessage message = new MimeMessage();

            MailboxAddress from = new MailboxAddress("Smart",
            "smartemma03@gmail.com");
            message.From.Add(from);

            MailboxAddress to = new MailboxAddress("Smart",
            email);
            message.To.Add(to);

            message.Subject = "Smart's Web";

            BodyBuilder bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = "<h1>Password Reset Link</h1>" +
                $"<a href=\"{confirmationLink}\" >Reset Password</>";
            bodyBuilder.TextBody = "Click the link to reset your password";

            message.Body = bodyBuilder.ToMessageBody();

            SmtpClient client = new SmtpClient();
            client.Connect("smtp.gmail.com", 587, false);
            client.Authenticate("smartemma03@gmail.com", "diamondemma");

            client.Send(message);
            client.Disconnect(true);
            client.Dispose();

            //Unhandled exception here
        }

        [AcceptVerbs("Get", "Post")]
        [AllowAnonymous]
        public async Task<IActionResult> IsEmailAllowed(string email)
        {
            var user = await userManager.FindByEmailAsync(email);
            return (user == null) ? Json(true) : Json($"'{email}' is already in use");

        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (userId == null || token == null)
            {
                //Error view is used to indicate an error here
                ViewBag.Title = "Error";
                ViewBag.Message = "Invalid email confirmation link";
                return View("Error");
            }
            var user = await userManager.FindByIdAsync(userId);
            if (user == null)
            {
                //Error view is used to indicate an error here
                ViewBag.Title = "Error";
                ViewBag.Message = "The user ID is invalid";
                return View("Error");
            }

            var result = await userManager.ConfirmEmailAsync(user, token);

            if (result.Succeeded)
            {
                //Error view is used to pass a message here
                ViewBag.Title = "Email Confirmed";
                ViewBag.Message = "Your email has been confirmed, you can now sign in to your account";
                return View("Error");
            }

            //Error view is used to indicate an error here
            ViewBag.Title = "Error";
            ViewBag.Message = "Email could not be confirmed, confimation link" +
                " has probably expired";
            return View("Error");
        }

        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Login(string returnUrl)
        {
            LoginViewModel model = new LoginViewModel
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };
            ViewBag.Title = "Login";
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginViewModel model, string returnUrl)
        {
            model.ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
            ViewBag.Title = "Login";
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null && !user.EmailConfirmed
                    && (await userManager.CheckPasswordAsync(user, model.Password)))
                {
                    ModelState.AddModelError("", "Email not confirmed yet");
                    await SendEmailConfirmationLink(user);
                    return View(model);
                }
                var result = await signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, true);
                if (result.Succeeded)
                {
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    {
                        return Redirect(returnUrl);
                    }
                    return RedirectToAction("Index", "Home");
                }

                if (result.IsLockedOut)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var passwordResetLink = Url.Action("ResetPassword", "Account", new { user.Email, Token = token });
                    ViewBag.Title = "Locked";
                    ViewBag.Message = "Your account has been locked due to an excessive amount of incorrect trials, " +
                        "you can reset with the link below";
                    ViewBag.OptionalLink = passwordResetLink;
                    ViewBag.OptionalLinkName = "Reset Password";
                    return View("Error");
                }
            }

            ModelState.AddModelError("", "Invalid Login Credentials");
            ViewBag.Title = "Register";
            return View(model);
        }

        [HttpPost]
        [AllowAnonymous]
        public IActionResult ExternalLogin(string provider, string returnUrl)
        {
            var redirectUrl = Url.Action("ExternalLoginCallback", "Account", new { ReturnUrl = returnUrl });

            var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, redirectUrl);
            return new ChallengeResult(provider, properties);
        }

        [AllowAnonymous]
        public async Task<IActionResult> ExternalLoginCallback(string? returnUrl, string? remoteError)
        {
            returnUrl ??= Url.Content("~/");

            LoginViewModel loginViewModel = new LoginViewModel()
            {
                ReturnUrl = returnUrl,
                ExternalLogins = (await signInManager.GetExternalAuthenticationSchemesAsync()).ToList()
            };

            if (remoteError != null)
            {
                ModelState.AddModelError(string.Empty, $"Error from external provider: {remoteError}");
                return View("Login", loginViewModel);
            }

            var info = await signInManager.GetExternalLoginInfoAsync();
            if (info == null)
            {
                ModelState.AddModelError(string.Empty, "Error loading external login information");
                return View("Login", loginViewModel);
            }
            var email = info.Principal.FindFirstValue(ClaimTypes.Email);
            IdentityUser user = null;

            if (email != null)
            {
                user = await userManager.FindByEmailAsync(email);
                if (user != null && !user.EmailConfirmed)
                {
                    ModelState.AddModelError("", "Email not confirmed yet");
                    return View("Login", loginViewModel);
                }
            }

            var signInResult = await signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, false, true);

            if (signInResult.Succeeded)
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                if (email != null)
                {

                    if (user == null)
                    {
                        user = new IdentityUser()
                        {
                            UserName = email,
                            Email = email
                        };
                        await userManager.CreateAsync(user);

                        await SendEmailConfirmationLink(user);

                        //Error view is used to pass a message here
                        ViewBag.Title = "Registration Successful";
                        ViewBag.Message = "Your registration has been completed succesfully," +
                            " kindly confirm your email to sign in";
                        return View("Error");
                    }

                    await userManager.AddLoginAsync(user, info);
                    await signInManager.SignInAsync(user, false);
                    return LocalRedirect(returnUrl);
                }

                ViewBag.Message = $"Email claim not received from {info.LoginProvider}, Kindly contact our support team";
                return View("Error");
            }
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ForgotPassword()
        {
            ViewBag.Title = "Forgot Password";
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                IdentityUser user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    string token = await userManager.GeneratePasswordResetTokenAsync(user);
                    string passwordResetLink = Url.Action("ResetPassword", "Account",
                        new { Email = model.Email, Token = token }, Request.Scheme);
                    SendToEmail(model.Email, passwordResetLink);
                }

                //Error view is used to pass a message here
                ViewBag.Title = "Request Successful";
                ViewBag.Message = "A message containg your password reset link will be sent to your " +
                    "mailbox if you have an account with us";
                return View("Error");
            }
            return View(model);
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult ResetPassword(string email, string token)
        {
            ViewBag.Title = "Reset Password";
            if (token == null || email == null)
            {
                ModelState.AddModelError("", "Invalid Password reset link");
            }
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> ResetPassword(PasswordResetViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user != null)
                {
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (result.Succeeded)
                    {
                        if (await userManager.IsLockedOutAsync(user))
                        {
                            await userManager.SetLockoutEndDateAsync(user, DateTimeOffset.Now);
                        }
                        //Error view is used to pass a message here
                        ViewBag.Title = "Success";
                        ViewBag.Message = "Yor password has been reset successfully";
                        return View("Error");
                    }

                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                    return View(model);
                }
                //Error view is used to pass a message here (tricky result to discourage brute-forcing)
                ViewBag.Title = "Success";
                ViewBag.Message = "Yor password has been reset successfully";
                return View("Error");
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> ChangePassword()
        {
            var user = await userManager.GetUserAsync(User);
            bool hasPassword = await userManager.HasPasswordAsync(user);
            ChangePasswordViewModel model = new ChangePasswordViewModel()
            {
                CurrentPassword = "",
                NewPassword = "",
                ConfirmNewPassword = "",
                HasPassword = hasPassword
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                {
                    return RedirectToAction("Login");
                }

                if (model.HasPassword)
                {
                    var result = await userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }

                    await signInManager.RefreshSignInAsync(user);
                    //Error view is used to pass a message here
                    ViewBag.Title = "Successful!";
                    ViewBag.Message = "Your password has been set successfully";
                    return View("Error");
                }
                else
                {
                    var result = await userManager.AddPasswordAsync(user, model.NewPassword);

                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View(model);
                    }

                    await signInManager.RefreshSignInAsync(user);
                    //Error view is used to pass a message here
                    ViewBag.Title = "Successful!";
                    ViewBag.Message = "Your password has been set successfully";
                    return View("Error");
                }
            }
            return View(model);
        }

        /*[HttpGet]
        public IActionResult AccessDenied()
        {
            ViewBag.Title = "Access deneied";
            ViewBag.Message = "You're not allowed to access this resource";
            return View("Error");
        }*/
    }
}
