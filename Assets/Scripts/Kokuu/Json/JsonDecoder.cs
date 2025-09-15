using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace Kokuu.Json
{
    using JsonObject = IDictionary;
    using JsonArray = IList;
    
    internal class JsonDecoder
    {
        private string str;
        private int index;
        private readonly List<string> path = new();

        public object Decode(string json)
        {
            this.str = json ?? throw new ArgumentNullException(nameof(json));
            index = 0;
            path.Clear();
            path.Add("root");
            object value = DecodeElement();
            if (index != json.Length) Log("Decoder Did Not Reach The End of String");
            return value;
        }
        
        private object DecodeValue()
        {
            if (index >= str.Length) throw ExceptionAtCurrent("Value");
            char ch = str[index];
            
            if (ch == '{') return DecodeObject();
            if (ch == '[') return DecodeArray();
            if (ch == '"') return DecodeString();
            if (ch is '-' or >= '0' and <= '9') return DecodeNumber();
            
            if (index + 4 <= str.Length && str[index..(index + 4)] == "true")
            {
                index += 4;
                return true;
            }
            if (index + 5 <= str.Length && str[index..(index + 5)] == "false")
            {
                index += 5;
                return false;
            }
            if (index + 4 <= str.Length && str[index..(index + 4)] == "null")
            {
                index += 4;
                return null;
            }
            
            throw ExceptionAtCurrent("Value");
        }

        private object DecodeObject()
        {
            DecodeChar('{');
            DecodeWhiteSpace();
            
            JsonObject obj = new Dictionary<string, object>();
            
            if (index >= str.Length) throw ExceptionAtCurrent("'}' or Member");
            if (str[index] == '}')
            {
                index++;
                return obj;
            }

            do
            {
                DecodeMember(obj);
                char ch = DecodeAny("'}' or ','");
                if (ch == '}') break;
                if (ch == ',') continue;
                throw ExceptionAtPrevious("Expect '}' or ','");
            } while (true);

            if (obj.Contains("content__"))
            {
                Type type = null;
                if (obj.Contains("type__") && obj["type__"] is string s) type = Type.GetType(s);
                if (type is null)
                {
                    Log($"Failed to Decode Serializable Element \"{string.Concat(path)}\", Unknown Type");
                    return null;
                }
                
                try
                {
                    return UnityEngine.JsonUtility.FromJson((string)obj["content__"], type);
                }
                catch
                {
                    Log($"Failed to Decode Serializable Element \"{string.Concat(path)}\", Failed to Deserialize");
                    return null;
                }
            }

            return obj;
        }

        private void DecodeMember(JsonObject target)
        {
            DecodeWhiteSpace();
            string key = DecodeString();
            DecodeWhiteSpace();
            DecodeChar(':');
            DecodeWhiteSpace();
            int begin = index;
            path.Add($".{key}");
            object value = DecodeValue();
            path.RemoveAt(path.Count - 1);
            int end = index;
            DecodeWhiteSpace();
            
            target.Add(key, key == "content__" ? str[begin..end] : value);
        }

        private JsonArray DecodeArray()
        {
            DecodeChar('[');
            DecodeWhiteSpace();
            
            JsonArray array = new List<object>();
            
            if (index >= str.Length) throw ExceptionAtCurrent("']' or Element");
            if (str[index] == ']')
            {
                index++;
                return array;
            }

            int i = 0;
            do
            {
                path.Add($"[{i++}]");
                array.Add(DecodeElement());
                path.RemoveAt(path.Count - 1);
                char ch = DecodeAny("']' or ','");
                if (ch == ']') break;
                if (ch == ',') continue;
                throw ExceptionAtPrevious("Expect ']' or ','");
            } while (true);

            return array;
        }

        private object DecodeElement()
        {
            DecodeWhiteSpace();
            object value = DecodeValue();
            DecodeWhiteSpace();
            return value;
        }

        private string DecodeString()
        {
            DecodeChar('"');
            StringBuilder builder = new StringBuilder();

            do
            {
                char ch = DecodeAny("Character");
                if (ch == '"') break;
                if (ch == '\\')
                {
                    char esc = DecodeAny("Escape");
                    switch (esc)
                    {
                        case '"': builder.Append('"'); break;
                        case '\\': builder.Append('\\'); break;
                        case '/': builder.Append('/'); break;
                        case 'b': builder.Append('\b'); break;
                        case 'f': builder.Append('\f'); break;
                        case 'n': builder.Append('\n'); break;
                        case 'r': builder.Append('\r'); break;
                        case 't': builder.Append('\t'); break;
                        case 'u':
                        {
                            byte[] bytes = {
                                (byte)(16 * DecodeHexAt(index + 2) + DecodeHexAt(index + 3)),
                                (byte)(16 * DecodeHexAt(index + 0) + DecodeHexAt(index + 1))
                            };
                            index += 4;
                            
                            int DecodeHexAt(int i)
                            {
                                if (i >= str.Length) throw ExceptionAt("Hex", i);
                                char hex = str[i];
                                return hex switch
                                {
                                    >= '0' and <= '9' => hex - '0',
                                    >= 'a' and <= 'f' => hex - 'a' + 10,
                                    >= 'A' and <= 'F' => hex - 'A' + 10,
                                    _ => throw ExceptionAt("Hex", i)
                                };
                            }
                            
                            builder.Append(Encoding.Unicode.GetString(bytes));
                            break;
                        }
                        default: throw ExceptionAtPrevious("Escape");
                    }
                    continue;
                }
                if (ch < ' ') throw ExceptionAtPrevious("Character");
                builder.Append(ch);
            } while (true);
            
            return builder.ToString();
        }

        private object DecodeNumber()
        {
            bool isNegative = false;
            bool isFraction = false;
            int integer = 0;
            float fraction = 0;
            int exponent = 0;
            int temp;

            char ch = DecodeAny("'-' or Digit");
            if (ch == '-')
            {
                isNegative = true;
                ch = DecodeAny("Digit");
            }

            if (ch is '0') { }
            else if (ch is >= '1' and <= '9')
            {
                integer = ch - '0';
                fraction = ch - '0';

                while (TryDecodeDigit(out temp))
                {
                    integer = integer * 10 + temp;
                    fraction = fraction * 10 + temp;
                }
            }
            else throw ExceptionAtPrevious("Digit");

            if (index < str.Length && str[index] is '.')
            {
                index++;
                isFraction = true;
                
                fraction = fraction * 10 + DecodeDigit();
                exponent--;

                while (TryDecodeDigit(out temp))
                {
                    fraction = fraction * 10 + temp;
                    exponent--;
                }
            }

            if (index < str.Length && str[index] is 'e' or 'E')
            {
                index++;
                isFraction = true;

                bool isExpNegative = false;
                ch = DecodeAny("'-', '+' or Digit");
                if (ch is '-') isExpNegative = true;
                else if (ch is '+') { }
                else if (ch is >= '0' and <= '9') index--;
                else throw ExceptionAtPrevious("'-', '+' or Digit");
                
                int exp = DecodeDigit();
                while (TryDecodeDigit(out int digit)) exp = exp * 10 + digit;
                
                if (isExpNegative) exp = -exp;
                exponent += exp;
            }

            if (isNegative)
            {
                integer = -integer;
                fraction = -fraction;
            }
            
            if (isFraction)
            {
                if (exponent >= 0)
                    for (int i = 0; i < exponent; i++)
                        fraction *= 10;
                else
                    for (int i = 0; i > exponent; i--)
                        fraction /= 10;
                return fraction;
            }
            return integer;

            int DecodeDigit()
            {
                char c = DecodeAny("Digit");
                if (c is >= '0' and <= '9') return c - '0';
                throw ExceptionAtPrevious("Digit");
            }
            
            bool TryDecodeDigit(out int d)
            {
                d = 0;
                if (index >= str.Length) return false;
                char c = str[index];
                if (c is not (>= '0' and <= '9')) return false;
                index++;
                d = c - '0';
                return true;
            }
        }
        
        private void DecodeWhiteSpace()
        {
            while (index < str.Length && str[index] is ' ' or '\t' or '\r' or '\n') index++;
        }
        
        private char DecodeAny(string expectation)
        {
            if (index >= str.Length) throw ExceptionAtCurrent(expectation);
            return str[index++];
        }
        
        private void DecodeChar(char expectation)
        {
            if (DecodeAny($"'{expectation}'") != expectation)
                throw ExceptionAtPrevious($"'{expectation}'");
        }

        private JsonGrammarException ExceptionAtCurrent(string expectation) => ExceptionAt(expectation, index);
        private JsonGrammarException ExceptionAtPrevious(string expectation) => ExceptionAt(expectation, index - 1);
        private JsonGrammarException ExceptionAt(string expectation, int i) => new(expectation, str, i, path);
        
        private static void Log(string message, LogType type = LogType.Warning) => JsonUtility.Log(message, type);
    }
}