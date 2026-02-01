using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Paperthin;

public sealed class AccessTokenSettings
{
	public string Key { get; init; } = string.Empty;
	public string Issuer { get; init; } = string.Empty;
	public string Audience { get; init; } = string.Empty;
	public int ExpirationMinutes { get; init; } = 0;
}

public sealed class AccessTokenEncoder
{
	private AccessTokenSettings _settings;

	public AccessTokenEncoder(IOptions<AccessTokenSettings> options)
	{
		_settings = options.Value;
	}

	public string Encode(params Claim[] claims)
	{
		var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
		var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

		var expires = DateTime.UtcNow.AddMinutes(_settings.ExpirationMinutes);

		var token = new JwtSecurityToken(
			issuer: _settings.Issuer,
			audience: _settings.Audience,
			claims: claims,
			expires: expires,
			signingCredentials: credentials);

		return new JwtSecurityTokenHandler().WriteToken(token);
	}
}
