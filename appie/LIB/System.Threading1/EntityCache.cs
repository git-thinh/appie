using System;
using System.Collections.Generic;
using System.Text;

namespace System.Threading
{
    static class EntityCache<T>
    {
        static Dictionary<int, T> _users = new Dictionary<int, T>();

        internal static T GetUser(int id)
        {
            //T u = null;
            T u = default(T);

            lock (_users)
                if (_users.TryGetValue(id, out u))
                    return u;

            u = RetrieveUser(id);   // Method to retrieve user from database
            lock (_users) _users[id] = u;
            return u;
        }

        // User is a custom class with fields for user data
        internal static T RetrieveUser(int id) { return default(T); }
    }
}
