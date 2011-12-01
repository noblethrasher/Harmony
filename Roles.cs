using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Prelude;
using System.Security.Principal;

namespace Harmony
{
    public abstract class Role
    {
        public static RoleCollection operator +(Role x, Role y)
        {
            return new RoleCollection(x, y);
        }
    }

    public class RoleCollection : IEnumerable<Role>
    {
        HashSet<Role> roles = new HashSet<Role>(new AdhocEqualityComparer<Role>((x, y) => x.GetType() == y.GetType(), x => x.GetHashCode()));

        public RoleCollection()
        {
            
        }
        
        public RoleCollection(Role x)
        {
            Add(x);
        }

        public RoleCollection(Role x, Role y)
        {
            Add(x);
            Add(y);
        }

        public RoleCollection(Role x, Role y, Role z)
        {
            Add(x);
            Add(y);
            Add(z);
        }

        public RoleCollection(params Role[] xs)
        {
            if(xs != null)
                for(var i = 0; i < xs.Length; i++)
                    Add(xs[i]);
        }

        public void Add(Role role)
        {
            if (role != null)
                roles.Add(role);
        }

        public void AddRange(IEnumerable<Role> roles)
        {
            if(roles != null)
                foreach (var role in roles)
                    Add(role);
        }

        public T GetRole<T>() where T : Role
        {
            return (T) roles.FirstOrDefault(x => x.GetType() == typeof(T));
        }

        public IEnumerator<Role> GetEnumerator()
        {
            return roles.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public class User : IPrincipal, IIdentity
    {
        public RoleCollection Roles { get; protected set; }
        
        public User(string name, string authenticationType, bool isAuthenticated) : this(name, authenticationType, isAuthenticated, null)
        {
            
        }

        public User(string name, string authenticationType, bool isAuthenticated, IEnumerable<Role> roles)
        {
            Roles = new RoleCollection();
            
            if(roles != null)
                Roles.AddRange(roles);

            Name = name;
            AuthenticationType = authenticationType;
            IsAuthenticated = isAuthenticated;
        }

        public Guid Session { get; protected set; }

        public IIdentity Identity
        {
            get { return this; }
        }

        public virtual bool IsInRole(string role)
        {
            return false;
        }

        public virtual string AuthenticationType
        {
            get;
            protected set;
        }

        public virtual bool IsAuthenticated
        {
            get;
            protected set;
        }

        public virtual string Name
        {
            get;
            protected set;
        }
    }    
}
