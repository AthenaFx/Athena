﻿using System.IO;

namespace Athena.Web
{
    public class ParsingResult
    {
        public ParsingResult(string contentType, Stream body)
        {
            ContentType = contentType;
            Body = body;
        }

        public string ContentType { get; }
        public Stream Body { get; }
    }
}