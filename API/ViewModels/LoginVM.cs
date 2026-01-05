
using API.Models;

namespace API.ViewModels
{
    public class LoginVM
    {
        public List<string>? Roles { get; set; }
        public TokenCredentials AccessToken { get; set; }
        public TokenCredentials RefreshToken { get; set; }
    }
}
