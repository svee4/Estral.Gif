using Microsoft.AspNetCore.Identity;

namespace Estral.Gif.Database;

public sealed class User : IdentityUser
{
	public string DiscordUserId { get; private set; }

	public User() { }

	public static User Create(string discordUserId)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(discordUserId);
		if (discordUserId.Length is not (17 or 18) || !discordUserId.All(char.IsDigit))
		{
			throw new ArgumentException("Discord user id length must be 17 or 18 and must be only digits", nameof(discordUserId));
		}

		return new User()
		{
			DiscordUserId = discordUserId,
			UserName = discordUserId,
		};
	}
}
