using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using System.Collections.Specialized;

namespace Harmony
{
    public abstract class SecurableCookie
    {
        protected internal HttpCookie cookie;

        static HttpCookie CopyCookie(HttpCookie old)
        {
            var @new = new HttpCookie(old.Name, old.Value)
            {
                Domain = old.Domain,
                Expires = old.Expires,
                HttpOnly = old.HttpOnly,
                Path = old.Path,
                Secure = old.Secure
            };

            return @new;
        }
        
        
        protected SecurableCookie(HttpCookie cookie)
        {
            this.cookie = CopyCookie(cookie);
        }

        public string Domain
        {
            get
            {
                return cookie.Domain;
            }
        }

        public DateTime Expires
        {
            get
            {
                return cookie.Expires;
            }
        }

        public abstract bool? HasKeys { get;  }

        public bool HttpOnly
        {
            get
            {
                return cookie.HttpOnly;
            }
        }

        public string Name
        {
            get
            {
                return cookie.Name;
            }
        }

        public string Path
        {
            get
            {
                return cookie.Path;
            }
        }

        public bool Secure
        {
            get
            {
                return cookie.Secure;
            }
        }

        public string Value
        {
            get
            {
                return cookie.Value;
            }
        }

        public NameValueCollection Values
        {
            get
            {
                return cookie.Values;
            }
        }
        
        public abstract EncryptedCookie Encrypt();
        public abstract NonEncryptedCookie Decrypt();


        public static implicit operator SecurableCookie(HttpCookie cookie)
        {
            try
            {
                if (cookie != null)
                {
                    var value = cookie.Value.Decrypt();
                    return new EncryptedCookie(new NonEncryptedCookie(cookie));
                }

                return null;
            }
            catch (ArgumentException ex)            
            {
                if(ex.Source == "System.Web")
                    return new NonEncryptedCookie(cookie);

                throw ex;
            }
            catch (System.Web.HttpException ex)
            {
                if (ex.Source == "System.Web")
                    return new NonEncryptedCookie(cookie);

                throw ex;
            }
        }

        public static implicit operator HttpCookie(SecurableCookie secure)
        {
            return secure.cookie;
        }
    }

    public class EncryptedCookie : SecurableCookie
    {

        internal EncryptedCookie(HttpCookie cookie) : base(Encrypt(cookie))
        {

        }

        internal EncryptedCookie(NonEncryptedCookie cookie) : base(cookie.cookie)
        {

        }

        static HttpCookie Encrypt(HttpCookie cookie)
        {
            if(cookie != null)
                cookie.Value = cookie.Value.Encrypt();

            return cookie;
        }

        static HttpCookie Decrypt(HttpCookie cookie)
        {
            if(cookie != null)
                cookie.Value = cookie.Value.Decrypt();

            return cookie;
        }
        
        
        public override EncryptedCookie Encrypt()
        {
            return this;
        }

        public override NonEncryptedCookie Decrypt()
        {
            this.cookie = Decrypt(this.cookie);

            return new NonEncryptedCookie(this.cookie);
        }

        public override bool? HasKeys
        {
            get
            {
                return null;
            }
            
        }
    }

    public class NonEncryptedCookie : SecurableCookie
    {
        public NonEncryptedCookie(HttpCookie httpCookie) : base(httpCookie)
        {
            
        }

        public override EncryptedCookie Encrypt()
        {
            return new EncryptedCookie(this.cookie);
        }

        public override NonEncryptedCookie Decrypt()
        {
            return this;
        }

        public override bool? HasKeys
        {
            get { return cookie.HasKeys; }
        }
    }
}
