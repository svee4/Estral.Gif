using System.Globalization;
using Estral.Gif.Database;
using Estral.Gif.Infra.Extensions;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;

var builder = WebApplication.CreateBuilder(args);


if (builder.Environment.IsDevelopment())
{
	// environment variables in production 
	_ = builder.Configuration.AddJsonFile("appsettings.secret.json", optional: false);
}

builder.Services.AddNpgsql<AppDbContext>(
	builder.Configuration.GetRequiredValue("ConnectionStrings:Postgres"),
	optionsAction: options =>
	{
		if (builder.Environment.IsDevelopment())
		{
			_ = options.EnableSensitiveDataLogging();
		}
	}
);

builder.Services.AddIdentity<User, IdentityRole>(ConfigureIdentity)
	.AddEntityFrameworkStores<AppDbContext>();

builder.Services.ConfigureApplicationCookie(options =>
{
	options.Cookie.HttpOnly = true;
	options.Cookie.SameSite = SameSiteMode.Strict;
	options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
	options.Cookie.Path = "/";

	if (!int.TryParse(
		builder.Configuration.GetRequiredValue("Auth:CookieExpirationMinutes"),
		CultureInfo.InvariantCulture,
		out var expMinutes))
	{
		ConfigurationException.Throw("Value for 'Auth:CookieExpirationMinutes' must be parsable into an int");
	}

	options.SlidingExpiration = true;
	options.ExpireTimeSpan = TimeSpan.FromMinutes(expMinutes);

	options.LoginPath = "/Auth/Login";
	options.LogoutPath = "/Auth/Logout";
	options.AccessDeniedPath = "/Auth/AccessDenied";
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
	options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto);

builder.Services.AddRazorPages();
builder.Services.AddHttpClient();

var app = builder.Build();


if (app.Environment.IsDevelopment())
{
	app.UseDeveloperExceptionPage();
}
else
{
	app.UseExceptionHandler("/Error");
	app.UseForwardedHeaders();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapRazorPages();

app.Run();



static void ConfigureIdentity(IdentityOptions options) { }
