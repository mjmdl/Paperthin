using Microsoft.AspNetCore.Identity;

namespace Paperthin;

public sealed class AccountService(AccountRepository _accountRepository)
{
	public async Task Create(Account account)
	{
		await AssertEmailNotExists(account.Email);
		account.Password = HashPassword(account);
		await _accountRepository.Create(account);
	}

	private async Task AssertEmailNotExists(string email)
	{
		if (await _accountRepository.EmailExists(email))
			throw new InvalidOperationException("E-mail is already in use.");
	}

	private static string HashPassword(Account account)
	{
		var hasher = new PasswordHasher<Account>();
		return hasher.HashPassword(account, account.Password);
	}
}
