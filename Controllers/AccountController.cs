using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using SpendingTracker.Models;
using SpendingTracker.ViewModels;

namespace SpendingTracker.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AccountController(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            IEmailSender emailSender,
            IWebHostEnvironment webHostEnvironment)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _webHostEnvironment = webHostEnvironment;
        }

        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = new ApplicationUser
            {
                UserName = vm.Email,
                Email = vm.Email,
                FirstName = vm.FirstName,
                LastName = vm.LastName,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, vm.Password);
            if (result.Succeeded)
            {
                TempData["Success"] = "Account created successfully. Please log in.";
                return RedirectToAction("Login");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel vm, string? returnUrl = null)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user != null)
            {
                var passwordCheck = await _signInManager.CheckPasswordSignInAsync(user, vm.Password, lockoutOnFailure: false);
                if (passwordCheck.Succeeded)
                {
                    await _signInManager.SignInAsync(user, vm.RememberMe);
                    TempData["Success"] = "Welcome back!";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                        return Redirect(returnUrl);
                    return RedirectToAction("Index", "Home");
                }
            }

            ModelState.AddModelError(string.Empty, "Invalid email or password.");
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        [HttpGet]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> EditProfile()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var vm = new EditProfileViewModel
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
                Currency = user.Currency,
                ExistingProfilePicture = user.ProfilePicture
            };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> EditProfile(EditProfileViewModel vm)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                vm.ExistingProfilePicture = user.ProfilePicture;
                return View(vm);
            }

            // Handle photo deletion
            if (vm.DeletePhoto && !string.IsNullOrEmpty(user.ProfilePicture))
            {
                var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
                user.ProfilePicture = null;
            }

            // Handle photo upload
            if (vm.ProfilePhoto != null && vm.ProfilePhoto.Length > 0)
            {
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var ext = Path.GetExtension(vm.ProfilePhoto.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(ext))
                {
                    ModelState.AddModelError("ProfilePhoto", "Only JPG, JPEG, and PNG images are allowed.");
                    vm.ExistingProfilePicture = user.ProfilePicture;
                    return View(vm);
                }

                if (vm.ProfilePhoto.Length > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ProfilePhoto", "Maximum allowed photo size is 2MB.");
                    vm.ExistingProfilePicture = user.ProfilePicture;
                    return View(vm);
                }

                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "profiles");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                if (!string.IsNullOrEmpty(user.ProfilePicture))
                {
                    var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePicture.TrimStart('/'));
                    if (System.IO.File.Exists(oldPath))
                    {
                        try { System.IO.File.Delete(oldPath); } catch { }
                    }
                }

                var fileName = $"{user.Id}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await vm.ProfilePhoto.CopyToAsync(stream);
                }

                user.ProfilePicture = $"/uploads/profiles/{fileName}";
            }

            user.FirstName = vm.FirstName;
            user.LastName = vm.LastName;
            user.Currency = vm.Currency;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                foreach (var e in updateResult.Errors)
                    ModelState.AddModelError(string.Empty, e.Description);
                vm.ExistingProfilePicture = user.ProfilePicture;
                return View(vm);
            }

            // Change password if provided
            if (!string.IsNullOrWhiteSpace(vm.NewPassword) && !string.IsNullOrWhiteSpace(vm.CurrentPassword))
            {
                var pwResult = await _userManager.ChangePasswordAsync(user, vm.CurrentPassword, vm.NewPassword!);
                if (!pwResult.Succeeded)
                {
                    foreach (var e in pwResult.Errors)
                        ModelState.AddModelError(string.Empty, e.Description);
                    vm.ExistingProfilePicture = user.ProfilePicture;
                    return View(vm);
                }
                await _signInManager.RefreshSignInAsync(user);
            }

            TempData["Success"] = "Profile updated successfully.";
            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Microsoft.AspNetCore.Authorization.Authorize]
        public async Task<IActionResult> DeleteProfilePicture()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!string.IsNullOrEmpty(user.ProfilePicture))
            {
                var oldPath = Path.Combine(_webHostEnvironment.WebRootPath, user.ProfilePicture.TrimStart('/'));
                if (System.IO.File.Exists(oldPath))
                {
                    try { System.IO.File.Delete(oldPath); } catch { }
                }
                user.ProfilePicture = null;
                await _userManager.UpdateAsync(user);
                await _signInManager.RefreshSignInAsync(user);
                TempData["Success"] = "Profile picture deleted successfully.";
            }

            return RedirectToAction(nameof(EditProfile));
        }

        // -- Forgot Password --------------------------------------------------

        [HttpGet]
        public IActionResult ForgotPassword() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ForgotPasswordConfirmation));
            }

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var resetLink = Url.Action(
                nameof(ResetPassword),
                "Account",
                new { token = token, email = vm.Email },
                Request.Scheme)!;

            var htmlBody = $@"<p>Hi {user.FirstName},</p>
<p>You requested a password reset for your SpendTracker account.</p>
<p><a href=""{resetLink}"">Reset Password</a></p>
<p>If you did not request this, please ignore this email. This link expires in 24 hours.</p>";

            await _emailSender.SendEmailAsync(vm.Email, "Reset Your SpendTracker Password", htmlBody);

            TempData["ResetLink"] = resetLink;
            return RedirectToAction(nameof(ForgotPasswordConfirmation));
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation() => View();

        // -- Reset Password ---------------------------------------------------

        [HttpGet]
        public IActionResult ResetPassword(string? token, string? email)
        {
            if (token == null || email == null)
            {
                TempData["Error"] = "Invalid password reset link.";
                return RedirectToAction("Login");
            }
            var vm = new ResetPasswordViewModel { Token = token, Email = email };
            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            var result = await _userManager.ResetPasswordAsync(user, vm.Token, vm.NewPassword);
            if (result.Succeeded)
            {
                TempData["Success"] = "Your password has been reset. Please log in with your new password.";
                return RedirectToAction(nameof(ResetPasswordConfirmation));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            return View(vm);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation() => View();

        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string userId, string token)
        {
            if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(token))
            {
                return RedirectToAction("Login");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{userId}'.");
            }

            try
            {
                var decodedTokenBytes = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlDecode(token);
                var decodedToken = System.Text.Encoding.UTF8.GetString(decodedTokenBytes);

                var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
                if (result.Succeeded)
                {
                    TempData["Success"] = "Email verified successfully! You can now log in.";
                }
                else
                {
                    TempData["Error"] = "Error confirming your email: " + string.Join(", ", result.Errors.Select(e => e.Description));
                }
            }
            catch (Exception)
            {
                TempData["Error"] = "Error confirming your email: Invalid confirmation token format.";
            }

            return RedirectToAction("Login");
        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation() => View();

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationViewModel vm)
        {
            if (!ModelState.IsValid) return View(vm);

            var user = await _userManager.FindByEmailAsync(vm.Email);
            if (user == null)
            {
                TempData["Success"] = "Verification email sent if the account exists.";
                return RedirectToAction("Login");
            }

            if (await _userManager.IsEmailConfirmedAsync(user))
            {
                TempData["Success"] = "This email is already verified. Please log in.";
                return RedirectToAction("Login");
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var code = Microsoft.AspNetCore.WebUtilities.WebEncoders.Base64UrlEncode(System.Text.Encoding.UTF8.GetBytes(token));
            var confirmationLink = Url.Action("ConfirmEmail", "Account", new { userId = user.Id, token = code }, Request.Scheme);

            var emailBody = $@"
                <div style='font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; padding: 20px; border: 1px solid #e2e8f0; border-radius: 8px;'>
                    <h2 style='color: #2b6cb0;'>Verify Your Email Address</h2>
                    <p>Please click the button below to verify your email and activate your Expense Tracker account.</p>
                    <div style='text-align: center; margin: 30px 0;'>
                        <a href='{confirmationLink}' style='background-color: #3182ce; color: white; padding: 12px 24px; text-decoration: none; border-radius: 6px; font-weight: bold; display: inline-block;'>Verify Email</a>
                    </div>
                    <p style='font-size: 0.8em; color: #718096;'>If you cannot click the button above, copy and paste this URL into your browser:</p>
                    <p style='font-size: 0.8em; color: #3182ce; word-break: break-all;'>{confirmationLink}</p>
                </div>";

            await _emailSender.SendEmailAsync(user.Email!, "Confirm your email - Expense Tracker", emailBody);

            TempData["Success"] = "Verification email sent if the account exists.";
            return RedirectToAction("Login");
        }
    }
}
