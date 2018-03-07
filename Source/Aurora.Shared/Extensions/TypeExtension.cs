// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

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

        public static IEnumerable<(Type, T[])> GetTypesWithAttribute<T>(this Assembly assembly) where T : Attribute
        {
            foreach (Type type in assembly.GetTypes())
            {
                var attributes = type.GetTypeInfo().GetCustomAttributes(typeof(T), true);
                if (attributes.Count() > 0)
                {
                    yield return (type, attributes.Cast<T>().ToArray());
                }
            }
        }


        public static async Task<List<Assembly>> GetAssemblyListAsync()
        {
            List<Assembly> assemblies = new List<Assembly>();

            var files = await Windows.ApplicationModel.Package.Current.InstalledLocation.GetFilesAsync();
            if (files == null)
                return assemblies;

            foreach (var file in files.Where(file => file.FileType == ".dll" || file.FileType == ".exe"))
            {
                try
                {
                    assemblies.Add(Assembly.Load(new AssemblyName(file.DisplayName)));
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine(ex.Message);
                }

            }

            return assemblies;
        }
    }
}
