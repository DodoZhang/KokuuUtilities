using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kokuu.Json
{
    using JsonObject = IDictionary;
    using JsonArray = IList;
    
    internal class JsonEncoder
    {
        public JsonFormat format;
        
        private readonly StringBuilder builder = new();
        private readonly List<string> path = new();

        public string Encode(object value)
        {
            builder.Clear();
            path.Clear();
            path.Add("root");
            EncodeValue(value);
            return builder.ToString();
        }

        private void EncodeValue(object value)
        {
            if (value is null) builder.Append("null");
            else if (value is JsonObject obj) EncodeObject(obj);
            else if (value is JsonArray array) EncodeArray(array);
            else if (value is string str) EncodeString(str);
            else if (value is short s) builder.Append(s);
            else if (value is int i) builder.Append(i);
            else if (value is long l) builder.Append(l);
            else if (value is ushort us) builder.Append(us);
            else if (value is uint ui) builder.Append(ui);
            else if (value is ulong ul) builder.Append(ul);
            else if (value is float f) builder.Append($"{f:0.0###########}");
            else if (value is double d) builder.Append($"{d:0.0###########}");
            else if (value is bool b) builder.Append(b ? "true" : "false");
            else EncodeSerializable(value);
        }

        private void EncodeObject(JsonObject obj)
        {
            if (obj.Count == 0)
            {
                builder.Append("{}");
                return;
            }
            
            if (format == JsonFormat.Indented)
            {
                string indent = new(' ', path.Count * 4);
                builder.Append("{\n");
                foreach (object rawKey in obj.Keys)
                {
                    if (rawKey is not string key)
                    {
                        Log($"Failed to Encode Member {rawKey} in Dictionary \"{string.Concat(path)}\", Which It's Key is Not a String");
                        continue;
                    }

                    builder.Append(indent);
                    EncodeString(key);
                    builder.Append(": ");
                    path.Add($".{key}");
                    EncodeValue(obj[key]);
                    path.RemoveAt(path.Count - 1);
                    builder.Append(",\n");
                }
                builder.Remove(builder.Length - 2, 2);
                builder.Append("\n").Append(' ', (path.Count - 1) * 4).Append("}");
            }
            else
            {
                builder.Append("{");
                foreach (object rawKey in obj.Keys)
                {
                    if (rawKey is not string key)
                    {
                        Log($"Failed to Encode Member {rawKey} in Dictionary \"{string.Concat(path)}\", Which It's Key is Not a String");
                        continue;
                    }
                    EncodeString(key);
                    builder.Append(":");
                    path.Add($".{key}");
                    EncodeValue(obj[key]);
                    path.RemoveAt(path.Count - 1);
                    builder.Append(",");
                }
                builder.Remove(builder.Length - 1, 1).Append("}");
            }
        }
        
        private void EncodeArray(JsonArray array)
        {
            if (array.Count == 0)
            {
                builder.Append("[]");
                return;
            }
            
            if (format == JsonFormat.Indented)
            {
                string indent = new(' ', path.Count * 4);
                builder.Append("[\n");
                for (int i = 0; i < array.Count; i++)
                {
                    builder.Append(indent);
                    path.Add($"[{i}]");
                    EncodeValue(array[i]);
                    path.RemoveAt(path.Count - 1);
                    if (i != array.Count - 1) builder.Append(",");
                    builder.Append("\n");
                }
                builder.Append(' ', (path.Count - 1) * 4).Append("]");
            }
            else
            {
                builder.Append("[");
                for (int i = 0; i < array.Count; i++)
                {
                    path.Add($"[{i}]");
                    EncodeValue(array[i]);
                    path.RemoveAt(path.Count - 1);
                    if (i != array.Count - 1) builder.Append(",");
                }
                builder.Append("]");
            }
        }

        private void EncodeString(string str)
        {
            builder.Append('\"');
            foreach (char c in str)
            {
                switch (c)
                {
                    case '\"': builder.Append(@"\"""); break;
                    case '\\': builder.Append(@"\\"); break;
                    case '\b': builder.Append(@"\b"); break;
                    case '\f': builder.Append(@"\f"); break;
                    case '\n': builder.Append(@"\n"); break;
                    case '\r': builder.Append(@"\r"); break;
                    case '\t': builder.Append(@"\t"); break;
                    case <' ': builder.Append($@"\u{(int)c:X4}"); break;
                    default: builder.Append(c); break;
                }
            }
            builder.Append('\"');
        }

        private  void EncodeSerializable(object value)
        {
            string json;

            try
            {
                json = UnityEngine.JsonUtility.ToJson(value, format == JsonFormat.Indented);
            }
            catch (ArgumentException)
            {
                Log($"Failed to Encode Value {value}({value.GetType().Name}) at \"{string.Concat(path)}\"");
                builder.Append("null");
                return;
            }
            
            if (format == JsonFormat.Indented)
            {
                string indent = new(' ', path.Count * 4);
                json = json.Replace("\n", "\n" + indent);
                builder.Append("{\n");
                builder.Append(indent).Append($"\"type__\": \"{value.GetType().AssemblyQualifiedName}\",\n");
                builder.Append(indent).Append($"\"content__\": {json}\n");
                builder.Append(' ', (path.Count - 1) * 4).Append("}");
            }
            else
            {
                builder.Append("{");
                builder.Append($"\"type__\":\"{value.GetType().AssemblyQualifiedName}\",");
                builder.Append($"\"content__\":{json}");
                builder.Append("}");
            }
        }
        
        private static void Log(string message, LogType type = LogType.Warning) => JsonUtility.Log(message, type);
    }
}