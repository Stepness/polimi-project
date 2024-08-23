using Microsoft.AspNetCore.Authorization;
using PolimiProject.Models;
using PolimiProject.Services;
using PolimiProject.Settings;

namespace PolimiProject.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRepositoryUsers _repositoryUsers;

    public UsersController(IRepositoryUsers repositoryUsers)
    {
        _repositoryUsers = repositoryUsers;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(SignDto signDto)
    {
        var user = new UserEntity
        {
            Username = signDto.Username,
            Password = signDto.Password,
            Role = Roles.Reader
        };
        
        var addUserResult = await _repositoryUsers.AddUserAsync(user);

        if (addUserResult.Result == AddUserResultType.UserAlreadyExists)
            return Conflict(new { message = "Username already exist" });

        return Ok(JwtTokenHelper.GenerateJsonWebToken(addUserResult.User));
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(SignDto model)
    {
        var user = await _repositoryUsers.Authenticate(model.Username, model.Password);

        if (user == null)
            return Unauthorized(new { message = "Username or password is incorrect" });

        return Ok(JwtTokenHelper.GenerateJsonWebToken(user));
    }

    [HttpGet]
    [Authorize(Policy = IdentityData.AdminUserPolicy)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _repositoryUsers.GetAllUsers();
        return Ok(users.Select(x => new{ x.Username, x.Role }).ToList());
    }

    [HttpPut("{username}/promote-role")]
    [Authorize(Policy = IdentityData.AdminUserPolicy)]
    public async Task<IActionResult> PromoteRole(string username)
    {
        if (await _repositoryUsers.UpdateUserRoleToWriter(username))
            return Ok();

        return BadRequest();
    }

    [HttpPost("validate")]
    [Authorize]
    public IActionResult Validate()
    {
        return Ok();
    }
}