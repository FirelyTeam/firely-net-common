/* 
 * Copyright (c) 2021, Firely (info@fire.ly) and contributors
 * See the file CONTRIBUTORS for details.
 * 
 * This file is licensed under the BSD 3-Clause license
 * available at https://raw.githubusercontent.com/FirelyTeam/firely-net-sdk/master/LICENSE
 */


#if NETSTANDARD2_0_OR_GREATER || NET5_0_OR_GREATER
using System;
using System.IO;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Text.Json;
#nullable enable

namespace Hl7.Fhir.Serialization
{
    internal static class SystemTextJsonParsingExtensions
    {
        // While we are waiting for this https://github.com/dotnet/runtime/issues/28482,
        // there's no other option than to just force our way to these valuable properties.
        private static readonly Lazy<Func<JsonReaderState, long>> GETLINENUMBER =
            new(() => Utility.PropertyInfoExtensions.GetField<JsonReaderState, long>("_lineNumber"));
        private static readonly Lazy<Func<JsonReaderState, long>> GETPOSITION =
            new(() => Utility.PropertyInfoExtensions.GetField<JsonReaderState, long>("_bytePositionInLine"));

        internal static string GetRawText(this ref Utf8JsonReader reader)
        {
            var doc = JsonDocument.ParseValue(ref reader);
            return doc.RootElement.GetRawText();
        }

        internal static (long lineNumber, long position) GetLocation(this JsonReaderState state)
        {
            // Note: linenumber/position are 0 based, so adding 1 position here.
            var lineNumber = GETLINENUMBER.Value(state) + 1;
            var position = GETPOSITION.Value(state) + 1;
            return (lineNumber, position);
        }

        internal static (long lineNumber, long position) GetLocation(this ref Utf8JsonReader reader) =>
            reader.CurrentState.GetLocation();

        internal static string GenerateLocationMessage(this ref Utf8JsonReader reader) =>
            GenerateLocationMessage(ref reader, out var _, out var _);

        internal static string GenerateLocationMessage(this ref Utf8JsonReader reader, out long lineNumber, out long position)
        {
            (lineNumber, position) = reader.GetLocation();
            return $"line {lineNumber}, position {position}.";
        }


        public static bool TryGetNumber(this ref Utf8JsonReader reader, out object? value)
        {
            value = null;

            var gotValue = reader.TryGetDecimal(out var dec) && (value = dec) is { };
            if (!gotValue) gotValue = reader.TryGetUInt64(out var uint64) && (value = uint64) is { };
            if (!gotValue) gotValue = reader.TryGetInt64(out var int64) && (value = int64) is { };
            if (!gotValue) gotValue = reader.TryGetDouble(out var dbl) && dbl.IsNormal() && (value = dbl) is { };

            return gotValue;
        }

#if NETSTANDARD2_0
        internal static bool IsNormal(this float f) => !float.IsNaN(f) && !float.IsInfinity(f);
        internal static bool IsNormal(this double d) => !double.IsNaN(d) && !double.IsInfinity(d);
#else
        internal static bool IsNormal(this float f) => float.IsNormal(f);
        internal static bool IsNormal(this double d) => double.IsNormal(d);
#endif

        public static void Recover(this ref Utf8JsonReader reader, FhirJsonPocoDeserializerState state)
        {
            switch (reader.TokenType)
            {
                case JsonTokenType.None:
                    return;
                case JsonTokenType.Null:
                case JsonTokenType.Number or JsonTokenType.String:
                case JsonTokenType.True or JsonTokenType.False:
                    reader.ReadInternal(state);
                    return;
                case JsonTokenType.PropertyName:
                    SkipTo(ref reader, JsonTokenType.PropertyName, state);
                    return;
                case JsonTokenType.StartArray:
                    SkipTo(ref reader, JsonTokenType.EndArray, state);
                    reader.ReadInternal(state);
                    return;
                case JsonTokenType.StartObject:
                    SkipTo(ref reader, JsonTokenType.EndObject, state);
                    reader.ReadInternal(state);
                    return;
                default:
                    throw new InvalidOperationException($"Cannot recover, aborting. Token {reader.TokenType} was unexpected at this point. " +
                        reader.GenerateLocationMessage());
            }
        }

        public static void SkipTo(this ref Utf8JsonReader reader, JsonTokenType tt, FhirJsonPocoDeserializerState state)
        {
            var depth = reader.CurrentDepth;

            while (reader.ReadInternal(state) && reader.CurrentDepth >= depth)
            {
                if (reader.CurrentDepth == depth && reader.TokenType == tt) break;
            }
        }

        public static bool ReadInternal(this ref Utf8JsonReader reader, FhirJsonPocoDeserializerState? state = null)
        {
            if (state?.BufferState is not null)
            {
                if (state.BufferState.IsFirstIteration)
                    reader.readFirstBlockFromStream(state.BufferState);

                while (!reader.Read())
                {
                    if (reader.IsFinalBlock) return false;
                    reader.readNextBlockFromStream(state.BufferState);
                }
                return true;
            }
            else
            {
                return reader.Read();
            }
        }

        private static ReadOnlySpan<byte> Utf8Bom => new byte[] { 0xEF, 0xBB, 0xBF };

        private static void readFirstBlockFromStream(this ref Utf8JsonReader reader, ReadBufferState state)
        {
            state.IsFirstIteration = false;

            // read the first block
            int bytesRead = state.ReadToBuffer(0, state.Buffersize);

            var span = state.Buffer.AsSpan();

            // Read past the UTF-8 BOM bytes if a BOM exists.
            span = span.StartsWith(SystemTextJsonParsingExtensions.Utf8Bom)
                ? span.Slice(SystemTextJsonParsingExtensions.Utf8Bom.Length, bytesRead - SystemTextJsonParsingExtensions.Utf8Bom.Length)
                : span.Slice(0, bytesRead);


            reader = new(span, bytesRead < state.Buffersize, default);
        }

        private static void readNextBlockFromStream(this ref Utf8JsonReader reader, ReadBufferState state)
        {
            int contentLength;
            int bytesRead;

            if (reader.BytesConsumed < state.Buffersize)
            {
                var bufferSpan = state.Buffer.AsSpan();
                var bytesConsumed = checked((int)reader.BytesConsumed);

                if (bufferSpan.StartsWith(Utf8Bom))
                {
                    bytesConsumed += Utf8Bom.Length;
                }

                ReadOnlySpan<byte> leftover = state.Buffer.AsSpan(bytesConsumed);

                if (leftover.Length == state.Buffersize)
                {
                    // resize the buffer, because the reader could not read the whole token
                    state.ResizeBuffer(state.Buffersize * 2);

                }

                leftover.CopyTo(state.Buffer);
                bytesRead = state.ReadToBuffer(leftover.Length, state.Buffersize - leftover.Length);
                contentLength = bytesRead == 0 ? 0 : bytesRead + leftover.Length;
            }
            else
            {
                bytesRead = state.ReadToBuffer(0, state.Buffersize);
                contentLength = bytesRead;
            }

            reader = new(state.Buffer.AsSpan(0, contentLength), bytesRead == 0, reader.CurrentState);
        }
    }
}

#nullable restore
#endif