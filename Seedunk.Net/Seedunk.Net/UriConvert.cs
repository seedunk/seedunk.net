using System;
using System.Collections.Generic; 

namespace Seedunk.Net
{
    public class UriConvert
    {

        public static string DictionaryToQueryString(Dictionary<string, string> values)
        {
            string url = "";
            foreach (var val in values)
            {
                string value = val.Value;
                value = value.Replace("+", "%2B");
                value = value.Replace(" ", "%20");
                value = value.Replace("/", "%2F");
                value = value.Replace("?", "%3F");
                value = value.Replace("%", "%25");
                value = value.Replace("#", "%23");
                value = value.Replace("&", "%26");
                value = value.Replace("=", "%3D");
                url += "&" + val.Key + "=" + value; 
            }
            if (url.Substring(0, 1).Equals("&"))
            {
                return url.Substring(1);
            }
            else
            {
                return url;
            }
        }


        public static Dictionary<string, string> QueryStringToDictionary(string queryString)
        {
            string[] args = queryString.Split('&') ;
            Dictionary<string, string> kv = new Dictionary<string, string>();
            for(int i=0;i<args.Length;i++)
            {
                string[] kvArgs = args[i].Split('=');
                if (kvArgs.Length == 2) {
                    string value = kvArgs[1];
                        value = value.Replace( "%2B", "+");
                        value = value.Replace("%20", " ");
                        value = value.Replace(  "%2F", "/");
                        value = value.Replace( "%3F", "?");
                        value = value.Replace( "%25", "%");
                        value = value.Replace( "%23", "#");
                        value = value.Replace( "%26", "&");
                        value = value.Replace("%3D", "="); 
                    kv.Add(kvArgs[0], value);
                }  
            }
            return kv;
             
        }

        
    }
}
