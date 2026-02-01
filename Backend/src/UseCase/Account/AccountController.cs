using Microsoft.AspNetCore.Mvc;

namespace Paperthin;

[ApiController]
[Route("api/v1/account")]
public sealed class AccountController(AccountService _accountService)
	: ControllerBase
{
	[HttpPost("signup")]
	public async Task<IActionResult> SignUp([FromBody] SignUpRequest signUpRequest)
	{
		await _accountService.Create(signUpRequest.ToAccount());
		return Ok();
	}

	[HttpPost("login")]
	public async Task<ActionResult<LogInResponse>> LogIn([FromBody] LogInRequest logInRequest)
	{
		string accessToken = await _accountService.LogIn(logInRequest.Username, logInRequest.Password);
		return Ok(new LogInResponse() { AccessToken = accessToken });
	}
}
