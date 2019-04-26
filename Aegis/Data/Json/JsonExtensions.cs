using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Aegis.Data.Json
{
    public static class JsonExtensions
    {
        public static JProperty GetProperty(this JToken src, string path, bool exceptionWhenResultIsNull = true)
        {
            JToken currentToken = null;
            foreach (var key in path.Split(new char[] { '/', '\\' }))
            {
                if (currentToken == null)
                {
                    if (src is JValue || src is JArray)
                        break;
                    currentToken = src[key];
                }
                else
                {
                    if (currentToken is JValue || currentToken is JArray)
                        break;
                    currentToken = currentToken[key];
                }

                if (currentToken == null)
                    break;
            }


            if (exceptionWhenResultIsNull == true && currentToken == null)
                throw new AegisException(AegisResult.Json_KeyNotContain, string.Format("'{0}' key is not contains.", path));

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
    }
}
