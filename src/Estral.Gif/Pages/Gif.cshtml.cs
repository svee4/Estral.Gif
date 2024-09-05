using Estral.Gif.Database;
using Estral.Gif.Infra.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Estral.Gif.Pages;

public class GifModel : PageModel
{
	public GifDto? Gif { get; private set; }
	public IReadOnlyList<GroupDto> Groups { get; private set; }

	public async Task OnGet(int gifId, [FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenticated user's id is null");
		await SetModelData(gifId, userId, dbContext, token);
	}

	public async Task OnPostUnsave(int gifId, [FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenticated user's id is null");

		_ = await dbContext.SavedGifs
				.Where(gif => gif.Id == gifId && gif.OwnerId == userId)
				.ExecuteDeleteAsync(token);

		await SetModelData(gifId, userId, dbContext, token);
	}

	public async Task OnPostMoveToGroup(int gifId, [FromForm] int groupId, [FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenticated user's id is null");

		if (!dbContext.Groups.Any(group => group.Id == groupId && group.OwnerId == userId))
		{
			await SetModelData(gifId, userId, dbContext, token);
			return;
		}

		_ = await dbContext.SavedGifs
				.Where(gif => gif.Id == gifId && gif.OwnerId == userId)
				.ExecuteUpdateAsync(model => model.SetProperty(gif => gif.GroupId, groupId), token);

		await SetModelData(gifId, userId, dbContext, token);
	}

	private async Task SetModelData(int gifId, string userId, AppDbContext dbContext, CancellationToken token)
	{
		Gif = await dbContext.SavedGifs
			.Where(gif => gif.Id == gifId && gif.OwnerId == userId)
			.Select(gif => new GifDto
			{
				Id = gif.Id,
				Url = gif.Url,
				Group = gif.Group == null ? null : gif.Group.Name,
				GroupId = gif.Group == null ? null : gif.Group.Id,
			})
			.FirstOrDefaultAsync(token);

		Groups = await dbContext.Groups
			.Where(group => group.OwnerId == userId)
			.Select(group => new GroupDto
			{
				Id = group.Id,
				Name = group.Name
			})
			.ToListAsync(token);
	}

	public sealed class GifDto
	{
		public required int Id { get; init; }
		public required string Url { get; init; }
		public required string? Group { get; init; }
		public required int? GroupId { get; init; }
	}

	public sealed class GroupDto
	{
		public required int Id { get; init; }
		public required string Name { get; init; }
	}
}
