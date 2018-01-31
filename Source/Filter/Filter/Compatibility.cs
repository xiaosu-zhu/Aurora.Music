// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
#if (PORTABLE || NET35)
namespace MathNet.Filtering
{
    using System;

    [AttributeUsage(AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    internal class TargetedPatchingOptOutAttribute : Attribute
    {
        public string Reason { get; private set; }

        public TargetedPatchingOptOutAttribute(string reason)
        {
            Reason = reason;
        }
    }
}
#endif