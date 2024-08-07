using System.ComponentModel.DataAnnotations;

namespace PolimiProject.Models;

public class SignDto
{
    [Required] public string Username { get; set; } = string.Empty;

    [Required] public string Password { get; set; } = string.Empty;
}