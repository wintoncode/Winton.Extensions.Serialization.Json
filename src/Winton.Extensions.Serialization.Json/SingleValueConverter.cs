// Copyright (c) Winton. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json;

namespace Winton.Extensions.Serialization.Json
{
    /// <inheritdoc />
    /// <summary>
    ///     A <see cref="JsonConverter" /> for serializing types with a single backing field as the value of that field, and
    ///     deserializing back to the original type.
    /// </summary>
    public class SingleValueConverter : JsonConverter
    {
        /// <inheritdoc />
        public override bool CanConvert(Type objectType)
        {
            return true;
        }

        /// <inheritdoc />
        public override object? ReadJson(
            JsonReader reader,
            Type objectType,
            object existingValue,
            JsonSerializer serializer)
        {
            object value = reader.Value;
            if (value == null)
            {
                return null;
            }

            TypeInfo typeInfo = (Nullable.GetUnderlyingType(objectType) ?? objectType).GetTypeInfo();

            Type fieldType = GetSingleFieldInfo(typeInfo).FieldType;

            ConstructorInfo constructorInfo =
                typeInfo.DeclaredConstructors.SingleOrDefault(ci => TakesSingleParameterOfType(ci, fieldType));

            if (constructorInfo == null)
            {
                throw new JsonSerializationException(
                    $"{nameof(SingleValueConverter)} can only be used on types with a constructor taking a single parameter of the same type as its backing field.");
            }

            return constructorInfo.Invoke(
                new[]
                {
                    Convert.ChangeType(value, fieldType)
                });
        }

        /// <inheritdoc />
        public override void WriteJson(
            JsonWriter writer,
            object value,
            JsonSerializer serializer)
        {
            writer.WriteValue(GetSingleFieldInfo(value.GetType().GetTypeInfo()).GetValue(value));
        }

        private static FieldInfo GetSingleFieldInfo(TypeInfo typeInfo)
        {
            FieldInfo[] fieldInfos = typeInfo.DeclaredFields.Where(fi => !fi.IsStatic && !fi.IsPublic).ToArray();

            if (fieldInfos.Length != 1)
            {
                throw new JsonSerializationException(
                    $"{nameof(SingleValueConverter)} can only be used on types with a single backing field.");
            }

            return fieldInfos.Single();
        }

        private static bool TakesSingleParameterOfType(MethodBase methodBase, Type type)
        {
            ParameterInfo[] parameterInfos = methodBase.GetParameters();

            return parameterInfos.Length == 1 && parameterInfos.Single().ParameterType == type;
        }
    }
}