using System;
using System.Collections.Generic;
using UnityEngine;

namespace Kokuu.Json
{
    public class JsonGrammarException : Exception
    {
        public JsonGrammarException(string expectation, string json, int index, List<string> path)
            : base(GenerateMessage(expectation, json, index, path)) { }

        private static string GenerateMessage(string expectation, string json, int index, List<string> path)
        {
            string context = String.Empty;
            if (index > 0) context += json[Math.Max(index - 10, 0)..index];
            if (index < json.Length) context += $"<color=red>{json[index]}</color>";
            if (index < json.Length - 1) context += json[(index + 1)..Mathf.Min(index + 11, json.Length)];

            return $"Failed to Decode Element \"{string.Concat(path)}\" at Index {index} (<color=grey>{context}</color>), Expect {expectation}";
        }
    }
}