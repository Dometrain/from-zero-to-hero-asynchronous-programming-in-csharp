using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using PrincipalExample.Models;

namespace PrincipalExample.Controllers;

public class HomeController : Controller
{
	public IActionResult Index()
	{
		// Access the current user's identity
		var username = User.Identity?.Name;  // "testuser"
		var isAuthenticated = User.Identity?.IsAuthenticated;  // true
		var userRoles = User.IsInRole("Admin");  // true

		// Get all claims of the user
		var claims = User.Claims.Select(c => new { c.Type, c.Value });

		// Use the principal (User) for authorization checks
		if (User.IsInRole("Admin"))
		{
			ViewData["Message"] = "Welcome, Admin!";
		}
		else
		{
			ViewData["Message"] = "Welcome, User!";
		}

		return View();
	}
}