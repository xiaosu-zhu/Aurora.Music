using Aurora.Shared.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;

namespace TagLib
{
    public static class UwpFileExtension
    {
        private sealed class UwpFile : TagLib.File.IFileAbstraction
        {
            private readonly IStorageFile file;

            public UwpFile(IStorageFile file) => this.file = file;

            public string Name => file.Path;

            public Stream ReadStream => AsyncHelper.RunSync(async () =>
                {
                    var stream = await file.OpenReadAsync();
                    return stream.AsStreamForRead();
                });

            public Stream WriteStream => AsyncHelper.RunSync(async () =>
                {
                    var stream = await file.OpenAsync(FileAccessMode.ReadWrite);
                    return stream.AsStream();
                });

            public void CloseStream(Stream stream)
            {
                if (stream is null)
                    throw new ArgumentNullException("stream");

                stream.Close();
            }
        }

        public static File.IFileAbstraction AsAbstraction(this IStorageFile file)
        {
            if (file is null)
                throw new ArgumentNullException(nameof(file));

            return new UwpFile(file);
        }
    }
}
