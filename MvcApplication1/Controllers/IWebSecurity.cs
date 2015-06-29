using System.Web;

namespace MvcApplication1.Controllers
{
    public interface IWebSecurity
    {
        bool Login(string userName, string password, bool persistCookie = true);
        void Logout();
        void CreateUserAndAccount(string userName, string password);
        int GetUserId(string userName);
        bool ChangePassword(string userName, string oldPassword, string newPassword);
        void CreateAccount(string userName, string newPassword);
    }

    public class WebSecurityImpl : IWebSecurity
    {
        private readonly HttpContextBase _httpContext;

        public WebSecurityImpl(HttpContextBase httpContext)
        {
            _httpContext = httpContext;
        }

        public bool Login(string userName, string password, bool persistCookie = true)
        {
            var httpCookie = new HttpCookie("__AUTH", userName);
            _httpContext.Response.Cookies.Add(httpCookie);

            return true;
        }

        public void Logout()
        {
            throw new System.NotImplementedException();
        }

        public void CreateUserAndAccount(string userName, string password)
        {
            throw new System.NotImplementedException();
        }

        public int GetUserId(string userName)
        {
            throw new System.NotImplementedException();
        }

        public bool ChangePassword(string userName, string oldPassword, string newPassword)
        {
            throw new System.NotImplementedException();
        }

        public void CreateAccount(string userName, string newPassword)
        {
            throw new System.NotImplementedException();
        }
    }
}