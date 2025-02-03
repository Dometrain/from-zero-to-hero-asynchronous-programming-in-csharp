using Microsoft.AspNetCore.Authentication.Cookies;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
	.AddCookie(options =>
	{
		options.LoginPath = "/Account/Login";
		options.AccessDeniedPath = "/Account/AccessDenied";
	});

// Add MVC
builder.Services.AddControllersWithViews();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
	app.UseExceptionHandler("/Home/Error");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
		name: "default",
		pattern: "{controller=Home}/{action=Index}/{id?}")
	.WithStaticAssets();

app.MapControllerRoute(
		name: "login",
		pattern: "{controller=Account}/{action=Login}/{id?}")
	.WithStaticAssets();

app.MapControllerRoute(
		name: "logout",
		pattern: "{controller=Account}/{action=Logout}/{id?}")
	.WithStaticAssets();


app.Run();