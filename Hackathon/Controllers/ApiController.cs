using Hackathon.Areas.Identity.Data;
using Hackathon.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Hackathon.Areas.Identity.Pages.Account;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.WebUtilities;
using System.Text.Encodings.Web;
using System.Text;

namespace Hackathon.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles ="Admin")]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;
        private readonly HackathonContext _context;
        private readonly SignInManager<HackathonUser> _signInManager;
        private readonly UserManager<HackathonUser> _userManager;
        private readonly IUserStore<HackathonUser> _userStore;
        private readonly IUserEmailStore<HackathonUser> _emailStore;
        private readonly IEmailSender _emailSender;
        public ApiController(
            ILogger<ApiController> logger,
            HackathonContext context,
            UserManager<HackathonUser> userManager,
            IUserStore<HackathonUser> userStore,
            SignInManager<HackathonUser> signInManager,
            IEmailSender emailSender)
        {
            _logger = logger;
            _context = context;
            _userManager = userManager;
            _userStore = userStore;
            _emailStore = (IUserEmailStore<HackathonUser>)_userStore;
            _signInManager = signInManager;
            _logger = logger;
            _emailSender = emailSender;
            }
        public IList<AuthenticationScheme> ExternalLogins { get; set; }

        // GET: api/Api
        [HttpGet]
        public async Task<ActionResult<IEnumerable<HackathonUser>>> GetTodoItems()
        {
            return await _context.HackathonUser.ToListAsync();
        }

        // GET: api/Api/5
        [HttpGet("{id}")]
        public async Task<ActionResult<HackathonUser>> GetTodoItem(string id)
        {
            var todoItem = await _context.HackathonUser.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            return todoItem;
        }

        // POST: api/Api
        /*[HttpPost]
        public async Task<ActionResult<HackathonUser>> PostTodoItem(HackathonUser todoItem)
        {
            var user = Activator.CreateInstance<HackathonUser>();

            user.Name = todoItem.Name;
            user.SecondName = todoItem.SecondName;

            await _userStore.SetUserNameAsync(user, todoItem.Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, todoItem.Email, CancellationToken.None);
            user.EmailConfirmed = true;
            var result = await _userManager.CreateAsync(user, todoItem.PasswordHash);
            await _userManager.AddToRoleAsync(user, "Admin"); *//*Міняти для створення Адміна*//*

            

            return CreatedAtAction(nameof(GetTodoItem), new { id = todoItem.Id }, todoItem);
        }*/
        // POST: api/Api
        [HttpPost]
        public async Task<ActionResult<HackathonUser>> PostTodoItem(string Name, string SecondName, string Email, string Password)
        {
            string returnUrl = null;
            returnUrl ??= Url.Content("~/");
            var existingUser = await _userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                // Обробка випадку, коли користувач з такою електронною адресою вже існує
                ModelState.AddModelError(string.Empty, "User with this email already exists.");
                return BadRequest(ModelState);
            }
            var user = Activator.CreateInstance<HackathonUser>();

            user.Name = Name;
            user.SecondName = SecondName;

            await _userStore.SetUserNameAsync(user, Email, CancellationToken.None);
            await _emailStore.SetEmailAsync(user, Email, CancellationToken.None);
            user.EmailConfirmed = false;
            var result = await _userManager.CreateAsync(user, Password);
            await _userManager.AddToRoleAsync(user, "User");
            if (result.Succeeded)
            {
                _logger.LogInformation("User created a new account with password.");

                var userId = await _userManager.GetUserIdAsync(user);
                var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
                var callbackUrl = Url.Page(
                    "/Account/ConfirmEmail",
                    pageHandler: null,
                    values: new { area = "Identity", userId = userId, code = code, returnUrl = returnUrl },
                    protocol: Request.Scheme);

                await _emailSender.SendEmailAsync(Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

            }



            return CreatedAtAction(nameof(GetTodoItem), new { id = user.Id }, user);
        }
        [HttpPost]
        [Route("/api/[controller]/validate")]
        public async Task<IActionResult> Validate(string Email, string Password)
        {
            var existingUser = await _userManager.FindByEmailAsync(Email);
            if (existingUser != null)
            {
                var result = await _signInManager.CheckPasswordSignInAsync(existingUser, Password, lockoutOnFailure: false);
                if (result.Succeeded)
                {
                    // Повертаємо, наприклад, статус 200 OK або об'єкт з інформацією про користувача
                    return Ok(new { Message = "User is Valid!" });
                }
            }
            else
            {
                return BadRequest(new { Message = "User with this email doesn't exist!" });
            }
            
            return NotFound();

        }


        // DELETE: api/Api/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTodoItem(string id)
        {
            var todoItem = await _context.HackathonUser.FindAsync(id);

            if (todoItem == null)
            {
                return NotFound();
            }

            _context.HackathonUser.Remove(todoItem);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
