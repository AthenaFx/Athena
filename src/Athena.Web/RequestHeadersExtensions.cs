using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Athena.Web
{
    using RequestHeaders = WebEnvironmentExtensions.WebRequest.RequestHeaders;

    public static class RequestHeadersExtensions
    {
        public static IReadOnlyCollection<AcceptedMediaType> GetAcceptedMediaTypes(this RequestHeaders requestHeaders)
        {
            var acceptHeaders = requestHeaders.Accept.Split(',');
            var position = 1;

            var result = new List<AcceptedMediaType>();

            foreach (var acceptHeader in acceptHeaders)
            {
                var parts = acceptHeader.Split(';');
                var currentPosition = position;
                position++;

                if(!parts.Any())
                    continue;

                if (parts.Length < 2)
                {
                    result.Add(new AcceptedMediaType(parts[0], currentPosition));
                    continue;
                }

                if(!parts[1].StartsWith("q=", StringComparison.OrdinalIgnoreCase))
                    continue;

                double quality;

                result.Add(double.TryParse(parts[1].Substring(2), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out quality)
                    ? new AcceptedMediaType(parts[0], currentPosition, quality)
                    : new AcceptedMediaType(parts[0], currentPosition));
            }

            return new ReadOnlyCollection<AcceptedMediaType>(result);
        }
    }

    public class AcceptedMediaType
    {
        public AcceptedMediaType(string name, int position, double quality = 1)
        {
            Name = name.Trim();
            Position = position;
            Quality = quality;
        }

        public string Name { get; private set; }
        public int Position { get; private set; }
        public double Quality { get; private set; }

        public bool Matches(string mediaType)
        {
            if (Name == "*/*" || Name == "*")
                return true;

            if (Name.Equals(mediaType, StringComparison.OrdinalIgnoreCase))
                return true;

            var mediaTypeParts = mediaType.Split('/');
            var nameParts = Name.Split('/');

            if (mediaType.Length < 2 || nameParts.Length < 2)
                return false;

            if (Name.StartsWith("*/") && mediaTypeParts[1] == nameParts[1])
                return true;

            if (Name.EndsWith("/*") && mediaTypeParts[0] == nameParts[0])
                return true;

            return false;
        }

        public double GetPriority()
        {
            return (1000 / Quality) + Position;
        }
    }
}