using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Harmony
{
    public abstract class InitializablePage<T> : System.Web.UI.Page
    {
        public void Initialize(Action<InitializablePage<T>> action)
        {
            action(this);
        }
    }
}
