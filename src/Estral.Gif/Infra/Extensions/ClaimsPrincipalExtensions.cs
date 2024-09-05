using System.Security.Claims;

namespace Estral.Gif.Infra.Extensions;

public static class ClaimsPrincipalExtensions
{
	public static bool IsAuthenticated(this ClaimsPrincipal principal) =>
		principal.Identity?.IsAuthenticated ?? false;

	/// <summary>
	/// Get user id for claims principal, if one exists, otherwise null
	/// </summary>
	/// <param name="principal"></param>
	/// <returns></returns>
	public static string? GetUserId(this ClaimsPrincipal principal)
	{
		var claim = principal.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier);
		return claim?.Value;
	}

}
