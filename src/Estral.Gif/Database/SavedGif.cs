using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Estral.Gif.Database;

public sealed class SavedGif
{
	public int Id { get; private set; }
	public string Url { get; set; }

	public string OwnerId { get; set; }
	public User Owner { get; set; }

	public int? GroupId { get; set; }
	public Group? Group { get; set; }

	private SavedGif() { }

	public static SavedGif Create(User owner, string url)
	{
		ArgumentNullException.ThrowIfNull(owner, nameof(owner));
		ArgumentException.ThrowIfNullOrWhiteSpace(url, nameof(url));

		return new SavedGif()
		{
			OwnerId = owner.Id,
			Owner = owner,
			Url = url
		};
	}


	public sealed class Configuration : IEntityTypeConfiguration<SavedGif>
	{
		public void Configure(EntityTypeBuilder<SavedGif> builder)
		{
			builder.HasIndex(model => model.OwnerId);
			builder.HasIndex(model => model.GroupId);
		}
	}
}
