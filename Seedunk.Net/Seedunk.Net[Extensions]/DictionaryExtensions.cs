using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Seedunk.Net
{
    /// <summary>
    /// 字典扩展
    /// </summary>
    public static class DictionaryExtensions
    {
        public static Dictionary<TKey, TValue> Merge<TKey, TValue>(this Dictionary<TKey, TValue> first, Dictionary<TKey, TValue> second)
        {

            foreach (var pair in second)
            {
                if (!first.ContainsKey(pair.Key))
                {
                    first.Add(pair.Key, pair.Value);
                }
            } 
            return first;
        }
        public static string ToHttpQuery<TKey, TValue>(this Dictionary<TKey, TValue> where)
        {
            string sql = "";
            if (where != null)
                foreach (var item in where)
                {
                    sql +=  item.Key + "=";
                    if (item.Value.GetType() == typeof(int))
                    {
                        sql += Convert.ToString(item.Value);
                    }
                    else if (item.Value.GetType() == typeof(string[]))
                    { 
                        sql += string.Join("%2C", item.Value as string[]); 
                    }
                    else
                    {
                        sql +=   Convert.ToString(item.Value) ;
                    }
                    sql += "&";
                }
            return sql.Length>0?("?"+sql.Substring(0, sql.Length - 1)) :"";
        }
        /// <summary>
        /// 转为SQL语句 And连接符
        /// </summary>
        /// <typeparam name="TKey"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="where"></param>
        /// <returns></returns>
        public static string ToAndSQL<TKey, TValue>(this Dictionary<TKey, TValue> where)
        { 
            string sql = " 1=1";
            if(where!=null)
                foreach (var item in where)
                {
                    sql += " and " + item.Key + "=";
                    if (item.Value.GetType() == typeof(int))
                    {
                        sql += Convert.ToString(item.Value);
                    }
                    else if (item.Value.GetType() == typeof(string[]))
                    {

                        sql += "in("; 
                        sql += string.Join(",", item.Value as string[]);
                        sql += ")";

                    }
                    else
                    {
                        sql += "'" + Convert.ToString(item.Value) + "'";
                    }
                }
            return sql;
        }
        public static string ToOrderSQL<TKey, TValue>(this Dictionary<TKey, TValue> orderBys)
        {
            string sql = "";
            if (orderBys != null) {
                sql = " order by ";
                foreach (var orderBy in orderBys)
                {
                    sql += orderBy.Key + " " + orderBy.Value;
                }
            }
           
            return sql;
        }

        public static string GetString(this Dictionary<string, object> dict,string name,string defaultValue)
        {
            if (dict.ContainsKey(name))
            {
                return Convert.ToString(dict[name]);
            }
            else {
                return defaultValue;
            }
             
        }

        public static string ToJSONString(this Dictionary<string, object> dict )
        {
            return JsonConvert.SerializeObject(dict, Formatting.Indented); 
        }
          
    }
}
