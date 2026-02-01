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

		const string schema = "paperthin";

		foreach (var entity in modelBuilder.Model.GetEntityTypes())
		{
			string tableName = StringUtil.ToSnakeCase(entity.ClrType.Name);

			entity.SetTableName(tableName);
			entity.SetSchema(schema);

			foreach (var property in entity.GetProperties())
				property.SetColumnName(StringUtil.ToSnakeCase(property.Name));
		}
	}

	public DbSet<Account> Accounts => Set<Account>();
}
