using System;
using System.Collections.Generic;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace Winton.Extensions.Serialization.Json
{
    public class SingleValueConverterTests
    {
        [JsonConverter(typeof(SingleValueConverter))]
        public struct DecimalValue
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly decimal _value;

            public DecimalValue(decimal value)
            {
                _value = value;
            }
        }

        [JsonConverter(typeof(SingleValueConverter))]
        public struct IntValue
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly int _value;

            public IntValue(int value)
            {
                _value = value;
            }
        }

        [JsonConverter(typeof(SingleValueConverter))]
        public struct MultipleBackingFields
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly int _value1;
#pragma warning disable 414
            private readonly int _value2;
#pragma warning restore 414

            // ReSharper disable once UnusedParameter.Local
            public MultipleBackingFields(int value)
            {
                _value1 = value;
                _value2 = 0;
            }
        }

        [JsonConverter(typeof(SingleValueConverter))]
        public struct NoBackingField
        {
            // ReSharper disable once UnusedParameter.Local
            public NoBackingField(int value)
            {
            }
        }

        [JsonConverter(typeof(SingleValueConverter))]
        public struct OnlyMultipleParameterConstructor
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly int _value1;

            // ReSharper disable once UnusedParameter.Local
            public OnlyMultipleParameterConstructor(int value1, int value2)
            {
                _value1 = value1;
            }
        }

        [JsonConverter(typeof(SingleValueConverter))]
        public struct OnlyParameterlessConstructor
        {
#pragma warning disable 169
            private readonly int _value;
#pragma warning restore 169
        }

        [JsonConverter(typeof(SingleValueConverter))]
        public struct OnlySingleParameterConstructorTakingWrongType
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly int _value;

            public OnlySingleParameterConstructorTakingWrongType(string value)
            {
                _value = int.Parse(value);
            }
        }

        [JsonConverter(typeof(SingleValueConverter))]
        public struct StringValue
        {
            // ReSharper disable once NotAccessedField.Local
            private readonly string _value;

            public StringValue(string value)
            {
                _value = value;
            }
        }

        public sealed class CanConvert : SingleValueConverterTests
        {
            [Theory]
            [InlineData(typeof(decimal))]
            [InlineData(typeof(decimal?))]
            [InlineData(typeof(int))]
            [InlineData(typeof(int?))]
            [InlineData(typeof(string))]
            [InlineData(typeof(object))]
            [InlineData(typeof(DecimalValue))]
            [InlineData(typeof(DecimalValue?))]
            [InlineData(typeof(IntValue))]
            [InlineData(typeof(IntValue?))]
            [InlineData(typeof(StringValue))]
            [InlineData(typeof(StringValue?))]
            private void ShouldReturnTrueForAnyType(Type type)
            {
                var singleValueConverter = new SingleValueConverter();

                bool canConvert = singleValueConverter.CanConvert(type);

                canConvert.Should().BeTrue();
            }
        }

        public sealed class ReadJson : SingleValueConverterTests
        {
            public static IEnumerable<object[]> ReadTestCases => new List<object[]>
            {
                new object[]
                {
                    "\"test\"",
                    new StringValue("test"),
                    typeof(StringValue)
                },
                new object[]
                {
                    "\"test\"",
                    (StringValue?)new StringValue("test"),
                    typeof(StringValue?)
                },
                new object[]
                {
                    "null",
                    null,
                    typeof(StringValue?)
                },
                new object[]
                {
                    "1",
                    new IntValue(1),
                    typeof(IntValue)
                },
                new object[]
                {
                    "1",
                    (IntValue?)new IntValue(1),
                    typeof(IntValue?)
                },
                new object[]
                {
                    "null",
                    null,
                    typeof(IntValue?)
                },
                new object[]
                {
                    "1.3",
                    new DecimalValue(1.3M),
                    typeof(DecimalValue)
                },
                new object[]
                {
                    "1.3",
                    (DecimalValue?)new DecimalValue(1.3M),
                    typeof(DecimalValue?)
                },
                new object[]
                {
                    "null",
                    null,
                    typeof(DecimalValue?)
                }
            };

            [Theory]
            [MemberData(nameof(ReadTestCases))]
            private void ShouldRead(string value, object expected, Type type)
            {
                object actual = JsonConvert.DeserializeObject(value, type);

                actual.Should().Be(expected);
            }

            [Fact]
            private void ShouldThrowIfTypeHasMultipleBackingFields()
            {
                Action reading = () => JsonConvert.DeserializeObject<MultipleBackingFields>("1");

                reading.Should().Throw<JsonSerializationException>()
                       .WithMessage("SingleValueConverter can only be used on types with a single backing field.");
            }

            [Fact]
            private void ShouldThrowIfTypeHasNoBackingField()
            {
                Action reading = () => JsonConvert.DeserializeObject<NoBackingField>("1");

                reading.Should().Throw<JsonSerializationException>()
                       .WithMessage("SingleValueConverter can only be used on types with a single backing field.");
            }

            [Fact]
            private void ShouldThrowIfTypeHasOnlyMultipleParameterConstructor()
            {
                Action reading = () => JsonConvert.DeserializeObject<OnlyMultipleParameterConstructor>("1");

                reading.Should().Throw<JsonSerializationException>()
                       .WithMessage(
                           "SingleValueConverter can only be used on types with a constructor taking a single parameter of the same type as its backing field.");
            }

            [Fact]
            private void ShouldThrowIfTypeHasOnlyParameterlessConstructor()
            {
                Action reading = () => JsonConvert.DeserializeObject<OnlyParameterlessConstructor>("1");

                reading.Should().Throw<JsonSerializationException>()
                       .WithMessage(
                           "SingleValueConverter can only be used on types with a constructor taking a single parameter of the same type as its backing field.");
            }

            [Fact]
            private void ShouldThrowIfTypeHasOnlySingleParameterConstructorTakingWrongType()
            {
                Action reading = () =>
                    JsonConvert.DeserializeObject<OnlySingleParameterConstructorTakingWrongType>("1");

                reading.Should().Throw<JsonSerializationException>()
                       .WithMessage(
                           "SingleValueConverter can only be used on types with a constructor taking a single parameter of the same type as its backing field.");
            }
        }

        public sealed class WriteJson : SingleValueConverterTests
        {
            public static IEnumerable<object[]> WriteTestCases => new List<object[]>
            {
                new object[]
                {
                    new StringValue("test"),
                    "\"test\""
                },
                new object[]
                {
                    (StringValue?)new StringValue("test"),
                    "\"test\""
                },
                new object[]
                {
                    // ReSharper disable once RedundantCast
                    (StringValue?)null,
                    "null"
                },
                new object[]
                {
                    new IntValue(1),
                    "1"
                },
                new object[]
                {
                    (IntValue?)new IntValue(1),
                    "1"
                },
                new object[]
                {
                    // ReSharper disable once RedundantCast
                    (IntValue?)null,
                    "null"
                },
                new object[]
                {
                    new DecimalValue(1.3M),
                    "1.3"
                },
                new object[]
                {
                    (DecimalValue?)new DecimalValue(1.3M),
                    "1.3"
                },
                new object[]
                {
                    // ReSharper disable once RedundantCast
                    (DecimalValue?)null,
                    "null"
                },
                new object[]
                {
                    null,
                    "null"
                }
            };

            [Fact]
            private void ShouldThrowIfTypeHasMultipleBackingFields()
            {
                Action writing = () => JsonConvert.SerializeObject(new MultipleBackingFields(1));

                writing.Should().Throw<JsonSerializationException>()
                       .WithMessage("SingleValueConverter can only be used on types with a single backing field.");
            }

            [Fact]
            private void ShouldThrowIfTypeHasNoBackingField()
            {
                Action writing = () => JsonConvert.SerializeObject(new NoBackingField(1));

                writing.Should().Throw<JsonSerializationException>()
                       .WithMessage("SingleValueConverter can only be used on types with a single backing field.");
            }

            [Theory]
            [MemberData(nameof(WriteTestCases))]
            private void ShouldWrite(object obj, string expected)
            {
                string actual = JsonConvert.SerializeObject(obj);

                actual.Should().Be(expected);
            }
        }
    }
}