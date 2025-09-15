using UnityEngine;

namespace Kokuu.Json
{
    public static class JsonUtility
    {
        public static string Encode(object value, JsonFormat format = JsonFormat.Indented)
        {
            return new JsonEncoder { format = format }.Encode(value);
        }

        public static object Decode(string json)
        {
            return new JsonDecoder().Decode(json);
        }

        internal static void Log(string message, LogType type = LogType.Warning)
        {
            Debug.LogFormat(type, LogOption.None, null, "[Json Utility] " + message);
        }
    }
}