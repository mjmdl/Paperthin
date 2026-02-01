using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Paperthin;

public sealed class AccessTokenSettings
{
	public string Key { get; init; } = string.Empty;
	public string Issuer { get; init; } = string.Empty;
	public string Audience { get; init; } = string.Empty;
	public int ExpirationMinutes { get; init; } = 0;
}

public sealed class AccessTokenEncoder(
	AccessTokenSettings _settings,
	JwtSecurityTokenHandler _handler)
{
	public string Encode(params Claim[] claims)
	{
		SymmetricSecurityKey key = GetKey();
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

	public JwtSecurityToken? Decode(string token)
	{
		try
		{
			return _handler.ReadJwtToken(token);
		}
		catch (Exception)
		{
			return null;
		}
	}

	public ClaimsPrincipal? DecodeIfCorrect(string token)
	{
		try
		{
			return _handler.ValidateToken(token, new TokenValidationParameters
			{
				ValidateIssuer = true,
				ValidIssuer = _settings.Issuer,

				ValidateAudience = true,
				ValidAudience = _settings.Audience,

				ValidateIssuerSigningKey = true,
				IssuerSigningKey = GetKey(),

				ValidateLifetime = true,
				ClockSkew = TimeSpan.Zero,
			}, out _);
		}
		catch (Exception)
		{
			return null;
		}
	}

	private SymmetricSecurityKey GetKey()
	{
		return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Key));
	}
}
