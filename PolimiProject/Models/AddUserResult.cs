namespace PolimiProject.Models;

public class AddUserResult
{
    public AddUserResultType Result { get; set; }
    public UserEntity User { get; set; }
}

public enum AddUserResultType
{
    Success,
    UserAlreadyExists,
    Failure
}