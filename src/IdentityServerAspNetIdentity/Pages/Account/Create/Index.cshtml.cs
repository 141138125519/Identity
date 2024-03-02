using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityServerAspNetIdentity.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Serilog;

namespace IdentityServerAspNetIdentity.Pages.Account.Create
{
    [SecurityHeaders]
    [AllowAnonymous]
    public class Index : PageModel
    {
        private readonly UserManager<ApplicationUser> _useUserManager;
        private readonly IIdentityServerInteractionService _interaction;
        

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public Index(
            UserManager<ApplicationUser> userManager,
            IIdentityServerInteractionService interaction)
        {
            _useUserManager = userManager;
            _interaction = interaction;
        }

        public async Task<IActionResult> OnGet(string? returnUrl)
        {
            Input = new InputModel
            {
                ReturnUrl = returnUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPost()
        {
            var context = await _interaction.GetAuthorizationContextAsync(Input.ReturnUrl);

            if (Input.Button == "cancel")
            {
                if (context == null)
                {
                    return Redirect("~/");
                }

                ArgumentNullException.ThrowIfNull(Input.ReturnUrl, nameof(Input.ReturnUrl));
                await _interaction.DenyAuthorizationAsync(context, AuthorizationError.AccessDenied);

                if (context.IsNativeClient())
                {
                    return this.LoadingPage(Input.ReturnUrl);
                }

                return Redirect(Input.ReturnUrl ?? "~/");
            }

            if (Input.Button == "create")
            {
                if (!ModelState.IsValid)
                {
                    return Page();
                }

                var newUser = new ApplicationUser
                {
                    UserName = Input.Username,
                    Email = Input.Email,
                };
                var result = _useUserManager.CreateAsync(newUser, Input.Password).Result;
                if (!result.Succeeded)
                {
                    Serilog.Log.Debug("User not created");

                    foreach (var error in result.Errors)
                    {
                        Serilog.Log.Error(error.Description);
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return Page();
                }
                Serilog.Log.Debug("User created");

                if (Url.IsLocalUrl(Input.ReturnUrl))
                {
                    return Redirect(Input.ReturnUrl);
                }
                else if (string.IsNullOrEmpty(Input.ReturnUrl))
                {
                    return Redirect("~/");
                }
                else
                {
                    throw new ArgumentException("invalid return URL");
                }
            }

            return Page();
        }
    }
}
