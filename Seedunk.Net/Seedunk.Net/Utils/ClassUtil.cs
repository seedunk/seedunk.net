
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.IO.Pipes;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;


namespace Seedunk.Net.Utils
{



    public delegate string DataRowToObjectPropertyNameParser(string propertyName);

    public delegate bool DataRowToObjectValueParser(PropertyInfo column, DataRow row, out object obj);
    public delegate bool ObjectToKVValueParser(PropertyInfo column, object obj, out object value);
    public delegate bool KVToObjectValueParser(PropertyInfo column, object obj, out object value);




    #region 过时->ModelUtil 
    public class ClassLoader<T>
    {

        public static void SetPropertyValue(object obj, string propertyName, object value)
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop != null && prop.CanWrite)
            {
                prop.SetValue(obj, value, null);
            }
        }
        public static T2 GetPropertyValue<T2>(object obj, string propertyName,T2 def ) where T2:class 
        {
            var prop = typeof(T).GetProperty(propertyName);
            if (prop != null && prop.CanRead)
            {   
                if (typeof(T2) == typeof(Int32))
                {
                    return  int.Parse(Convert.ToString(prop.GetValue(obj))) as T2 ;   
                }
                else if (typeof(T2) == typeof(string))
                {
                    return  Convert.ToString(prop.GetValue(obj))  as T2;
                }
                else {
                    return (T2)prop.GetValue(obj);
                }
                
            }
            else return def;
        }
        public static List<T> ListFromList<T2>(List<T2> inList) where T2 : T
        {
            List<T> ret = new List<T>();
            for (int i = 0; i < inList.Count; i++)
            {
                ret.Add(inList[i]);
            }
            return ret;

        }

        public static string ObjectToKVString(T instance, ObjectToKVValueParser parser)
        {

            Dictionary<string, object> kv = ObjectToKV(instance, parser);
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in kv)
            {
                string format = "\"{0}\":\"{1}\",";
                if (kvp.GetType() == typeof(int) || kvp.GetType() == typeof(double))
                {
                    format = "\"{0}\":{1},";
                }
                sb.AppendFormat(format, kvp.Key, kvp.Value);
            }
            return "{" + sb.ToString().TrimEnd(',', ' ') + "}";
        }

        public static Dictionary<string, object> ObjectToKV(T instance, ObjectToKVValueParser parser)
        {
            return ClassUtil.ObjectToKV(instance, parser);
        }
        public static T KVStringToObject(string kvString, KVToObjectValueParser valueParser)
        {

            Dictionary<string, object> kvs = new Dictionary<string, object>();
            if (kvString.Substring(0, 1).Equals("{") &&
                kvString.Substring(kvString.Length - 1).Equals("}"))
            {
                string arg0 = kvString.Substring(1, kvString.Length - 1);
                foreach (var arg in arg0.Split(','))
                {
                    string[] kv = arg.Split(':');
                    if (kv.Length == 2)
                    {
                        string key = kv[0].Substring(1, kv[0].Length - 2);
                        string val = kv[1];
                        if (val.Substring(0, 1).Equals("\""))
                        {
                            kvs.Add(key, val.Substring(1, val.Length - 2));
                        }
                        else
                        {
                            kvs.Add(key, int.Parse(val));
                        }
                    }
                }
            }
            return KVToObject<T>(kvs, valueParser);
        }


        public static T KVToObject<T>(Dictionary<string, object> kv, KVToObjectValueParser valueParser)
        {

            T obj = Activator.CreateInstance<T>();
            ClassUtil.ObjectBindKV(obj, kv, valueParser);
            return obj;
        }



        public static T DataRowToObject(DataRow row)
        {

            return DataRowToObject(row, (n) => { return n; }, (PropertyInfo b, DataRow a, out object c) =>
            {
                c = null;
                return false;
            });
        }




        //过期->BindDataRow  
        public static T DataRowToObject(DataRow row, DataRowToObjectPropertyNameParser parser, DataRowToObjectValueParser valueParser)
        {
            Dictionary<string, object> kv = new Dictionary<string, object>();
            T obj = Activator.CreateInstance<T>();
            foreach (var property in typeof(T).GetProperties())
            {
                string columnName = parser(property.Name);
                if (row.Table.Columns.Contains(columnName) && property.CanWrite)
                {

                    if (valueParser(property, row, out object outObj))
                    {
                        property.SetValue(obj, outObj);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        int val = int.TryParse(Convert.ToString(row[columnName]), out int outVal) ? outVal : 0;
                        property.SetValue(obj, val);
                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        double val = double.TryParse(Convert.ToString(row[columnName]), out var outVal) ? outVal : 0;
                        property.SetValue(obj, val);
                    }
                    else if (property.PropertyType == typeof(uint))
                    {
                        uint val = uint.TryParse(Convert.ToString(row[columnName]), out var outUint) ? outUint : 0;
                        property.SetValue(obj, val);
                    }
                    else if (property.PropertyType == typeof(short))
                    {
                        short val = short.TryParse(Convert.ToString(row[columnName]), out var outVal) ? outVal : (short)0;
                        property.SetValue(obj, val);
                    }
                    else if (property.PropertyType == typeof(ushort))
                    {
                        ushort val = ushort.TryParse(Convert.ToString(row[columnName]), out ushort outVal) ? outVal : (ushort)0;
                        property.SetValue(obj, val);
                    }
                    else if (property.PropertyType == typeof(string[]))
                    {
                        property.SetValue(obj, Convert.ToString(row[columnName]).Split(','));
                    }
                    else if (property.PropertyType == typeof(string) || property.PropertyType == typeof(string))
                    {
                        property.SetValue(obj, Convert.ToString(row[columnName]));
                    }
                }


            }
            return obj;
        }

        public static bool TryLoadFromDLL(string dllPath, string className, out T instance)
        {
            instance = default;
            if (ClassUtil.TryCreateInstanceFromDLL(dllPath, className, out object obj))
            {
                if (obj != null)
                {
                    try
                    {
                        instance = (T)obj;
                        return true;
                    }
                    catch
                    {

                    }
                }
            }
            return false;
        }
    }
    public class ClassUtil
    {

        public static void ObjectBindKVString(object obj, string kvString, KVToObjectValueParser valueParser)
        {

            Dictionary<string, object> kvs = new Dictionary<string, object>();
            if (kvString.Substring(0, 1).Equals("{") &&
                kvString.Substring(kvString.Length - 1).Equals("}"))
            {
                string arg0 = kvString.Substring(1, kvString.Length - 1);
                foreach (var arg in arg0.Split(','))
                {
                    string[] kv = arg.Split(':');
                    if (kv.Length == 2)
                    {
                        string key = kv[0].Substring(1, kv[0].Length - 2);
                        string val = kv[1];
                        if (val.Substring(0, 1).Equals("\""))
                        {

                            val = val.Substring(1, val.Length - 2);
                            if (val.Length > 1 && val.Substring(val.Length - 1, 1).Equals("\""))
                            {

                                val = val.Substring(0, val.Length - 1);
                            }
                            kvs.Add(key, val);

                        }
                        else
                        {
                            kvs.Add(key, int.Parse(val));
                        }
                    }
                }
            }
            ObjectBindKV(obj, kvs, valueParser);
        }
        public static void ObjectBindDataRow(object obj, DataRow row, DataRowToObjectValueParser valueParser)
        {
            ObjectBindDataRow(obj, row, valueParser, (s) => { return s; });
        }
        public static void ObjectBindDataRow(object obj, DataRow row, DataRowToObjectValueParser valueParser, DataRowToObjectPropertyNameParser propertyNameParser)
        {

            foreach (var property in obj.GetType().GetProperties())
            {
                string columnName = propertyNameParser(property.Name);

                if (property.CanWrite)
                {

                    if (valueParser(property, row, out object outObj))
                    {
                        if (outObj != null)
                            property.SetValue(obj, outObj);
                    }
                    else if (row.Table.Columns.Contains(columnName))
                    { //标准处理 只能为表内数据

                        if (property.PropertyType == typeof(int))
                        {
                            int val = int.TryParse(Convert.ToString(row[columnName]), out int outVal) ? outVal : 0;
                            property.SetValue(obj, val);
                        }
                        if (property.PropertyType == typeof(double))
                        {
                            double val = double.TryParse(Convert.ToString(row[columnName]), out var outVal) ? outVal : 0;
                            property.SetValue(obj, val);
                        }
                        else if (property.PropertyType == typeof(string[]))
                        {
                            property.SetValue(obj, Convert.ToString(row[columnName]).Split(','));
                        }
                        else
                        {
                            property.SetValue(obj, Convert.ToString(row[columnName]));
                        }

                    }

                }


            }
        }
        public static void ObjectBindKV(object obj, Dictionary<string, object> kv, KVToObjectValueParser valueParser)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                if (kv.ContainsKey(property.Name) && !property.Name.Substring(0, 1).Equals("_"))
                {
                    if (valueParser(property, kv[property.Name], out object outObj))
                    {
                        property.SetValue(obj, outObj);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(obj, int.TryParse(Convert.ToString(kv[property.Name]), out var outVar) ? outVar : 0);

                    }
                    else if (property.PropertyType == typeof(double))
                    {
                        property.SetValue(obj, double.TryParse(Convert.ToString(kv[property.Name]), out var outVar) ? outVar : 0);

                    }
                    else if (property.PropertyType == typeof(ushort))
                    {
                        property.SetValue(obj, ushort.TryParse(Convert.ToString(kv[property.Name]), out var outVar) ? outVar : (ushort)0);

                    }
                    else if (property.PropertyType == typeof(string[]))
                    {
                        property.SetValue(obj, Convert.ToString(kv[property.Name]).Split(','));
                    }
                    else
                    {
                        property.SetValue(obj, Convert.ToString(kv[property.Name]));
                    }
                }
            }
        }



        public static void ObjectPropertyBindValue(object obj, string propertyName, object propertyValue)
        {
            foreach (var property in obj.GetType().GetProperties())
            {
                if (property.Name.Equals(propertyName))
                {
                    try
                    {
                        property.SetValue(obj, propertyValue);
                    }
                    catch
                    {
                        property.SetValue(obj, null);
                    }
                }
            }
        }


        public static bool TryGetTypeFromDLL(string dllPath, string className, out Type type)
        {
            return TryGetTypeFromDLL(dllPath, className, out type, out string msg);
        }
        public static string ObjectToKVString(object instance, ObjectToKVValueParser parser)
        {
            return KVToKVString(ObjectToKV(instance, parser));
        }
        public static string KVToKVString(Dictionary<string, object> kv)
        {
            StringBuilder sb = new StringBuilder();
            foreach (var kvp in kv)
            {
                string format = "\"{0}\":\"{1}\",";
                if (kvp.GetType() == typeof(int) || kvp.GetType() == typeof(double))
                {
                    format = "\"{0}\":{1},";
                }
                sb.AppendFormat(format, kvp.Key, kvp.Value);
            }
            return "{" + sb.ToString().TrimEnd(',', ' ') + "}";

        }

        public static long ObjectToKVExcludeObject { get { return 0x100100452; } }
        public static Dictionary<string, object> ObjectToKV(object instance, ObjectToKVValueParser parser)
        {
            Dictionary<string, object> kv = new Dictionary<string, object>();

            foreach (var property in instance.GetType().GetProperties())
            {

                if (parser != null && parser(property, property.GetValue(instance, null), out object value))
                {
                    if (value.GetType() == typeof(long) && (long)value == ObjectToKVExcludeObject)
                    {
                    }
                    else
                    {
                        kv.Add(property.Name, value);
                    }
                }
                else if (property.PropertyType == typeof(ushort) || property.PropertyType == typeof(short) || property.PropertyType == typeof(double) ||
                    property.PropertyType == typeof(uint) || property.PropertyType == typeof(int))
                {
                    kv.Add(property.Name, Convert.ToString(property.GetValue(instance, null)));
                }
                else
                {
                    kv.Add(property.Name, property.GetValue(instance, null));

                }
            }
            return kv;
        }

        public static bool TryGetTypeFromDLL(string dllPath, string className, out Type type, out string errMessage)
        {
            type = null;
            errMessage = "";
            if (!File.Exists(dllPath)) return false;
            try
            {
                Assembly assembly = Assembly.LoadFrom(dllPath);
                type = assembly.GetType(className);
            }
            catch (Exception ex)
            {
                errMessage = ex.Message;

            }
            return type != null;
        }

        public static bool TryCreateInstanceFromDLL(string dllPath, string className, out object instance)
        {
            return TryCreateInstanceFromDLL(dllPath, className, out instance, out string errMessagea);
        }
        public static bool TryCreateInstanceFromDLL(string dllPath, string className, out object instance, out string errMessage)
        {
            instance = null;
            errMessage = "";
            if (TryGetTypeFromDLL(dllPath, className, out Type type, out errMessage))
            {
                instance = Activator.CreateInstance(type);
                return instance != null;
            }
            else
            {
                return false;
            }
        }
    }
    #endregion
}
