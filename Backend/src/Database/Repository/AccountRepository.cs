using Microsoft.EntityFrameworkCore;

namespace Paperthin;

public sealed class AccountRepository(AppDbContext _dbContext)
{
	public async Task<bool> EmailExists(string email)
	{
		return await _dbContext.Accounts.AnyAsync(
			account => account.Email.ToLower() == email.ToLower());
	}

	public async Task Create(Account account)
	{
		await _dbContext.AddAsync(account);
		await _dbContext.SaveChangesAsync();
	}

	public async Task<Account?> FindByEmail(string email)
	{
		return await _dbContext.Accounts.SingleOrDefaultAsync(
			account => account.Email.ToLower() == email.ToLower());
	}
}
