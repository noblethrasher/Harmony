using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prelude;

//TODO: Refactor so that all HtmlElements with a "Text"->String property derive from HtmlTextElement < HtmlElement

namespace Harmony
{
    public static class HTMLHelpers
    {
        public static T WrapIn<T>(this IEnumerable<HtmlElement> xs, T WrappingElement) where T : HtmlElement
        {
            foreach (var x in xs)
                WrappingElement.Children.Add(x);

            return WrappingElement;
        }

        public static T WrapIn<T>(this HtmlElement x, T WrappingElement) where T : HtmlElement
        {
            WrappingElement.Children.Add(x);

            return WrappingElement;
        }

        public static T Wrap<T>(this T elem, IEnumerable<HtmlElement> xs) where T : HtmlElement
        {
            foreach (var x in xs)
                elem.Children.Add(x);

            return elem;
        }

        public static T Wrap<T>(this T parent, HtmlElement child) where T : HtmlElement
        {
            parent.Children.Add(child);

            return parent;
        }

        public static HtmlElementCollection BundleWith(this HtmlElement elem, HtmlElement sibling)
        {
            return new HtmlElementCollection(elem, sibling);
        }
    }    
    
    public class HTMLAttribute
    {
        public string Name { get; private set; }
        public string Value { get; set; }

        public HTMLAttribute(string name, string value)
        {
            Name = name ?? "unknown";
            Value = value;
        }

        public HTMLAttribute(string name) : this(name, null)
        {

        }

        public override string ToString()
        {
            return Name + "=\"" + Value + "\"";
        }

        public static bool operator == (HTMLAttribute x, HTMLAttribute y)
        {
            var obj_x = (object) x;
            var obj_y = (object) y;

            if (obj_x == obj_y)
                return true;

            if (obj_y == null || obj_x == null)
                return false;            
            
            return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);            
        }

        public static bool operator !=(HTMLAttribute x, HTMLAttribute y)
        {
            return !(x == y);
        }

        public override bool Equals(object obj)
        {
            var attr = obj as HTMLAttribute;

            return attr != null && attr == this;
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }

    public class StyleAttribute : HTMLAttribute
    {
        public StyleAttribute() : base("style") { }
    }

    public abstract class StyleRule
    {

    }

    public class Border : StyleRule
    {
        //int Width;
        //string Style;
        //string Color;
    }

    public sealed class HTMLAttributeCollection : IEnumerable<HTMLAttribute>
    {
        List<HTMLAttribute> Attributes = new List<HTMLAttribute>();

        public HTMLAttributeCollection()
        {
            
        }
        
        public HTMLAttributeCollection(IEnumerable<HTMLAttribute> attributes)
        {
            foreach (var attr in attributes)
                Attributes.Add(attr);
        }

        public void Add(HTMLAttribute attribute)
        {
            var attr = Attributes.FirstOrDefault(x => x == attribute);

            if (attr == null)
                Attributes.Add(attribute);
            else
                attr.Value = attribute.Value;
        }

        public bool Remove(HTMLAttribute attribute)
        {
            return Remove(attribute.Name);
        }

        public bool Remove(string Name)
        {
            var attr = Attributes.FirstOrDefault(x => x.Name == Name);

            return Attributes.Remove(attr);
        }

        public IEnumerator<HTMLAttribute> GetEnumerator()
        {
            return Attributes.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }    

    public class HtmlElementCollection : IEnumerable<HtmlElement>
    {

        List<HtmlElement> elements = new List<HtmlElement>();

        public IEnumerator<HtmlElement> GetEnumerator()
        {
            return elements.GetEnumerator();
        }
        
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public HtmlElementCollection() { }

        public HtmlElementCollection(HtmlElement x) { elements.Add(x); }
        public HtmlElementCollection(HtmlElement x, HtmlElement y) { elements.Add(x); elements.Add(y); }
        public HtmlElementCollection(params HtmlElement[] xs) { elements.AddRange(xs); }


        public static implicit operator HtmlElementCollection(string s)
        {
            var xs = new HtmlElementCollection();
            xs.Add(new TextNode(s));

            return xs;
        }

        public static implicit operator string(HtmlElementCollection xs)
        {
            return xs.ToString();
        }

        public void Add(HtmlElement elem)
        {
            elements.Add(elem);
        }

        public bool Remove(HtmlElement elem)
        {
            return elements.Remove(elem);
        }

        public int Count
        {
            get
            {
                return elements.Count;
            }
        }

        public HtmlElement this[int i]
        {
            get
            {
                return elements[i];
            }
        }

        public string ToString(uint n)
        {
            var sb = new StringBuilder(elements.Count);

            var count = elements.Count;
            var tail = count > 1 ? Environment.NewLine : "";
            var head = count > 1 ? "\t".Repeat(n) : "";
            
            for (var i = 0; i < count; i++)
                sb.Append(head + elements[i].ToString(n) + tail);

            var inner = sb.ToString();

            return inner.Length > 0 ? inner  : "";
        }

        public override string ToString()
        {
            return ToString(0);
        }        
    }

    public abstract class HtmlElement
    {
        HTMLAttribute _idAttribute = new HTMLAttribute("id");

        static readonly string[] BLOCK_LEVEL = new[]
            {
                "h1", "h2", "h3", "h4", "h5", "h6",                 
                "ul", "ol", "li", "dir", "menu", "dl", "dt", "dd",
                "table", "thead", "tbody", "tfoot", "th", "td", "tr",
                "p", "pre", "blockquote", "address",
                "div", "center", "form", "hr"
            };

        public string ID
        {
            get
            {
                return _idAttribute.Value;
            }
            set
            {
                _idAttribute.Value = value != null && value.Length > 0 ? value : null;
            }
        }
        
        public abstract string TagName { get; }

        protected virtual bool CanSelfClose
        {
            get
            {
                return true;
            }
        }

        public HtmlElementCollection Children { get; protected set; }
        public HTMLAttributeCollection Attributes { get; protected set; }

        public HtmlElement()
        {
            Attributes = new HTMLAttributeCollection();
            Children = new HtmlElementCollection();


            Attributes.Add(_idAttribute);
            
        }

        public static implicit operator string(HtmlElement elem)
        {
            return elem.ToString();
        }

        public override string ToString()
        {
            return ToString(0);
        }

        public virtual string ToString(uint n)
        {
            var tab = n > 0 && Children.Count > 1 ? "\t".Repeat(n) : "";

            
            var attrs = Attributes.Where(x => x.Value != null).ToList();

            var is_block_level = (BLOCK_LEVEL.Contains(TagName) ? Environment.NewLine : "");

            var text =  is_block_level +
                       
                        "<" + TagName + (attrs.Count > 0 ? attrs.Aggregate(" ", (x, y) => x + y + " ") : "");            

            if (Children != null && Children.Count > 0)
            {
                text += ">" + (Children.Count > 1 ? Environment.NewLine  : "");
                text += Children.ToString(n + 1);
                text += tab + "</" + TagName + ">";

            }
            else
            {
                text += CanSelfClose ? " />" : "></" + TagName + ">";
            }

            return text + is_block_level;
        }
    }

    public sealed class LI : HtmlElement
    {
        public override string TagName
        {
            get { return "li"; }
        }
    }

    public sealed class A : HtmlElement
    {
        private HTMLAttribute HREF = new HTMLAttribute("href");

        protected override bool CanSelfClose
        {
            get
            {
                return false;
            }
        }

        public string Href
        {
            get
            {
                return HREF.Value;
            }
            set
            {
                HREF.Value = value;
            }
        }

        public string Text
        {
            get
            {
                return Children.ToString();
            }
            set
            {
                Children = value;
            }
        }
        
        public A()
        {
            this.Attributes.Add(HREF);            
        }

        public A(string Href) : this()
        {
            HREF.Value = Href;            
        }
        
        public override string TagName
        {
            get { return "a"; }
        }
    }

    public abstract class ListElement : HtmlElement
    {
        public void Add(string text)
        {
            this.Children.Add(new TextNode(text).WrapIn(new LI()));
        }
    }
    
    public sealed class OL : ListElement
    {
        public override string TagName
        {
            get { return "ol"; }
        }
    }

    public sealed class EM : HtmlElement
    {
        public override string TagName
        {
            get { return "em"; }
        }
    }

    public sealed class STRONG : HtmlElement
    {
        public override string TagName
        {
            get { return "strong"; }
        }
    }

    public sealed class UL : ListElement
    {
        public override string TagName
        {
            get { return "ul"; }
        }
    }

    public sealed class P : HtmlElement
    {
        public override string TagName
        {
            get { return "p"; }
        }

        public P(string text)
        {
            this.Children.Add(new TextNode(text));
        }

        public P() { }
    }

    public sealed class IMG : HtmlElement
    {
        private HTMLAttribute SRC = new HTMLAttribute("src");

        public string Src
        {
            get
            {
                return SRC.Value;
            }
            set
            {
                SRC.Value = value;
            }
        }

        public IMG()
        {
            this.Attributes.Add(SRC);
        }

        public IMG(string src) : this()
        {
            SRC.Value = src;
        }
        
        public override string TagName
        {
            get { return "img"; }
        }
    }

    public sealed class TABLE : HtmlElement
    {
        public override string TagName
        {
            get { return "table"; }
        }
    }

    public sealed class TR: HtmlElement
    {
        public override string TagName
        {
            get { return "tr"; }
        }

        public TR(TD x) { this.Children.Add(x); }
        public TR(TD x, TD y) { this.Children.Add(x); this.Children.Add(y); }
        public TR(TD x, TD y, TD z) { this.Children.Add(x); this.Children.Add(y); this.Children.Add(z); }
        public TR(params TD[] xs)
        {
            if (xs != null)
                for (var i = 0; i < xs.Length; i++)
                    this.Children.Add(xs[i]);
        }
    }

    public sealed class TD : HtmlElement
    {
        public override string TagName
        {
            get { return "td"; }
        }

        public TD() { }

        public TD(string s)
        {
            this.Children.Add(new TextNode(s));
        }
    }

    public sealed class TextNode : HtmlElement
    {
        internal string text;

        public TextNode(string s)
        {
            this.text = s;
        }
        
        public override string TagName
        {
            get { return "textnode"; }
        }

        public override string ToString(uint n)
        {

            return text;
        }
    }

    public abstract class FormElement : HtmlElement
    {
        public string Name
        {
            get
            {
                var name_attr = this.Attributes.Where(x => x.Name == "name").FirstOrDefault();

                return name_attr != null ? name_attr.Value : "";
            }
            set
            {
                this.Attributes.Add(new HTMLAttribute("name", value));
            }
        }
    }

    public sealed class Input : FormElement
    {
        public Input(string Value)
        {
            this.Attributes.Add(new HTMLAttribute("type", "text"));
            this.Attributes.Add(new HTMLAttribute("value", Value));
        }

        public Input(string Value, string Type)
        {
            this.Attributes.Add(new HTMLAttribute("type", Type));
            this.Attributes.Add(new HTMLAttribute("value", Value));
        }

        public override string TagName
        {
            get { return "input"; }
        }
    }

    public sealed class Button : FormElement
    {
        public Button(string Text, string Value)
        {
            this.Attributes.Add(new HTMLAttribute("type", "button"));
            this.Attributes.Add(new HTMLAttribute("value", Value));
            this.Children.Add(new TextNode(Text));
        }

        public Button(string Text, string Value, string Type)
        {
            this.Attributes.Add(new HTMLAttribute("type", Type));
            this.Attributes.Add(new HTMLAttribute("value", Value));
            this.Children.Add(new TextNode(Text));
        }

        public Button(string Text)
        {
            this.Attributes.Add(new HTMLAttribute("type", "button"));
            this.Children.Add(new TextNode(Text));
        }

        public override string TagName
        {
            get { return "button"; }
        }
    }

    public sealed class Option : HtmlElement
    {
        public override string TagName
        {
            get { return "option"; }
        }

        TextNode text;

        public Option(string Text, string Value)
        {
            this.Attributes.Add(new HTMLAttribute("value", Value));

            text = new TextNode(Text);

            Children.Add(text);

        }

        public Option(object Text, object Value) : this(Text.ToString(), Value.ToString())
        {
            
        }

        public string Text
        {
            get
            {
                return text.ToString();
            }
            set
            {
                text = new TextNode(value);
            }
        }

        public string Value
        {
            get
            {
                var attr = Attributes.FirstOrDefault(x => x.Name == "value");
                return attr != null ? attr.Value : "";
            }
            set
            {
                Attributes.Add(new HTMLAttribute("value", value));
            }
        }

        const string SELECTED = "selected";

        public bool Selected
        {
            get
            {
                var attr = Attributes.FirstOrDefault(x => x.Name == SELECTED);

                if (attr != null)
                    return attr.Value == SELECTED;
                else
                    return false;
            }
            set
            {
                if (!value)
                    Attributes.Remove(SELECTED);
                else
                    Attributes.Add(new HTMLAttribute(SELECTED, SELECTED));
            }
        }
    }

    public sealed class Select : FormElement
    {
        public Select() { }

        public Select(IEnumerable<Option> opts)
        {
            this.Add(opts);
        }        
        
        public void Add(Option option)
        {
            Children.Add(option);
        }

        public void Add(IEnumerable<Option> options)
        {
            foreach (var opt in options)
                Children.Add(opt);
        }

        public void Remove(Option option)
        {
            Children.Remove(option);
        }
        
        public override string TagName
        {
            get { return "select"; }
        }
    }

    public abstract class Header : HtmlElement
    {
        TextNode text = new TextNode("");
        string level;

        protected Header(string text, string level)
        {
            this.text.text = text;
            this.level = level;

            Children.Add(this.text);
        }

        public override string TagName
        {
            get { return "h" + level; }
        }
    }

    public class H1 : Header
    {
        public H1(string s) : base(s, "1") { }
    }

    public class H2 : Header
    {
        public H2(string s) : base(s, "2") { }
    }

    public class H3 : Header
    {
        public H3(string s) : base(s, "3") { }
    }

    public class H4 : Header
    {
        public H4(string s) : base(s, "4") { }
    }

    public class H5 : Header
    {
        public H5(string s) : base(s, "5") { }
    }

    public class H6 : Header
    {
        public H6(string s) : base(s, "6") { }
    }

    public class Address : HtmlElement
    {
        public override string TagName
        {
            get { return "address"; }
        }
    }
}