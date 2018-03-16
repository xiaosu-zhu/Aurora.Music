// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aurora.Shared.Helpers
{
    public enum ActivateUsage { Navigation, SubNavigation, Query }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    public sealed class UriActivateAttribute : Attribute
    {
        // See the attribute guidelines at 
        //  http://go.microsoft.com/fwlink/?LinkId=85236
        readonly string relative;

        // This is a positional argument
        public UriActivateAttribute(string path)
        {
            this.relative = path;

            // TODO: Implement code here

        }

        public string Relative
        {
            get { return relative; }
        }

        public ActivateUsage Usage { get; set; }

        public bool CanShowInNewWindow { get; set; }
    }
}
