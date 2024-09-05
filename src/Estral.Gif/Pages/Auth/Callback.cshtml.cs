using Estral.Gif.Infra.Extensions;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.Net.Http.Headers;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Estral.Gif.Pages.Auth;

[ValidateAntiForgeryToken]
public class CallbackModel : PageModel
{
	public string? ErrorMessage { get; private set; }
	public bool PostFast { get; private set; }
	public OAuthParamsDto OAuthParams { get; private set; }

	public void OnGet(string code, string state)
	{
		PostFast = true;
		OAuthParams = new OAuthParamsDto
		{
			Code = code,
			State = state,
		};
	}

	public async Task OnPost(
		[FromForm] PostDto postDto,
		[FromServices] IConfiguration config,
		[FromServices] HttpClient httpClient,
		[FromServices] UserManager<Database.User> userManager,
		[FromServices] SignInManager<Database.User> signInManager,
		[FromServices] Database.AppDbContext dbContext,
		[FromServices] ILogger<CallbackModel> logger,
		CancellationToken token)
	{
		OAuthParams = new OAuthParamsDto
		{
			Code = "",
			State = "",
		};

		var cookieState = Request.Cookies["estral.gif.state"];

		if (string.IsNullOrWhiteSpace(cookieState) || cookieState != postDto.State)
		{
			ErrorMessage = "Invalid state";
			return;
		}

		if (signInManager.IsSignedIn(User))
		{
			ErrorMessage = "Already signed in";
			return;
		}

		if (string.IsNullOrWhiteSpace(postDto.Code))
		{
			ErrorMessage = "Missing required parameter code";
			return;
		}

		var clientId = config.GetRequiredValue("Auth:Discord:ClientId");
		var clientSecret = config.GetRequiredValue("Auth:Discord:ClientSecret");
		var redirectDomain = config.GetRequiredValue("Auth:Discord:RedirectDomain");

		TokenResponse? data;
		// get access token

		{
			using var request = new HttpRequestMessage(HttpMethod.Post, "https://discord.com/api/oauth2/token");
			request.Content = new FormUrlEncodedContent(new Dictionary<string, string>
				{
					{ "client_id", clientId },
					{ "client_secret", clientSecret },
					{ "grant_type", "authorization_code" },
					{ "code", postDto.Code },
					{ "redirect_uri", $"{redirectDomain}/auth/callback" }
				});

			var response = await httpClient.SendAsync(request, token);
			data = await response.Content.ReadFromJsonAsync<TokenResponse>(token)
				?? throw new Exception("Discord auth api returned null json (1)");
		}

		UserResponse? jsonUser;
		// get user info

		{
			using var request = new HttpRequestMessage(HttpMethod.Get, "https://discord.com/api/v10/users/@me");
			request.Headers.Authorization = new AuthenticationHeaderValue(data.TokenType, data.AccessToken);

			var response = await httpClient.SendAsync(request, token);
			jsonUser = await response.Content.ReadFromJsonAsync<UserResponse>(token)
				?? throw new Exception("Discord auth api returned null json (2)");
		}

		var user = await userManager.FindByLoginAsync(loginProvider: "Discord", providerKey: jsonUser.Id);

		if (user is null)
		{
			// create new user

			user = Database.User.Create(jsonUser.Id);

			if (await userManager.CreateAsync(user) is { Succeeded: false } createUserError)
			{
				logger.LogError("Failed to create user: {User}, {Error}", jsonUser, createUserError);
				throw new Exception("Failed to create user (1)");
			}

			var loginInfo = new UserLoginInfo("Discord", jsonUser.Id, jsonUser.Username);
			if (await userManager.AddLoginAsync(user, loginInfo) is { Succeeded: false } loginInfoErr)
			{
				logger.LogError("Failed to add login info for user {User}: {Errors}", user, loginInfoErr.Errors.ToList());
				throw new Exception("Failed to create user (2)");
			}
		}

		if (await signInManager.ExternalLoginSignInAsync("Discord", jsonUser.Id, isPersistent: true)
			is { Succeeded: false } signInErr)
		{
			ErrorMessage = "Failed to sign in: " +
				(signInErr.IsLockedOut
				? "User locked out"
				: signInErr.IsNotAllowed
				? "Login not allowed"
				: "Unknown reason");
			return;
		}

		Response.Redirect("/");
	}


	private class TokenResponse
	{
		[JsonPropertyName("access_token")]
		public required string AccessToken { get; init; }

		[JsonPropertyName("token_type")]
		public required string TokenType { get; init; }

		[JsonPropertyName("expires_in")]
		public required int ExpiresIn { get; init; }

		[JsonPropertyName("refresh_token")]
		public required string RefreshToken { get; init; }
		public required string Scope { get; init; }
	}


	public sealed class UserResponse
	{
		public required string Id { get; init; }
		public required string Username { get; init; }
		public required string? Avatar { get; init; }
	}


	public sealed class OAuthParamsDto
	{
		public string Code { get; init; }

		public string State { get; init; }
	}

	public sealed class PostDto
	{
		[Required]
		public string Code { get; set; }
		[Required]
		public string State { get; set; }
	}
}
