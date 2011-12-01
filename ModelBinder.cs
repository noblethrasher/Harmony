using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Harmony
{
    public abstract class ModelBinder<T> : Controller
    {
        protected HttpContextBase context;

        public abstract GetModelAttempt<T> GetModel();

        public Controller HandleMessage(string message, HttpContextBase context)
        {
            this.context = context;

            return HandleMessage(message, GetModel());
        }

        protected abstract Controller HandleMessage(string s, GetModelAttempt<T> model);
        
        public bool IsReusable
        {
            get { return false; }
        }

        public abstract void ProcessRequest(HttpContext context);        
    }

    public abstract class GenericModelBinder<T, K> : ModelBinder<T> where K : ModelFactory<T>, new()
    {
        public override GetModelAttempt<T> GetModel()
        {
            return new K().GetModel(context);
        }
    }

    public interface ModelFactory<T>
    {
        GetModelAttempt<T> GetModel(HttpContextBase context);
    }

    public class GetModelAttempt<T>
    {
        public bool Success { get; private set; }
        readonly T model = default(T);

        public GetModelAttempt(T model)
        {
            this.model = model;
            Success = true;
        }

        public static bool operator true(GetModelAttempt<T> attempt)
        {
            return attempt.Success;
        }

        public static bool operator false(GetModelAttempt<T> attempt)
        {
            return attempt.Success;
        }

        public static implicit operator T(GetModelAttempt<T> attempt)
        {
            return attempt.model;
        }
    }

    public class AdHocModelFactory<T> : ModelFactory<T>
    {
        Func<HttpContextBase, GetModelAttempt<T>> f;

        public AdHocModelFactory(Func<HttpContextBase, GetModelAttempt<T>> f)
        {
            this.f = f;
        }

        public GetModelAttempt<T> GetModel(HttpContextBase context)
        {
            return f(context);
        }

        
    }   
}
