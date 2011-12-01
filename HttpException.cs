using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Harmony
{
    public abstract class HttpException : Exception, IHttpHandler
    {
        public virtual bool IsReusable
        {
            get { return false; }
        }

        public virtual bool HandleAsHttp { get { return true; } }

        public abstract void ProcessRequest(HttpContext context);
    }

    public class NotFound : HttpException
    {
        public override void ProcessRequest(HttpContext context)
        {
            context.Response.StatusCode = 404;
            context.Response.Write("<h1>Error</h1>");
        }
    }
}