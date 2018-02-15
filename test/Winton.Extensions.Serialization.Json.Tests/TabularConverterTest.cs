using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Winton.Extensions.Serialization.Json
{
    public sealed class TabularConverterTest
    {
        [Fact]
        private void ShouldDeserializeEmpty()
        {
            const string serialized =
                "{\"columns\":[\"String\",\"Long\",\"Int\",\"Ignore\",\"Double\",\"Decimal\",\"Char\",\"Bool\"],\"rows\":[]}";

            JsonConvert.DeserializeObject<Container<Item>>(serialized).Should().BeEquivalentTo(new Container<Item>());
        }

        [Fact]
        private void ShouldDeserializeSingle()
        {
            var deserialized = new Container<Item>
            {
                new Item { Bool = true, Char = '2', Int = 3, Long = 4L, Double = 5.6d, Decimal = 7.8m, String = "90" }
            };

            const string serialized =
                "{\"columns\":[\"String\",\"Long\",\"Int\",\"Ignore\",\"Double\",\"Decimal\",\"Char\",\"Bool\"],\"rows\":[[\"90\",4,3,\"Ignore\",5.6,7.8,\"2\",true]]}";

            JsonConvert.DeserializeObject<Container<Item>>(serialized).Should().BeEquivalentTo(deserialized);
        }

        [Fact]
        private void ShouldDeserializeWithDefaults()
        {
            var deserialized = new Container<Item>
            {
                new Item { Bool = false, Int = 3, Double = 5.6d, String = "90" },
                new Item { Char = '2', Long = 4L, Decimal = 7.8m }
            };

            const string serialized =
                "{\"columns\":[\"String\",\"Long\",\"Int\",\"Ignore\",\"Double\",\"Decimal\",\"Char\",\"Bool\"],\"rows\":[[\"90\",0,3,\"Ignore\",5.6,0.0,\"\\u0000\",false],[null,4,0,\"Ignore\",0.0,7.8,\"2\",false]]}";

            JsonConvert.DeserializeObject<Container<Item>>(serialized).Should().BeEquivalentTo(deserialized);
        }

        [Fact]
        private void ShouldSerializeEmpty()
        {
            const string serialized =
                "{\"columns\":[\"Bool\",\"Char\",\"Decimal\",\"Double\",\"Int\",\"Long\",\"String\"],\"rows\":[]}";

            var deserialized = new Container<Item>();

            JsonConvert.SerializeObject(deserialized).Should().BeEquivalentTo(serialized);
        }

        [Fact]
        private void ShouldSerializeSingle()
        {
            const string serialized =
                "{\"columns\":[\"Bool\",\"Char\",\"Decimal\",\"Double\",\"Int\",\"Long\",\"String\"],\"rows\":[[true,\"2\",7.8,5.6,3,4,\"90\"]]}";

            var deserialized = new Container<Item>
            {
                new Item { Bool = true, Char = '2', Int = 3, Long = 4L, Double = 5.6d, Decimal = 7.8m, String = "90" }
            };

            JsonConvert.SerializeObject(deserialized).Should().BeEquivalentTo(serialized);
        }

        [Fact]
        private void ShouldSerializeWithDefaults()
        {
            const string serialized =
                "{\"columns\":[\"Bool\",\"Char\",\"Decimal\",\"Double\",\"Int\",\"Long\",\"String\"],\"rows\":[[false,\"\\u0000\",0.0,5.6,3,0,\"90\"],[false,\"2\",7.8,0.0,0,4,null]]}";

            var deserialized = new Container<Item>
            {
                new Item { Bool = false, Int = 3, Double = 5.6d, String = "90" },
                new Item { Char = '2', Long = 4L, Decimal = 7.8m }
            };

            JsonConvert.SerializeObject(deserialized).Should().BeEquivalentTo(serialized);
        }

        [JsonConverter(typeof(TabularConverter))]
        private sealed class Container<T> : List<T>
        {
        }

        [SuppressMessage(
            "ReSharper",
            "UnusedAutoPropertyAccessor.Local",
            Justification = "Accessors not used but needed implicitly by FluentAssertions ShouldBeEquivalentTo.")]
        private sealed class Item
        {
            public bool Bool { get; set; }

            public char Char { get; set; }

            public decimal Decimal { get; set; }

            public double Double { get; set; }

            public int Int { get; set; }

            public long Long { get; set; }

            public string String { get; set; }
        }
    }
}