using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Security.Principal;

namespace Harmony
{
    public abstract class FormsAuthenticationModule<T> : IHttpModule where T : Authenticator, new()
    {
        protected HttpApplication app;
        
        public void Dispose()
        {
            
        }

        public void Init(HttpApplication context)
        {
            app = context;
            app.AuthenticateRequest += Authenticate;
        }

        public void Authenticate(object sender, EventArgs e)
        {
            var context = app.Context;

            var user = context.Request["user"];
            var pass = context.Request["pass"];

            var authenticator = new T();

            authenticator.Init(app);

            var authenticated = false;

            if (user != null && pass != null)
            {
                authenticated = authenticator.Authenticate(user, pass);

                if (authenticated)
                {
                    HttpCookie cookie = authenticator.GetTicket().Encrypt();

                    context.Response.Cookies.Add(cookie);
                }

                context.User = authenticator.GetUser();
            }
            else
            {
                var authCookie = context.Request.Cookies[authenticator.AuthCookieName];

                if (authCookie != null)
                {
                    authenticated = authenticator.Authenticate(authCookie);

                    context.User = authenticator.GetUser();
                    
                }
            }


            context.User = authenticator.GetUser();
        }
    }

    public interface Authenticator
    {
        void Init(HttpApplication app);
        
        string AuthCookieName { get; }
        
        bool Authenticate(string user, string pass);
        bool Authenticate(SecurableCookie cookie);

        IPrincipal GetUser();

        SecurableCookie GetTicket();
    }
}
