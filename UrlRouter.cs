using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Prelude;

namespace Harmony
{

    
    public abstract class UrlRouter<TRoot> : IHttpModule where TRoot : Controller, new()
    {
        const StringComparison COMPARISON_METHOD = StringComparison.OrdinalIgnoreCase;
        
        HttpApplication application;

        public class HttpExceptionHandlerWrapper : IHttpHandler
        {
            Controller controller;

            public HttpExceptionHandlerWrapper(Controller controller)
            {
                this.controller = controller;
            }

            public bool IsReusable
            {
                get { return controller.IsReusable; }
            }

            public void ProcessRequest(HttpContext context)
            {
                try
                {
                    controller.ProcessRequest(context);
                }
                catch (HttpException ex)
                {
                    if (ex.HandleAsHttp)
                        ex.ProcessRequest(context);
                    else
                        throw;
                }
            }
        }        
        
        static readonly string[] ignoredExtensions = new[] { "jpg", "jpeg", "png", "gif", "css", "js", "html", "ico" };
        
        public virtual void Dispose() { }

        public void Init(HttpApplication context)
        {
            application = context;

            application.PostResolveRequestCache += Maphandler;
        }

        private string UrlExtenstenion(string[] pathSegments)
        {
            var last_segment = pathSegments[pathSegments.Length - 1];

            if (last_segment.Length > 0)
            {
                var temp = last_segment.Split('.');
                
                if (temp.Length > 1)
                {
                    var second_to_last = temp[temp.Length - 2];
                    var last = temp[temp.Length - 1];

                    if (second_to_last.Length > 0 && last.Length > 0)
                        return last;
                }
            }

            return "";
        }

        private void Maphandler(object sender, EventArgs e)
        {



           
            var context = new HttpContextWrapper(application.Context);

            var segments = context.Request.Path.ToLower().Split('/');

            if (segments[segments.Length - 1].Length == 0)
                segments = segments.Take(segments.Length - 1).ToArray();
            
            var ext = UrlExtenstenion(segments);

            var use_static_handler = ext.Length > 0 && ignoredExtensions.Any(x => x == ext);

            if (!use_static_handler)
            {
                Controller controller = new TRoot();

                try
                {
                    for (var i = 1; i < segments.Length; i++)
                        controller = controller.HandleMessage(segments[i], context);

                    context.RemapHandler(new HttpExceptionHandlerWrapper(controller));
                }
                catch (HttpException ex)
                {
                    if (ex.HandleAsHttp)
                        context.RemapHandler(ex);
                    else
                        throw;
                }
            }
        }
    }
}
