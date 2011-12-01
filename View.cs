using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.UI;

namespace Harmony
{
    public class View<T> : Page
    {
        public T Model { get; set; }
    }
}
