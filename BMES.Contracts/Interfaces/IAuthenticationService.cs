namespace BMES.Contracts.Interfaces
{
    public interface IAuthenticationService
    {
        bool Authenticate(string username, string password);
    }
}
