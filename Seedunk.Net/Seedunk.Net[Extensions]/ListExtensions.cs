using Newtonsoft.Json;
using Seedunk.Net.IModels;
using Seedunk.Net.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace Seedunk.Net
{
    /// <summary>
    /// 字典扩展
    /// </summary>
    public static class ListExtensions
    {
        public static List<TARGET> ToModels<T,TARGET>(this List<T> data) where TARGET:class where T : class, IModel  
        {
            List < TARGET > targets = new List < TARGET > ();
            for (int i = 0; i < data.Count; i++) {
                targets.Add(data[i].ToModel<TARGET>());
            } 
            return targets;
        }

        public static Dictionary<string,object> ToKeyValue<T>( List<T> data,string keyColumn,string valueColumn)
        {
            Dictionary<string, object> items = new Dictionary<string, object>();
            for (int i = 0; i < data.Count; i++)
            {
                Dictionary<string, object> item = ModelUtil<T>.ToDictionary(data[i]);
                items.Add(item.TryGetValue(keyColumn, out var key) ? key.ToString() : "", item.TryGetValue(valueColumn, out var val) ? val.ToString() : "");

            }
            return items;
        }

        public static string ToJSONString<T>(this List<T> data)
        {

        
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
        
    }
}
