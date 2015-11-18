using System.Linq;
using System.Security.Principal;

namespace FiberKartan.API.Security
{
    public class CustomPrincipal : IPrincipal
    {

        public CustomPrincipal(IIdentity client)
        {
            _identity = client;
        }

        private IIdentity _identity;
        private string[] _roles;

        public IIdentity Identity
        {
            get { return _identity; }
        }

        public bool IsInRole(string role)
        {
            if (_identity.Name == "test")
                _roles = new string[1] { "ADMIN" };
            else
                _roles = new string[1] { "USER" };
            return _roles.Contains(role);
        }
    }
}