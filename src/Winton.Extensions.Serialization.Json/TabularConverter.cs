using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Winton.Extensions.Serialization.Json
{
    public sealed class TabularConverter : JsonConverter
    {
        private readonly string _columnsProperty;
        private readonly string _rowsProperty;

        public TabularConverter()
            : this("columns")
        {
        }

        public TabularConverter(string columnsProperty = "columns", string rowsProperty = "rows")
        {
            _columnsProperty = columnsProperty;
            _rowsProperty = rowsProperty;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IList).GetTypeInfo().IsAssignableFrom(objectType.GetTypeInfo());
        }

        public override object ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            var columns = new List<string>();
            var rows = new List<List<object>>();

            if (reader.TokenType != JsonToken.StartObject)
            {
                throw new JsonSerializationException("Expects an object.");
            }

            while (reader.Read() && reader.TokenType == JsonToken.PropertyName)
            {
                var name = (string)reader.Value;

                if (name == _columnsProperty)
                {
                    if (reader.Read() && reader.TokenType == JsonToken.StartArray)
                    {
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            columns.Add((string)reader.Value);
                        }
                    }
                    else
                    {
                        throw new JsonSerializationException(
                            $"Expects '{_columnsProperty}' property to be an array of strings.");
                    }
                }
                else if (name == _rowsProperty)
                {
                    if (reader.Read() && reader.TokenType == JsonToken.StartArray)
                    {
                        while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                        {
                            if (reader.TokenType == JsonToken.StartArray)
                            {
                                var row = new List<object>();

                                while (reader.Read() && reader.TokenType != JsonToken.EndArray)
                                {
                                    row.Add(serializer.Deserialize(reader));
                                }

                                rows.Add(row);
                            }
                            else
                            {
                                throw new JsonSerializationException(
                                    $"Expects '{_rowsProperty}' property to be an array of arrays.");
                            }
                        }
                    }
                    else
                    {
                        throw new JsonSerializationException(
                            $"Expects '{_rowsProperty}' property to be an array of arrays.");
                    }
                }
                else
                {
                    throw new JsonSerializationException(
                        $"Expects an object with properties '{_columnsProperty}' and '{_rowsProperty}'.");
                }
            }

            if (reader.TokenType != JsonToken.EndObject)
            {
                throw new JsonSerializationException("Object is malformed.");
            }

            Type type = objectType.GenericTypeArguments.Single();
            PropertyInfo[] properties = columns
                .GroupJoin(type.GetTypeInfo().DeclaredProperties, x => x, y => y.Name, (x, y) => y.SingleOrDefault())
                .ToArray();
            var list = (IList)Activator.CreateInstance(objectType);

            foreach (List<object> row in rows)
            {
                object element = Activator.CreateInstance(type);

                foreach ((PropertyInfo property, object value) in properties.Zip(row, Tuple.Create))
                {
                    if (property != null)
                    {
                        object typedValue =
                            value != null && type.GetTypeInfo().IsAssignableFrom(value.GetType().GetTypeInfo())
                                ? value
                                : Convert.ChangeType(value, property.PropertyType);

                        property.SetValue(element, typedValue);
                    }
                }

                list.Add(element);
            }

            return list;
        }

        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            Type type = value.GetType().GenericTypeArguments.Single();
            PropertyInfo[] properties = type.GetTypeInfo().DeclaredProperties.ToArray();
            var list = (IList)value;

            writer.WriteStartObject();

            writer.WritePropertyName(_columnsProperty);
            writer.WriteStartArray();

            foreach (PropertyInfo property in properties)
            {
                writer.WriteValue(property.Name);
            }

            writer.WriteEndArray();

            writer.WritePropertyName(_rowsProperty);
            writer.WriteStartArray();

            foreach (object element in list)
            {
                writer.WriteStartArray();

                foreach (PropertyInfo property in properties)
                {
                    serializer.Serialize(writer, property.GetValue(element));
                }

                writer.WriteEndArray();
            }

            writer.WriteEndArray();

            writer.WriteEndObject();
        }
    }
}