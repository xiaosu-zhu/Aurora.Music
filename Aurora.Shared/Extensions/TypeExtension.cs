using System;
using System.Reflection;

namespace Aurora.Shared.Extensions
{
    public static class TypeExtension
    {
        public static object GetDefault(this Type type)
        {
            if (type.GetTypeInfo().IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }
    }
}
