using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace Pelco.Helpers
{
    public static class XMLParser
    {
        public static T Parse<T>(string str) where T : class
        {
            T retObj = default(T);
            try
            {
                using (var reader = XmlReader.Create(ToStream(str.Trim()), new XmlReaderSettings() { ConformanceLevel = ConformanceLevel.Document }))
                {
                    retObj = new XmlSerializer(typeof(T)).Deserialize(reader) as T;
                }
            }
            catch (Exception)
            { }
            return retObj;
        }

        private static Stream ToStream(string str)
        {
            var stream = new MemoryStream();
            var writer = new StreamWriter(stream);
            writer.Write(str);
            writer.Flush();
            stream.Position = 0;
            return stream;
        }
    }
}
