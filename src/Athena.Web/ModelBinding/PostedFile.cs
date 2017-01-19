using System.IO;

namespace Athena.Web.ModelBinding
{
    public class PostedFile
    {
        public PostedFile(string name, string contentType, Stream content)
        {
            Name = name;
            ContentType = contentType;
            Content = content;
        }

        public string Name { get; private set; }
        public string ContentType { get; private set; }
        public Stream Content { get; private set; }
    }
}