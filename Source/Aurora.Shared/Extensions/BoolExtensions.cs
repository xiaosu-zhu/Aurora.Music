// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
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
