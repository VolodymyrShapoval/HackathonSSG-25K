// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
#nullable disable

using System;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Hackathon.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Hackathon.Data;
using System.Text.Encodings.Web;

namespace Hackathon.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class ActivationAccountModel : PageModel
    {
        private readonly UserManager<HackathonUser> _userManager;
        private readonly IEmailSender _sender;
        private readonly SignInManager<HackathonUser> _signInManager;
        private readonly HackathonContext _context;
        public ActivationAccountModel(UserManager<HackathonUser> userManager, IEmailSender sender, SignInManager<HackathonUser> signInManager,HackathonContext context)
        {
            _userManager = userManager;
            _sender = sender;
            _signInManager = signInManager;
            _context= context;

        }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public bool DisplayConfirmAccountLink { get; set; }

        /// <summary>
        ///     This API supports the ASP.NET Core Identity default UI infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public string EmailConfirmationUrl { get; set; }

        public async Task<IActionResult> OnGetAsync(string email, string returnUrl = null)
        {
            returnUrl = returnUrl ?? Url.Content("~/");

            if (email == null)
            {
                
                return RedirectToAction("Index");
            }
            var existingUser = await _userManager.FindByEmailAsync(email);
            
            if (!existingUser.IsActive)
            {
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(existingUser);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                        "/Account/ConfirmEmail",
                        pageHandler: null,
                        values: new { area = "Identity", userId = existingUser.Id, code = code, returnUrl = returnUrl },
                        protocol: Request.Scheme);
                await _sender.SendEmailAsync(email, "Confirm your email",
    $"<p>Вітаємо Вас з найкращими новинами!</p>Для успішного завершення реєстрації та активації вашого облікового запису, перейдіть за <a href='{HtmlEncoder.Default.Encode(callbackUrl)}' style='color: #4CAF50; text-decoration: none; font-weight: bold;'>цим посиланням</a>.</p>Щиро вдячні за обрання нашого сервісу! Нам важливо, щоб кожен момент з нами був особливим.</p>З найкращими побажаннями, Команда <b>SSG-25K</b>"); await _signInManager.SignOutAsync();
                return RedirectToAction("Index");
            }

            return LocalRedirect(returnUrl);
        }
    }
}
