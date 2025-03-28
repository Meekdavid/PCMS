using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tests.Shared
{
    public class FilesDal
    {
    }

    public class FileModel
    {
        public Stream Content { get; }
        public string ContentType { get; }
        public string FileName { get; }

        public FileModel(Stream content, string contentType, string fileName)
        {
            Content = content ?? throw new ArgumentNullException(nameof(content));
            ContentType = contentType ?? throw new ArgumentNullException(nameof(contentType));
            FileName = fileName ?? throw new ArgumentNullException(nameof(fileName));
        }
    }
}
