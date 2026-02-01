using System.ComponentModel.DataAnnotations;

namespace Paperthin;

public sealed class SignUpRequest
{
	[Required]
	[Length(AccountConstraint.NameMinLength, AccountConstraint.NameMaxLength)]
	public string Name { get; init; } = "";

	[Required]
	[Length(AccountConstraint.EmailMinLength, AccountConstraint.EmailMaxLength)]
	[EmailAddress]
	public string Email { get; init; } = "";

	[Required]
	[Length(AccountConstraint.PasswordMinLength, AccountConstraint.PasswordMaxLength)]
	[RegularExpression(AccountConstraint.PasswordRegExp,
			ErrorMessage = "The field Password must contain at least an uppercase, lowercase, digit and special character.")]
	public string Password { get; init; } = "";

	public Account ToAccount()
	{
		return new Account
		{
			Name = this.Name,
			Email = this.Email,
			Password = this.Password,
		};
	}
}
