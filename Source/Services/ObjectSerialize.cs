using System;
using System.Text;
using Newtonsoft.Json;

namespace KnowledgeGraph.Services
{
    public static class ObjectSerialize
    {
        public static byte[] Serialize(this object obj)
        {
            if (obj == null)
            {
                return null;
            }

            var json = JsonConvert.SerializeObject(obj);
            return Encoding.ASCII.GetBytes(json);
        }

        public static object DeSerialize(this byte[] arrBytes, Type type)
        {
            try
            {
                Console.WriteLine(type);
                Console.WriteLine(arrBytes);
                var json = Encoding.Default.GetString(arrBytes);
                Console.WriteLine(json);
                return JsonConvert.DeserializeObject(json, type);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
                Console.WriteLine(e.StackTrace);
                throw e;
            }
        }

        public static string DeSerializeText(this byte[] arrBytes)
        {
            return Encoding.Default.GetString(arrBytes);
        }
    }
}