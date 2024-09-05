using Estral.Gif.Database;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Estral.Gif.Pages;

[Authorize]
[ValidateAntiForgeryToken]
public class AddGifModel : PageModel
{
	public void OnGet()
	{
	}

	public async Task OnPost(string link, [FromServices] AppDbContext dbContext, [FromServices] UserManager<User> userManager, CancellationToken token)
	{
		if (string.IsNullOrWhiteSpace(link))
		{
			return;
		}

		// dont validate link cuz idc
		var user = await userManager.GetUserAsync(User) ?? throw new InvalidOperationException("Null authenticated user");

		if ((await dbContext.SavedGifs
				.Where(gif => gif.OwnerId == user.Id && gif.Url == link)
				.Select(gif => gif.Id)
				.FirstOrDefaultAsync(token)) is { } existingGifId and not 0
		)
		{
			Response.Redirect($"/Gif/{existingGifId}");
			return;
		}

		var entry = SavedGif.Create(user, link);
		_ = dbContext.SavedGifs.Add(entry);
		_ = await dbContext.SaveChangesAsync(token);

		Response.Redirect($"/Gif/{entry.Id}");
	}
}
