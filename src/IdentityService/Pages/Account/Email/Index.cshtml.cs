using IdentityService.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace MesaProject.IdentityService.Pages.Account.Email
{
    public class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        //private readonly IEmailSender<ApplicationUser> _emailSender;
        public IndexModel(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager/*, IEmailSender<ApplicationUser> emailSender*/)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            //_emailSender = emailSender;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public bool IsEmailConfirmed { get; set; }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            Email = user.Email;
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user);

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(string action)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound("User not found.");
            }

            if (action == "confirm")
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var result = await _userManager.ConfirmEmailAsync(user, token);
                if (result.Succeeded)
                {
                    await _signInManager.RefreshSignInAsync(user);
                    TempData["StatusMessage"] = "Thank you for confirming your email.";
                }
                else
                {
                    TempData["StatusMessage"] = "Error confirming your email.";
                }
            }
            else if (action == "resend")
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { userId = user.Id, code = token },
                    protocol: Request.Scheme);

                // Replace this with your email sending logic
                //await _emailSender.SendConfirmationLinkAsync(user, "Confirm your email",
                //    $"Please confirm your account by <a href='{callbackUrl}'>clicking here</a>.");

                TempData["StatusMessage"] = "Confirmation link to verify your email sent. Please check your email.";
            }

            return RedirectToPage();
        }
    }
}
