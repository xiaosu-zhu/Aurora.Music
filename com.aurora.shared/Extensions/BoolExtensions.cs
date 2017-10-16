using System;

namespace Aurora.Shared.Extensions
{
    public static class BoolExtensions
    {
        public static T Execute<T>(this bool b, Func<T> func)
        {
            if (b)
            {
                return func();
            }
            else
            {
                return default(T);
            }
        }
    }
}
