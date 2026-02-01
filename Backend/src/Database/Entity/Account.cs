namespace Paperthin;

public sealed class Account
{
	public Guid Id { get; init; }
	public string Name { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;
	public DateTimeOffset CreatedAt { get; init; }
	public DateTimeOffset? UpdatedAt { get; set; }
	public DateTimeOffset? DeletedAt { get; set; }
}

public static class AccountConstraint
{
	public const int NameMinLength = 3;
	public const int NameMaxLength = 50;
	public const int EmailMinLength = 5;
	public const int EmailMaxLength = 300;
	public const int PasswordMinLength = 8;
	public const int PasswordMaxLength = 100;
	public const string PasswordRegExp = @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]+$";
}
