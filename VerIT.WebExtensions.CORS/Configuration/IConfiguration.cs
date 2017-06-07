
namespace VerIT.WebExtensions.CORS.Configuration
{
    public interface IConfiguration
    {
        bool IsAllowed(string host, string origin);

    }
}
