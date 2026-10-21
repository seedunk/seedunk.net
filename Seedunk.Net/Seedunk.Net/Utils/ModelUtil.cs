
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Seedunk.Net.Utils
{
    public delegate bool PropertyValueParser<T>(PropertyInfo info, T inFullData, object inData, out object value);


    public delegate string PropertyNameParser(PropertyInfo info);

    public class ModelUtil<T>
    {



        public static void BindDictionary(T obj, Dictionary<string, object> kv)
        {
            BindDictionary(obj, kv, null);
        }


        public static void BindDictionary(T obj, Dictionary<string, object> kv, PropertyValueParser<Dictionary<string, object>> valueParser)
        {
            //var obj = Activator.CreateInstance(typeof(T));
            foreach (var property in typeof(T).GetProperties())
            {
                string nameWithUnderline = Regex.Replace(property.Name, "([A-Z])", "_$1").ToLower().TrimStart('_');
                string name = kv.ContainsKey(nameWithUnderline) ? nameWithUnderline : property.Name;
                if (kv.ContainsKey(name))
                {

                    if (valueParser != null && valueParser(property, kv, kv[name], out object outObj))
                    {
                        property.SetValue(obj, outObj);
                    }
                    else if (property.PropertyType == typeof(int))
                    {
                        property.SetValue(obj, int.TryParse(Convert.ToString(kv[name]), out var outVar) ? outVar : 0);

                    }
                    else if (property.PropertyType == typeof(double))
                    {
                         
                        property.SetValue(obj, double.TryParse(Convert.ToString(kv[name]), out var outVar) ? outVar : 0);

                    }
                    else if (property.PropertyType == typeof(ushort))
                    {
                        property.SetValue(obj, ushort.TryParse(Convert.ToString(kv[name]), out var outVar) ? outVar : (ushort)0);

                    }
                    else if (property.PropertyType == typeof(string[]))
                    {
                        property.SetValue(obj, Convert.ToString(kv[name]).Split(','));
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        property.SetValue(obj, Convert.ToString(kv[name]));
                    }
                    else if (property.PropertyType == typeof(bool) && int.TryParse(Convert.ToString(kv[name]), out var outVar))
                    {
                        property.SetValue(obj, outVar != 0);
                    }
                }
            }
            // return obj as T;
        }

        public static T BindDataRow(DataRow row, PropertyValueParser<DataRow> valueParser)
        {
            return BindDataRow(row, valueParser, (info) => { return Regex.Replace(info.Name, "([A-Z])", "_$1").ToLower().TrimStart('_'); });
        }
        public static T BindDataRow(DataRow row, PropertyValueParser<DataRow> valueParser, PropertyNameParser nameParser)
        {

            T obj = Activator.CreateInstance<T>();
            foreach (var property in typeof(T).GetProperties())
            {
                string columnName = nameParser(property);
                object indata = null;
                if (row.Table.Columns.Contains(columnName) && property.CanWrite)
                {
                    indata = row[columnName];
                }

                if (valueParser != null && valueParser(property, row, indata, out object outObj))
                {
                    if (outObj != null)
                        property.SetValue(obj, outObj);
                }
                else if (property.PropertyType == typeof(double))
                {
                    property.SetValue(obj, double.TryParse(Convert.ToString(indata), out var outVar) ? outVar : 0);

                }
                else if (property.PropertyType == typeof(int))
                {
                    int val = int.TryParse(Convert.ToString(indata), out int outVal) ? outVal : 0;
                    property.SetValue(obj, val);
                }
                else if (property.PropertyType == typeof(uint))
                {
                    uint val = uint.TryParse(Convert.ToString(indata), out var outUint) ? outUint : 0;
                    property.SetValue(obj, val);
                }
                else if (property.PropertyType == typeof(short))
                {
                    short val = short.TryParse(Convert.ToString(indata), out var outVal) ? outVal : (short)0;
                    property.SetValue(obj, val);
                }
                else if (property.PropertyType == typeof(ushort))
                {
                    ushort val = ushort.TryParse(Convert.ToString(indata), out ushort outVal) ? outVal : (ushort)0;
                    property.SetValue(obj, val);
                }
                else if (property.PropertyType == typeof(string[]))
                {
                    property.SetValue(obj, Convert.ToString(indata).Split(','));
                }
                else if (indata != null && indata.GetType() == typeof(string))
                {
                    property.SetValue(obj, Convert.ToString(indata));
                }

            }
            return obj;

        }
        public static Dictionary<string, int> ToInt32Dictionary(DataRow row)
        {
            Dictionary<string, int> kv = new Dictionary<string, int>();
            foreach (DataColumn column in row.Table.Columns)
            {
                kv.Add(column.ColumnName, int.Parse(row[column].ToString()));
            }
            return kv;
        }
        public static Dictionary<string, object> ToDictionary(DataRow row)
        {
            Dictionary<string, object> kv = new Dictionary<string, object>();
            foreach (DataColumn column in row.Table.Columns)
            {
                kv.Add(column.ColumnName, row[column]);
            }
            return kv;
        }
        public static Dictionary<string, object> ToDictionary(T model)
        {
            return ToDictionary(model, null);
        }
        public static Dictionary<string, object> ToDictionary(T model, PropertyValueParser<T> parser)
        {
            Dictionary<string, object> kv = new Dictionary<string, object>();

            foreach (var property in typeof(T).GetProperties())
            {
                if (property.CanWrite && property.CanRead)
                {

                    string name = Regex.Replace(property.Name, "([A-Z])", "_$1").ToLower().TrimStart('_');

                    if (parser != null && parser(property, model, property.GetValue(model, null), out object value))
                    {
                        if (value != null && value.GetType() == typeof(Dictionary<string, object>))
                        {
                            Dictionary<string, object> subvalues = (Dictionary<string, object>)value;
                            foreach (var sub in subvalues)
                            {
                                kv.Add(sub.Key, sub.Value);
                            }
                        }
                        else if (value != null)
                            kv.Add(name, value);
                    }
                    else if (property.PropertyType == typeof(ushort)
                        || property.PropertyType == typeof(short)
                        || property.PropertyType == typeof(uint)
                        || property.PropertyType == typeof(int)
                        || property.PropertyType == typeof(string)
                        || property.PropertyType == typeof(bool) 
                        || property.PropertyType == typeof(double))
                    {
                        kv.Add(name, Convert.ToString(property.GetValue(model, null)));
                    }
                }
            }
            return kv;
        }

    }
}
