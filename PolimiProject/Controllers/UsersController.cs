using Microsoft.AspNetCore.Authorization;
using PolimiProject.Identity;
using PolimiProject.Models;
using PolimiProject.Services;
using PolimiProject.Settings;

namespace PolimiProject.Controllers;

using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase
{
    private readonly IRepository _repository;

    public UsersController(IRepository repository)
    {
        _repository = repository;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(SignDto signDto)
    {
        var user = new UserEntity
        {
            Username = signDto.Username,
            Password = signDto.Password,
            Role = Roles.Guest
        };
        
        var addUserResult = await _repository.AddUserAsync(user);

        if (addUserResult.Result == AddUserResultType.UserAlreadyExists)
            return Conflict(new { message = "Username already exist" });

        return Ok(JwtTokenHelper.GenerateJsonWebToken(addUserResult.User));
    }
    
    [HttpPost("login")]
    public async Task<IActionResult> Login(SignDto model)
    {
        var user = await _repository.Authenticate(model.Username, model.Password);

        if (user == null)
            return Unauthorized(new { message = "Username or password is incorrect" });

        return Ok(JwtTokenHelper.GenerateJsonWebToken(user));
    }

    [HttpGet]
    [Authorize(Policy = IdentityData.AdminUserPolicy)]
    public async Task<IActionResult> GetAll()
    {
        var users = await _repository.GetAllUsers();
        return Ok(users.Select(x => new{ x.Username, x.Role }).ToList());
    }

    [HttpPut("{username}/promote-role")]
    [Authorize(Policy = IdentityData.AdminUserPolicy)]
    public async Task<IActionResult> PromoteRole(string username)
    {
        if (await _repository.UpdateUserRoleToViewer(username))
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