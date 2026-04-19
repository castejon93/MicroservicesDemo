namespace Auth.Application.Interfaces
{
    /// <summary>
    /// Interface for password hashing operations.
    /// Defined in Application, implemented in Infrastructure.
    /// </summary>
    public interface IPasswordHasher
    {
        string Hash(string password);
        bool Verify(string password, string hashedPassword);
    }
}
