using FluentAssertions;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Hl7.Fhir.Validation;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Hl7.Fhir.Support.Poco.Tests
{
    [TestClass]
    public class FhirJsonDeserializationStreamTests
    {
        [TestMethod]
        public async Task MyTestMethod()
        {
            var filename = Path.Combine("TestData", "json-edge-cases.json");
            using var stream = File.Open(filename, FileMode.Open);

            var options = new JsonSerializerOptions().ForFhir(typeof(TestPatient).Assembly);

            Func<Task> act = async () => await JsonSerializer.DeserializeAsync<TestPatient>(stream, options);

            await act.Should().ThrowAsync<DeserializationFailedException>()
                .Where(ex => ex.Exceptions.All(e => e.ErrorCode == CodedValidationException.CONTAINED_RESOURCE_CANNOT_HAVE_NARRATIVE_CODE));
        }


        [TestMethod]
        public void DeserializeCorrectPatientFromStream()
        {
            var filename = Path.Combine("TestData", "TestPatient.json");
            using var stream = File.Open(filename, FileMode.Open);

            FhirJsonPocoDeserializer deserializer = new(typeof(TestPatient).Assembly, new() { DefaultBufferSize = 64 });

            Resource resource = null;

            Action act = () => resource = deserializer.DeserializeResource(stream);

            act.Should().NotThrow();
            resource.Should().NotBeNull();
        }

        [TestMethod]
        public async Task MyTestMethod2Async()
        {
            var options = new JsonSerializerOptions().ForFhir(typeof(TestPatient).Assembly);
            options.DefaultBufferSize = 512;

            var client = new HttpClient()
            {
                BaseAddress = new Uri("https://server.fire.ly")
            };

            using var stream = await client.GetStreamAsync("Patient/7f62bdb7-bd2f-4da0-bd8e-d03461627557");

            // var buffer = new byte[1024];

            //var bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length);

            //            var str = Encoding.UTF8.GetString(buffer, 0, bytesRead);

            var patient = await JsonSerializer.DeserializeAsync<TestPatient>(new StreamWrapper(stream), options);

            patient.Should().NotBeNull();
        }

        internal class StreamWrapper : Stream
        {
            private readonly Stream _stream;

            public StreamWrapper(Stream stream)
            {
                _stream = stream;
            }

            public override bool CanRead => _stream.CanRead;

            public override bool CanSeek => _stream.CanSeek;

            public override bool CanWrite => _stream.CanWrite;

            public override long Length => _stream.Length;

            public override long Position { get => _stream.Position; set => _stream.Position = value; }

            public override void Flush() => _stream.Flush();
            public override int Read(byte[] buffer, int offset, int count)
            {
                var bytesRead = _stream.Read(buffer, offset, count);
                return bytesRead;
            }

            public override long Seek(long offset, SeekOrigin origin) => _stream.Seek(offset, origin);
            public override void SetLength(long value) => _stream.SetLength(value);
            public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);

            public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
            {
                var bytesRead = base.ReadAsync(buffer, offset, count, cancellationToken);
                return bytesRead;
            }
        }
    }
}