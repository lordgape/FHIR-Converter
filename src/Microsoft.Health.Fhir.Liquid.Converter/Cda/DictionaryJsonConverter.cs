﻿// -------------------------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License (MIT). See LICENSE in the repo root for license information.
// -------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace Microsoft.Health.Fhir.Liquid.Converter.Cda
{
    /// <summary>
    /// One-way JsonConverter to deserialize JSON string to IDictionary
    /// </summary>
    public class DictionaryJsonConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            return ReadValue(reader);
        }

        private object ReadValue(JsonReader reader)
        {
            while (reader.TokenType == JsonToken.Comment)
            {
                if (!reader.Read())
                {
                    throw new JsonSerializationException("Unexpected Token when converting IDictionary<string, object>");
                }
            }

            switch (reader.TokenType)
            {
                case JsonToken.StartObject:
                    return ReadObject(reader);
                case JsonToken.StartArray:
                    return ReadArray(reader);
                case JsonToken.Integer:
                case JsonToken.Float:
                case JsonToken.String:
                case JsonToken.Boolean:
                case JsonToken.Undefined:
                case JsonToken.Null:
                case JsonToken.Date:
                case JsonToken.Bytes:
                    return reader.Value;
                default:
                    throw new JsonSerializationException($"Unexpected token when converting IDictionary<string, object>: {reader.TokenType}");
            }
        }

        private object ReadArray(JsonReader reader)
        {
            IList<object> list = new List<object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndArray:
                        return list;
                    default:
                        var v = ReadValue(reader);
                        list.Add(v);
                        break;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
        }

        private object ReadObject(JsonReader reader)
        {
            var obj = new Dictionary<string, object>();

            while (reader.Read())
            {
                switch (reader.TokenType)
                {
                    case JsonToken.PropertyName:
                        var propertyName = reader.Value.ToString();

                        if (!reader.Read())
                        {
                            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
                        }

                        // TODO: Should we do this when it is xml?
                        if (propertyName.StartsWith("@"))
                        {
                            // If property is attribute, remove "@"
                            propertyName = propertyName[1..];
                        }

                        var v = ReadValue(reader);
                        obj[propertyName] = v;
                        break;
                    case JsonToken.Comment:
                        break;
                    case JsonToken.EndObject:
                        return obj;
                }
            }

            throw new JsonSerializationException("Unexpected end when reading IDictionary<string, object>");
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(IDictionary<string, object>).IsAssignableFrom(objectType);
        }
    }
}
