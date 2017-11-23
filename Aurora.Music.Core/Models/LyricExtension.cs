using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.ApplicationModel.AppExtensions;
using Windows.Foundation.Collections;

namespace Aurora.Music.Core.Models
{
    public class LyricExtension : Extension
    {
        public LyricExtension(AppExtension ext, PropertySet properties) : base(ext, properties)
        {

        }

        public override void Execute(string str)
        {
            throw new NotImplementedException();
        }
    }
}
