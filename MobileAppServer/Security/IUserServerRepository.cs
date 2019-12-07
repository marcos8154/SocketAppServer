namespace MobileAppServer.Security
{
    public interface IServerUserRepository
    {
        ServerUser Authenticate(string userNameOrEmail, string password);
    }
}
