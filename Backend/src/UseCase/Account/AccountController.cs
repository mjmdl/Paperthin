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
}
