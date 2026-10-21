using Newtonsoft.Json;
using System;
using System.Collections.ObjectModel; 

namespace Seedunk.Net 
{
    public static class ObservableCollectionExtensions
    {
        public static string ToJSONString<T>(this ObservableCollection<T> data)
        {
            return JsonConvert.SerializeObject(data, Formatting.Indented);
        }
        public static ObservableCollection<T> ToObservableCollection<T>(this string data)
        {
            try
            {
                return JsonConvert.DeserializeObject<ObservableCollection<T>>(data); 
            }
            catch   {
                return new ObservableCollection<T>();
            }
        }
    }
}
