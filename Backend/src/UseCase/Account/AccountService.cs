using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;

namespace Paperthin;

public sealed class AccountService(
	AccountRepository _accountRepository,
	AccessTokenEncoder _accessTokenEncoder)
{
	public async Task Create(Account account)
	{
		await AssertEmailNotExists(account.Email);
		account.Password = HashPassword(account);
		await _accountRepository.Create(account);
	}

	public async Task<string> LogIn(string username, string password)
	{
		Account account = await FindByEmail(username);
		VerifyPassword(account, password);
		return GenerateAccessToken(account);
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

	private static void VerifyPassword(Account account, string rawPassword)
	{
		var hasher = new PasswordHasher<Account>();
		PasswordVerificationResult result = hasher.VerifyHashedPassword(account, account.Password, rawPassword);
		if (result == PasswordVerificationResult.Failed)
			throw new UnauthorizedAccessException("Password is wrong.");
	}

	private async Task<Account> FindByEmail(string email)
	{
		return await _accountRepository.FindByEmail(email) ??
			throw new KeyNotFoundException($"An account with the email {email} does not exist.");
	}

	private string GenerateAccessToken(Account account)
	{
		Console.WriteLine($"account.Id = {account.Id}");
		return _accessTokenEncoder.Encode(new Claim(JwtRegisteredClaimNames.Sub, account.Id.ToString()));
	}
}
