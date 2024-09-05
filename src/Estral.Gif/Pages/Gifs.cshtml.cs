using Estral.Gif.Database;
using Estral.Gif.Infra.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Estral.Gif.Pages;

[Authorize]
[ValidateAntiForgeryToken]
public class GifsModel : PageModel
{
	public IReadOnlyList<GifDto> Gifs { get; private set; }

	public async Task OnGet([FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenticated user's id is null");
		Gifs = await GetGifs(userId, dbContext, token);
	}


	private static async Task<IReadOnlyList<GifDto>> GetGifs(string userId, AppDbContext dbContext, CancellationToken token) =>
		await dbContext.SavedGifs
			.Where(gif => gif.OwnerId == userId)
			//.Include(gif => gif.Gif)
			.Select(gif => new GifDto
			{
				Id = gif.Id,
				Url = gif.Url,
				Group = gif.Group == null ? null : gif.Group.Name,
				GroupId = gif.Group == null ? default : gif.Group.Id
			})
			.ToListAsync(token);

	public sealed class GifDto
	{
		public required int Id { get; init; }
		public required string Url { get; init; }
		public required string? Group {  get; init; }
		public required int GroupId { get; init; }
	}
}
