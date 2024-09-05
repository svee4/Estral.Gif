using Estral.Gif.Database;
using Estral.Gif.Infra.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Estral.Gif.Pages;

[Authorize]
[ValidateAntiForgeryToken]
public class GroupModel : PageModel
{

	public GroupDto? Group { get; private set; }
	public string? ErrorMessage { get; private set; }

	public async Task OnGet(int groupId, [FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenticated user's id is null");
		Group = await GetGroupDto(dbContext, groupId, userId, token);
		if (Group is null)
		{
			ErrorMessage = "Group not found";
		}
	}

	public async Task OnPostChangeName(int groupId, string? newName, [FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenticated user's id is null");

		newName = newName?.Trim();

		if (string.IsNullOrWhiteSpace(newName))
		{
			ErrorMessage = "New name must not be null or only whitespace";
		}
		else if (
			await dbContext.Groups
				.Where(group => group.OwnerId == userId)
				.Where(group => group.Name == newName)
				.AnyAsync(token))
		{
			ErrorMessage = "Duplicate group name";
		}

		if (ErrorMessage is not null)
		{
			Group = await GetGroupDto(dbContext, groupId, userId, token);
			return;
		}

		_ = await dbContext.Groups
			.Where(group => group.Id == groupId && group.OwnerId == userId)
			.ExecuteUpdateAsync(model => model.SetProperty(group => group.Name, newName), token);

		Group = await GetGroupDto(dbContext, groupId, userId, token);
	}

	public async Task OnPostRemoveGif(int groupId, int gifId, [FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenticated user's id is null");

		_ = await dbContext.SavedGifs
			.Where(gif => gif.Id == gifId && gif.OwnerId == userId)
			.ExecuteUpdateAsync(model => model.SetProperty(gif => gif.Group, (Group?)null), token);

		Group = await GetGroupDto(dbContext, groupId, userId, token);
	}

	public async Task OnPostUnsave(int groupId, int gifId, [FromServices] AppDbContext dbContext, CancellationToken token)
	{
		var userId = User.GetUserId() ?? throw new Exception("Authenticated user's id is null");

		_ = await dbContext.SavedGifs
			.Where(gif => gif.Id == gifId && gif.OwnerId == userId)
			.ExecuteDeleteAsync(token);

		Group = await GetGroupDto(dbContext, groupId, userId, token);
	}

	private static async Task<GroupDto?> GetGroupDto(AppDbContext dbContext, int groupId, string userId, CancellationToken token) =>
		await dbContext.Groups
			.Where(group => group.Id == groupId && group.OwnerId == userId)
			.Select(group => new GroupDto
			{
				Id = group.Id,
				Name = group.Name,
				Gifs = group.SavedGifs.Select(gif => new GifDto
				{
					Id = gif.Id,
					Url = gif.Url
				}).ToList()
			})
			.FirstOrDefaultAsync(token);

	public sealed class GroupDto
	{
		public required int Id { get; init; }
		public required string Name { get; init; }
		public required IReadOnlyList<GifDto> Gifs { get; init; }
	}

	public sealed class GifDto
	{
		public required int Id { get; init; }
		public required string Url { get; init; }
	}
}
