using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Web.Script.Serialization;

namespace Pelco.Helpers
{
    public static class Serialization
    {
        public static string Serialize<T>(T objectToSerialize) where T : class
        {
            string base64String = string.Empty;
            using (var ms = new MemoryStream())
            {
                new BinaryFormatter().Serialize(ms, objectToSerialize);
                base64String = Convert.ToBase64String(ms.ToArray());
            }
            return base64String;
        }

        public static T Deserialize<T>(string base64String) where T : class
        {
            T retObj = default(T);
            using (var ms = new MemoryStream(Convert.FromBase64String(base64String)))
            {
                var formatter = new BinaryFormatter();
                retObj = (T)formatter.Deserialize(ms);
            }
            return retObj;
        }

        public static string SerializeJson<T>(T objectToSerialize) where T : class
        {
            string json = string.Empty;
            json = new JavaScriptSerializer().Serialize(objectToSerialize);
            return json;
        }

        public static T DeserializeJson<T>(string json) where T : class
        {
            T retObj = default(T);
            retObj = new JavaScriptSerializer().Deserialize<T>(json);
            return retObj;
        }
    }
}
