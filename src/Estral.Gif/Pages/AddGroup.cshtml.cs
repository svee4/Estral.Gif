using Estral.Gif.Database;
using Estral.Gif.Infra.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Estral.Gif.Pages;

public class AddGroupModel : PageModel
{
	public void OnGet()
	{
	}

	public async Task OnPost(string name, [FromServices] AppDbContext dbContext, [FromServices] UserManager<User> userManager, CancellationToken token)
	{
		if (string.IsNullOrWhiteSpace(name))
		{
			return;
		}

		var user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException("Null authenticated user");

		if ((await dbContext.Groups
				.Where(group => group.OwnerId == user.Id && group.Name == name)
				.Select(group => group.Id)
				.FirstOrDefaultAsync(token)) is { } existingGroupId and not 0
		)
		{
			Response.Redirect($"/Group/{existingGroupId}");
			return;
		}

		var entry = Group.Create(name, user);
		_ = dbContext.Groups.Add(entry);
		_ = await dbContext.SaveChangesAsync(token);

		Response.Redirect($"/Group/{entry.Id}");
	}
}
