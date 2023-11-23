using Hackathon.Areas.Identity.Data;
using Hackathon.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace Hackathon.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly UserManager<HackathonUser> _userManager;
        private readonly SignInManager<HackathonUser> _signInManager;
        public HomeController(ILogger<HomeController> logger, UserManager<HackathonUser> userManager, SignInManager<HackathonUser> signInManager)
        {
            _logger = logger;
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        public async Task<IActionResult> DeactivateAccount()
        {
            if (User.Identity.IsAuthenticated)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    user.IsActive = false;
                    var result = await _userManager.UpdateAsync(user);

                    if (result.Succeeded)
                    {
                        await _signInManager.SignOutAsync(); // Sign out the user after deactivating the account
                        return RedirectToAction("Index");
                    }
                }
            }

            // If something goes wrong or user not authenticated, redirect to some page (for example, the Index page)
            return RedirectToAction("Index");
        }
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}