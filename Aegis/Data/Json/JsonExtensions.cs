using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using Aegis.Calculate;

namespace Aegis.Data.Json
{
    public static class JsonExtensions
    {
        public static JProperty GetProperty(this JToken src, string path, bool exceptionWhenResultIsNull = true)
        {
            JToken currentToken = src;
            foreach (var key in path.Split(new char[] { '/', '\\' }))
            {
                if (key.Length == 0)
                    continue;


                if (currentToken is JValue)
                    break;
                if (currentToken is JArray)
                {
                    JArray array = currentToken as JArray;
                    int index = key.ToInt32(-1);

                    if (index == -1 || index >= array.Count)
                        throw new AegisException(AegisResult.Json_InvalidKey, "Invalid index of array({0}).", index);

                    currentToken = array[index];
                    continue;
                }


                currentToken = currentToken[key];
                if (currentToken == null)
                    break;
            }


            if (exceptionWhenResultIsNull == true && currentToken == null)
                throw new AegisException(AegisResult.Json_KeyNotContain, "'{0}' key is not contains.", path);

            return (currentToken?.Parent as JProperty);
        }


        public static bool IsNullOrEmpty(this JToken token)
        {
            return (token == null) ||
                   (token.Type == JTokenType.Array && !token.HasValues) ||
                   (token.Type == JTokenType.Object && !token.HasValues) ||
                   (token.Type == JTokenType.String && token.ToString() == String.Empty) ||
                   (token.Type == JTokenType.Null);
        }


        public static bool IsEqualWith(this JToken token, object target)
        {
            JToken tokenB;
            if (target is JToken)
                tokenB = target as JToken;
            else
                tokenB = new JValue(target);

            return JToken.DeepEquals(token, tokenB);
        }


        #region Value getters
        public static IJEnumerable<JToken> GetEnumerable(this JToken src, string path)
        {
            return src.GetProperty(path).Value.AsJEnumerable();
        }


        public static string GetStringValue(this JToken src, string path)
        {
            JProperty prop = src.GetProperty(path);
            return (string)prop.Value;
        }


        public static string GetStringValue(this JToken src, string path, string defaultValue)
        {
            try
            {
                JProperty prop = src.GetProperty(path);
                return (string)prop.Value;
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }


        public static int GetIntValue(this JToken src, string path)
        {
            JProperty prop = src.GetProperty(path);
            return (int)prop.Value;
        }


        public static int GetIntValue(this JToken src, string path, int defaultValue)
        {
            try
            {
                JProperty prop = src.GetProperty(path);
                return (int)prop.Value;
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }


        public static long GetLongValue(this JToken src, string path)
        {
            JProperty prop = src.GetProperty(path);
            return (long)prop.Value;
        }


        public static long GetLongValue(this JToken src, string path, long defaultValue)
        {
            try
            {
                JProperty prop = src.GetProperty(path);
                return (long)prop.Value;
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }


        public static double GetDoubleValue(this JToken src, string path)
        {
            JProperty prop = src.GetProperty(path);
            return (double)prop.Value;
        }


        public static double GetDoubleValue(this JToken src, string path, double defaultValue)
        {
            try
            {
                JProperty prop = src.GetProperty(path);
                return (double)prop.Value;
            }
            catch (ArgumentException)
            {
                return defaultValue;
            }
        }
        #endregion


        #region Value setters
        public static void SetValue(this JToken token, string path, string val)
        {
            token.GetProperty(path).Value = val;
        }


        public static void SetValue(this JToken token, string path, int val)
        {
            token.GetProperty(path).Value = val;
        }


        public static void SetValue(this JToken token, string path, long val)
        {
            token.GetProperty(path).Value = val;
        }


        public static void SetValue(this JToken token, string path, double val)
        {
            token.GetProperty(path).Value = val;
        }
        #endregion
    }
}
