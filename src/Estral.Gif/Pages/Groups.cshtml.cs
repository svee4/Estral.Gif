using Estral.Gif.Database;
using Estral.Gif.Infra.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Estral.Gif.Pages;

[Authorize]
[ValidateAntiForgeryToken]
public class GroupsModel : PageModel
{

	public IReadOnlyList<GroupDto> Groups { get; private set; }

	public async Task OnGet([FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenciated user's id is null");
		Groups = await GetGroups(dbContext, userId, token);
	}

	public async Task OnPost(int id, [FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenciated user's id is null");

		_ = await dbContext.Groups
			.Where(group => group.Id == id && group.OwnerId == userId)
			.ExecuteDeleteAsync(token);

		Groups = await GetGroups(dbContext, userId, token);
	}

	private static async Task<IReadOnlyList<GroupDto>> GetGroups(AppDbContext dbContext, string userId, CancellationToken token) =>
		await dbContext.Groups
			.Where(group => group.OwnerId == userId)
			//.Include(group => group.SavedGifs)
			.Select(group => new GroupDto
			{
				Id = group.Id,
				Name = group.Name,
				GifCount = group.SavedGifs.Count
			})
			.ToListAsync(token);

	public sealed class GroupDto
	{
		public required int Id { get; init; }
		public required string Name { get; init; }
		public required int GifCount { get; init; }
	}
}
