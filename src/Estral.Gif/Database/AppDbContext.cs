using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Estral.Gif.Database;

public sealed class AppDbContext(DbContextOptions options) : IdentityDbContext<User>(options)
{
	public DbSet<Group> Groups { get; private set; }
	public DbSet<SavedGif> SavedGifs { get; private set; }


	protected override void OnModelCreating(ModelBuilder builder)
	{
		base.OnModelCreating(builder);
		builder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
	}
}
