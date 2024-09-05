using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;

namespace Estral.Gif.Database;

public sealed class Group
{
	public int Id { get; private set; }
	public string Name { get; set; }

	public string OwnerId { get; set; }
	public User Owner { get; set; }

	public IReadOnlyList<SavedGif> SavedGifs { get; private set; }

	private Group() { }

	public static Group Create(string name, User owner)
	{
		ArgumentException.ThrowIfNullOrWhiteSpace(name, nameof(name));
		ArgumentNullException.ThrowIfNull(owner, nameof(owner));

		return new Group()
		{
			Name = name,
			OwnerId = owner.Id,
			Owner = owner
		};
	}

	public sealed class Configuration : IEntityTypeConfiguration<Group>
	{
		public void Configure(EntityTypeBuilder<Group> builder) 
		{
			// unsure if this is implicit; better safe than sorry
			builder.HasIndex(model => model.OwnerId);
		}
	}
}
