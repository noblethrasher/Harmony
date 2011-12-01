using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;
using System.Web;
using System.Web.Security;
using System.IO;
using System.Web.UI.WebControls;

namespace Harmony
{
    public class HTMLString
    {
        string html_string;

        public HTMLString(string s)
        {
            this.html_string = s;
        }

        public override string ToString()
        {
            return HttpContext.Current.Server.HtmlEncode(html_string);
        }

        public static implicit operator string(HTMLString str)
        {
            return str == null ? "" : str.ToString();
        }

        public static implicit operator HTMLString(string s)
        {
            return new HTMLString(s);
        }

        public static implicit operator HTMLString(DateTime s)
        {
            return new HTMLString(s.ToShortDateString());
        }

        public static implicit operator HTMLString(int s)
        {
            return new HTMLString(s.ToString());
        }

        public static implicit operator HTMLString(uint s)
        {
            return new HTMLString(s.ToString());
        }

        public static implicit operator HTMLString(bool s)
        {
            return new HTMLString(s.ToString());
        }

        public static implicit operator HTMLString(short s)
        {
            return new HTMLString(s.ToString());
        }

        public static implicit operator HTMLString(ushort s)
        {
            return new HTMLString(s.ToString());
        }

        public static implicit operator HTMLString(byte s)
        {
            return new HTMLString(s.ToString());
        }

        public static implicit operator HTMLString(long s)
        {
            return new HTMLString(s.ToString());
        }

        public static implicit operator HTMLString(ulong s)
        {
            return new HTMLString(s.ToString());
        }
    }    
    
    public static class Utils
    {

        public static NonEncryptedCookie Decrypt(this HttpCookie cookie)
        {
            return ((SecurableCookie)cookie).Decrypt();
        }
        
        public static PageEx<T> GetCompiledPageInstance<T>(this string path) where T : Page
        {
            var context = HttpContext.Current;

            return new PageEx<T>((T)PageParser.GetCompiledPageInstance(path, context.Server.MapPath(path), context));
        }

        public static PageEx<T> GetCompiledPageInstance<T>(this string path, Action<T> init) where T : Page
        {
            var page = path.GetCompiledPageInstance<T>();
            
            if(init != null)
                init(page);

            return page;
        }

        public static PageEx<Page> GetCompiledPageInstance(this string path)
        {
            var context = HttpContext.Current;

            return new PageEx<Page>((Page)PageParser.GetCompiledPageInstance(path, context.Server.MapPath(path), context));
        }

        public static string Encrypt(this string s)
        {            
            var bytes = Encoding.Unicode.GetBytes(s);

            return MachineKey.Encode(bytes, MachineKeyProtection.All);
        }

        public static string Decrypt(this string s)
        {
            var bytes = MachineKey.Decode(s, MachineKeyProtection.All);

            return Encoding.Unicode.GetString(bytes);
        }

        public static IHttpHandler CreateHandler(this Action<HttpContext> action)
        {
            return new AdhocHttpHandler(action);
        }

        public static ModelFactory<T> CreateModelFactory<T>(this Func<HttpContextBase, GetModelAttempt<T>> f)
        {
            return new AdHocModelFactory<T>(f);
        }

        public static Controller MakeController(this IHttpHandler handler)
        {
            return handler as Controller ?? (Controller) (handler as AdHocController) ?? new AdHocTerminalController (handler);
        }

        public static Controller MakeController(this string path)
        {
            return GetCompiledPageInstance(path).MakeController();
        }

        public static void Redirect(this HttpContextBase context, string URL)
        {
            context.Response.Redirect(URL);            
        }

        public static void Redirect(this HttpContextBase context, string URL, bool EndResponse)
        {
            context.Response.Redirect(URL, EndResponse);
        }

        public static void Redirect(this HttpContext context, string URL)
        {
            context.Response.Redirect(URL);
        }

        public static void Redirect(this HttpContext context, string URL, bool EndResponse)
        {
            context.Response.Redirect(URL, EndResponse);
        }

        public static void Write(this HttpContextBase context, object o)
        {
            context.Response.Write(o);
        }

        public static void Write(this HttpContextBase context, string o)
        {
            context.Response.Write(o);
        }

        public static void Write(this HttpContextBase context, Func<string> f)
        {
            context.Response.Write(f());
        }

        public static void Write(this HttpContext context, object o)
        {
            context.Response.Write(o);
        }

        public static void Write(this HttpContext context, string o)
        {
            context.Response.Write(o);
        }

        public static void Write(this HttpContext context, Func<string> f)
        {
            context.Response.Write(f());
        }

        public static string QueryString(this HttpContext context, string name)
        {
            return context.Request.QueryString[name];
        }

        public static string QueryString(this HttpContext context, int index)
        {
            return context.Request.QueryString[index];
        }

        public static string QueryString(this HttpContextBase context, string name)
        {
            return context.Request.QueryString[name];
        }

        public static string QueryString(this HttpContextBase context, int index)
        {
            return context.Request.QueryString[index];
        }
    }

    public class PageEx<T> : IHttpHandler where T : Page
    {
        public readonly T Page;
        
        public PageEx(T Page)
        {
            this.Page = Page;            
        }

        public bool IsReusable
        {
            get { return (Page as IHttpHandler).IsReusable; }
        }

        public void ProcessRequest(HttpContext context)
        {
            (Page as IHttpHandler).ProcessRequest(context);
        }

        public static implicit operator T(PageEx<T> page)
        {
            return page.Page;
        }
    }

    public class AdhocHttpHandler : IHttpHandler
    {
        readonly Action<HttpContext> processRequest;
        
        public bool IsReusable
        {
            get { return false; }
        }

        public AdhocHttpHandler(Action<HttpContext> action)
        {
            processRequest = action;
        }

        public AdhocHttpHandler(Action action)
        {
            processRequest = c => action();
        }

        public AdhocHttpHandler(string text)
        {
            processRequest = c => c.Response.Write (text);
        }

        public AdhocHttpHandler(FileInfo file)
        {
            processRequest = c => c.Response.WriteFile (file.FullName);
        }

        public void ProcessRequest(HttpContext context)
        {
            processRequest(context);
        }
    }   

    public class AdhocHttpHandlerFactory : IHttpHandlerFactory
    {
        readonly Func<HttpContext, string, string, string, IHttpHandler> getHandler;
        readonly Action<IHttpHandler> release;

        public AdhocHttpHandlerFactory(Func<HttpContext, string, string, string, IHttpHandler> getHandler, Action<IHttpHandler> release)
        {
            this.getHandler = getHandler;
            this.release = release;
        }

        public IHttpHandler GetHandler(HttpContext context, string requestType, string url, string pathTranslated)
        {
            return getHandler(context, requestType, url, pathTranslated);
        }

        public void ReleaseHandler(IHttpHandler handler)
        {
            release(handler);
        }
    }

    public class AdhocAsynHttpHandler : IHttpAsyncHandler
    {
        Func<HttpContext, AsyncCallback, object, IAsyncResult> beginProcess;
        Action<IAsyncResult> endProcess;

        Action<HttpContext> processRequest;

        public AdhocAsynHttpHandler(Func<HttpContext, AsyncCallback, object, IAsyncResult> beginProcess, Action<IAsyncResult> endProcess, Action<HttpContext> processRequest)
        {
            this.beginProcess = beginProcess;
            this.endProcess = endProcess;
            this.processRequest = processRequest;
        }

        public IAsyncResult BeginProcessRequest(HttpContext context, AsyncCallback cb, object extraData)
        {
            return beginProcess(context, cb, extraData);
        }

        public void EndProcessRequest(IAsyncResult result)
        {
            endProcess(result);
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            processRequest(context);
        }
    }


    /// <summary>
    /// An Ad-Hoc controller that handles the last segment of a URL path. If AdHocTerminalController handles some segment_i in the URL example.com/segment_1/segment_2/.../segment_n then segment_(i + 1) will return a 404 
    /// </summary>
    public class AdHocTerminalController : Controller
    {
        Action<HttpContext> action;

        public AdHocTerminalController(Action<HttpContext> action)
        {
            this.action = action;
        }

        public AdHocTerminalController(Action action)
        {
            this.action = c => action();
        }

        public AdHocTerminalController(IHttpHandler handler)
        {
            action = c => handler.ProcessRequest(c);
        }

        public AdHocTerminalController(IHttpHandlerFactory factory)
        {
            action = c => factory.GetHandler(c, c.Request.RequestType, c.Request.RawUrl, null);
        }

        public Controller HandleMessage(string message, System.Web.HttpContextBase context)
        {
            throw new NotFound();
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(System.Web.HttpContext context)
        {
            action(context);
        }

        public static implicit operator AdHocTerminalController(Page page)
        {
            return new AdHocTerminalController(page);
        }

        public static implicit operator AdHocTerminalController(string path)
        {
            return new AdHocTerminalController(path.GetCompiledPageInstance());
        }
    }

    public class Redirect : Controller
    {
        readonly Func<string> url;
        readonly Action<HttpContextBase> action;

        public Redirect(Func<string> url, Action<HttpContextBase> action)
        {
            this.url = url;
            this.action = action;
        }

        public Redirect(string url, Action action) : this(() => url, c => action()) { }
        
        public Redirect(string url) : this(() => url, (Action<HttpContextBase>)null) { }

        public Redirect(Func<string> url) : this (url, (Action<HttpContextBase>) null) { }
        
        public Controller HandleMessage(string message, HttpContextBase context)
        {
            return this;
        }

        public bool IsReusable
        {
            get { return false; }
        }

        public void ProcessRequest(HttpContext context)
        {
            if (action != null)
                action(new HttpContextWrapper(context));
            
            context.Redirect(url());
        }
    }
}
