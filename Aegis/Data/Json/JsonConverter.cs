using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace Aegis.Data.Json
{
    public class JsonConverter
    {
        public static JObject Parse(string json)
        {
            return JObject.Parse(json);
        }


        public static JObject Parse(string json, JsonLoadSettings settings)
        {
            return JObject.Parse(json, settings);
        }
    }
}
