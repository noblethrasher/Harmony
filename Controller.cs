using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Harmony
{
    public interface Controller : IHttpHandler
    {
        Controller HandleMessage(string message, HttpContextBase context);
    }

    public class AdHocController : Controller
    {
        Action<HttpContext> processRequest;
        Func<string, HttpContextBase, Controller> handleMessage;

        public AdHocController(Func<string, HttpContextBase, Controller> handleMessage, Action<HttpContext> processRequest)
        {
            this.processRequest = processRequest;
            this.handleMessage = handleMessage;
        }
        
        public Controller HandleMessage(string message, HttpContextBase context)
        {
            return handleMessage(message, context);
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

    public abstract class ControllerBase : Controller
    {

        protected HttpContextBase context;


        Controller Controller.HandleMessage(string message, HttpContextBase context)
        {
            this.context = context;

            return HandleMessage(message);
        }

        public abstract Controller HandleMessage(string message);
        

        public virtual bool IsReusable
        {
            get { return false; }
        }

        public abstract void ProcessRequest(HttpContext context);
        

        
    }
}
