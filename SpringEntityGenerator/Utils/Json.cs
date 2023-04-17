using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace SpringEntityGenerator.Utils
{
    public class Json
    {

        private static readonly JsonSerializerSettings SerializerSettings=new JsonSerializerSettings()
        {
            ContractResolver = new CamelCasePropertyNamesContractResolver()
        };


        public static T? Deserialize<T>(string json)
        {
            return JsonConvert.DeserializeObject<T>(json, SerializerSettings);
        }

        public static string Serialize(object value)
        {
            return JsonConvert.SerializeObject(value, SerializerSettings);
        }

    }
}
