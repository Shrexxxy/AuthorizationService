using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;

namespace IdentityService.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<IdentityUser<Guid>> _signInManager;

        public LoginModel(SignInManager<IdentityUser<Guid>> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public string Email { get; set; }

        [BindProperty]
        public string Password { get; set; }

        public void OnGet()
        {
            // Страница вызывается пустой для отображения формы.
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
                return Page();

            // Попытка входа
            var result = await _signInManager.PasswordSignInAsync(Email, Password, false, lockoutOnFailure: false);

            // Проверка успешности
            if (result.Succeeded)
            {
                return LocalRedirect("/");
            }

            ModelState.AddModelError(string.Empty, "Не удалось выполнить вход. Проверьте логин и пароль.");
            return Page();
        }
    }
}