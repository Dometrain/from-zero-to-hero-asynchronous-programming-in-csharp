using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;

namespace PrincipalExample.Controllers;

public class AccountController : Controller
{
	public async Task<IActionResult> Login()
	{
		var user = User;

		// Simulate login (hardcoding a username and role for simplicity)
		var claims = new List<Claim>
		{
			new Claim(ClaimTypes.Name, "testuser"),
			new Claim(ClaimTypes.Role, "Admin")
		};
		var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
		var principal = new ClaimsPrincipal(identity);

		// Sign in the user
		await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal).ConfigureAwait(ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.None);

		return RedirectToAction("Index", "Home");
	}

	public async Task<IActionResult> Logout()
	{
		var user = User;

		// Sign out the user
		await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme).ConfigureAwait(ConfigureAwaitOptions.ForceYielding | ConfigureAwaitOptions.None);
		return RedirectToAction("Index", "Home");
	}
}