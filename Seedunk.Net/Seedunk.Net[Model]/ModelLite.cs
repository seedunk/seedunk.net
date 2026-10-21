using Seedunk.Net.IModels;
using Seedunk.Net.Models;
using Seedunk.Net.Utils;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;

namespace Seedunk.Net
{
    public class ModelLite<T> : IModelData<T> where T : class, IModel
    {
        protected T Model { get; set; }
        
        public IQuery Query { get; set; }
        public ModelLite(IQuery query) {
            Query = query; 
            Model = (T)Activator.CreateInstance(typeof(T));
        }
         
        public int GetCount(Dictionary<string, object> where)
        {
            throw new NotImplementedException();
        }

        public List<T> GetResult(Dictionary<string, object> where, Dictionary<string, string> orderBys, int limit, int span)
        {
            string whereSql = where == null ? "" : (" where " + where.ToAndSQL());
            if (orderBys != null)
            {
                whereSql += orderBys.ToOrderSQL();
            }
          
            QueryResponse res = Query.GetDataTable("select * from " + Model.ModelName + whereSql + " limit " + limit + "," + span + ";");
            List<T> result = new List<T>();
            for (int i = 0; i < res.Rows.Count; i++)
            {
                Dictionary<string, object> kv = ModelUtil<DataRow>.ToDictionary(res.Rows[i]);
                 T model = (T)Activator.CreateInstance(typeof(T));
                   model.BindDictionary(kv);
                 result.Add(model);
            }
            return result;
        }

       
           
         
        public bool TryAdd(T obj, out object outObject)  
        { 
           
            List<string> columnNames = new List<string>();
            List<string> columnValues = new List<string>();
            List<string> columnStringValues = new List<string>();

            Dictionary<string, object> values = new Dictionary<string, object>();
            foreach (var property in obj.ToDictionary())
            {

                string columnName = property.Key ;
                if (property.Value != null && !columnName.Equals(Model.PrimaryPropertyName))
                {
                    columnNames.Add(columnName);
                    columnValues.Add("@" + columnName);
                    values.Add(columnName, property.Value);
                    columnStringValues.Add("'" + Convert.ToString(property.Value) + "'");
                }

            }
            string sql = "insert into " + Model.ModelName
                            + "(" + string.Join(",", columnNames)
                            + ") values(" + string.Join(",", columnValues) + ")";

            Debug.WriteLine("insert into " + Model.ModelName
                                + "(" + string.Join(",", columnNames)
                                + ") values(" + string.Join(",", columnStringValues) + ")");
            var ret =  Query.ExecuteNonQuery(sql, values);
            if (ret.IsSuccess && ret.Content > 0)
            {
                var idRes = Query.GetDataTable("select max(" + Model.PrimaryPropertyName + ") as 'LastId' from " + Model.ModelName + " LIMIT 1;");
                if (idRes.IsSuccess && idRes.Rows.Count == 1)
                {
                    outObject = Convert.ToString(idRes.Rows[0]["LastId"]);
 
                    return true;
                }

            }
            outObject = null;
            return false;
        }

        public bool TryDelete(T obj)
        {
            throw new NotImplementedException();
        }

        public bool TryGet(Dictionary<string, object> where, out T model)
        {
            string whereSql = where == null ? "" : (" where " + where.ToAndSQL());
            string sql = "select * from " + Model.ModelName + whereSql + " limit 0,1;";
            Debug.WriteLine("sql:" + sql);
            QueryResponse res = Query.GetDataTable(sql);

            if (res.Rows.Count == 1)
            {
                Dictionary<string, object> kv = ModelUtil<DataRow>.ToDictionary(res.Rows[0]);
                model = (T)Activator.CreateInstance(typeof(T));
                model.BindDictionary(kv);
                return true;
            }
            else {
                model = null;
                return false;
            }  
        }

        public bool TryUpdate(T obj)
        {
            throw new NotImplementedException();
        }
    }
}
