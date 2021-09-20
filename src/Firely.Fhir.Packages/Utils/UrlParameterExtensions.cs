using System;
using System.Collections.Specialized;
using System.Text;

namespace Firely.Fhir.Packages
{
    public static class UrlParameterExtensions
    {
        public static void AddWhenValued(this NameValueCollection collection, string name, string? value)
        {
            if (!string.IsNullOrEmpty(value))
                collection.Add(name, value);
        }

        public static string ToQueryString(this NameValueCollection collection)
        {
            var builder = new StringBuilder();

            bool first = true;

            foreach (string key in collection.AllKeys)
            {
                foreach (string value in collection.GetValues(key))
                {
                    if (!first)
                    {
                        builder.Append("&");
                    }

                    builder.AppendFormat("{0}={1}", Uri.EscapeDataString(key), Uri.EscapeDataString(value));

                    first = false;
                }
            }

            return builder.ToString();
        }
        
    }
}
