using Estral.Gif.Infra.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Estral.Gif.Pages.Auth;

public class LoginModel : PageModel
{

	public string LoginUrl { get; private set; }

	public async Task OnGet(
		bool fast,
		[FromServices] SignInManager<Database.User> signinManager,
		[FromServices] IConfiguration config)
	{

		if (signinManager.IsSignedIn(User))
		{
			await signinManager.SignOutAsync();
		}

		var clientId = config.GetRequiredValue("Auth:Discord:ClientId");
		var redirectUri = config.GetRequiredValue("Auth:Discord:RedirectDomain") + "/auth/callback";

		var scope = "identify";
		var state = Guid.NewGuid().ToString();
		if (fast) state += "-fast";

		var url = $"https://discord.com/api/oauth2/authorize?response_type=code&prompt=none&client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&state={state}";

		Response.Cookies.Append("estral.gif.state", state, new CookieOptions()
		{
			HttpOnly = true,
			Path = "/",
			Secure = true,
			Expires = DateTimeOffset.Now.AddMinutes(5),
			SameSite = SameSiteMode.Strict
		});

		if (fast)
		{
			Response.Redirect(url);
		}

		LoginUrl = url;
	}
}
