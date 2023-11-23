using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;

namespace Hackathon.Controllers
{
    public class AccountController : Controller
    {
        private readonly IAuthenticationSchemeProvider _authenticationSchemeProvider;

        public AccountController(IAuthenticationSchemeProvider authenticationSchemeProvider)
        {
            _authenticationSchemeProvider = authenticationSchemeProvider;
        }

        public IActionResult ExternalLogin(string provider, string returnUrl = null)
        {
            // Перевірте, чи вказаний провайдер існує
            var authenticationScheme = _authenticationSchemeProvider.GetSchemeAsync(provider).Result;
            if (authenticationScheme == null)
            {
                return NotFound();
            }

            // Отримайте порт, на якому працює додаток
            var port = HttpContext.Connection.LocalPort;

            // Отримайте URL для повернення після успішної аутентифікації
            var redirectUrl = Url.Action(nameof(ExternalLoginCallback), "Account", new { returnUrl });

            // Спрямуйте користувача на зовнішню сторінку авторизації
            var absoluteRedirectUrl = $"{HttpContext.Request.Scheme}://localhost:{port}{redirectUrl}";

            var properties = new AuthenticationProperties { RedirectUri = absoluteRedirectUrl };
            return Challenge(properties, provider);
        }

        public IActionResult ExternalLoginCallback(string returnUrl = null, string remoteError = null)
        {
            // Обробка результатів зовнішньої аутентифікації
            // Ваш код обробки тут

            // Переадресація на URL після успішної аутентифікації або на сторінку з помилкою
            return RedirectToAction("Index", "Home");
        }
    }
}