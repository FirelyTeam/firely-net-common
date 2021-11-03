/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System;
using System.Reflection;
using System.Text.Json;
#nullable enable

namespace Hl7.Fhir.Serialization
{
    internal static class SystemTextJsonParsingExtensions
    {
        internal static (long lineNumber, long position) GetLocation(this ref Utf8JsonReader reader)
        {
            var lineNumber = ((long)typeof(JsonReaderState)
                .GetField("_lineNumber", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(reader.CurrentState)!) + 1;
            var position = ((long)typeof(JsonReaderState)
                .GetField("_bytePositionInLine", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(reader.CurrentState)!) + 1;

            return (lineNumber, position);
        }

        internal static string GenerateLocationMessage(this ref Utf8JsonReader reader) =>
            GenerateLocationMessage(ref reader, out var _, out var _);

        internal static string GenerateLocationMessage(this ref Utf8JsonReader reader, out long lineNumber, out long position)
        {
            // While we are waiting for this https://github.com/dotnet/runtime/issues/28482,
            // there's no other option than to just force our way to these valuable properties.
            // Note: linenumber/position are 0 based, so adding 1 position here.
            (lineNumber, position) = reader.GetLocation();
            return $" Line {lineNumber}, position {position}.";
        }


        public static bool TryGetNumber(this ref Utf8JsonReader reader, out object? value)
        {
            value = null;

            var gotValue = reader.TryGetDecimal(out var dec) && (value = dec) is { };
            if (!gotValue) gotValue = reader.TryGetUInt64(out var uint64) && (value = uint64) is { };
            if (!gotValue) gotValue = reader.TryGetInt64(out var int64) && (value = int64) is { };
            if (!gotValue) gotValue = reader.TryGetDouble(out var dbl) && double.IsNormal(dbl) && (value = dbl) is { };

            return gotValue;
        }


        public static void Recover(this ref Utf8JsonReader reader)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    return;
                case JsonTokenType.Null:
                case JsonTokenType.Number or JsonTokenType.String:
                case JsonTokenType.True or JsonTokenType.False:
                    reader.Read();
                    return;
                case JsonTokenType.PropertyName:
                    SkipTo(ref reader, JsonTokenType.PropertyName);
                    return;
                case JsonTokenType.StartArray:
                    SkipTo(ref reader, JsonTokenType.EndArray);
                    reader.Read();
                    return;
                case JsonTokenType.StartObject:
                    SkipTo(ref reader, JsonTokenType.EndObject);
                    reader.Read();
                    return;
                default:
                    throw new InvalidOperationException($"Cannot recover, aborting. Token {reader.TokenType} was unexpected at this point. " +
                        reader.GenerateLocationMessage());
            }
        }

        public static void SkipTo(this ref Utf8JsonReader reader, JsonTokenType tt)
        {
            var depth = reader.CurrentDepth;

            while (reader.Read() && reader.CurrentDepth >= depth)
            {
                if (reader.CurrentDepth == depth && reader.TokenType == tt) break;
            }
        }
    }


}

#nullable restore
#endif