using Microsoft.EntityFrameworkCore;

namespace Paperthin;

public sealed class AppDbContext : DbContext
{
	public AppDbContext(DbContextOptions<AppDbContext> options)
		: base(options)
	{
	}

	protected override void OnModelCreating(ModelBuilder modelBuilder)
	{
		base.OnModelCreating(modelBuilder);

		foreach (var entity in modelBuilder.Model.GetEntityTypes())
		{
			string? tableName = entity.GetTableName();
			if (tableName == null)
				continue;

			entity.SetTableName(tableName);
			entity.SetSchema("paperthin");

			foreach (var property in entity.GetProperties())
				property.SetColumnName(StringUtil.ToSnakeCase(property.Name));
		}
	}
}
