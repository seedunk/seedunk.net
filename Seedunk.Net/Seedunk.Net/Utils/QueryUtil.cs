
using System;
using System.Collections.Generic;
using System.Diagnostics;
namespace Seedunk.Net.Utils
{
    /// <summary>
    /// 检测表失败回调函数
    /// </summary>
    public delegate void CheckTableFailBack();
    public class QueryUtil
    {
        IQuery Query;
        public QueryUtil(IQuery query)
        {
            Query = query;
        }
        public void ExecuteScalar(string[] sqls)
        {
            for (int i = 0; i < sqls.Length; i++)
            {
                Query.ExecuteScalar(sqls[i]);
            }
        }


        public bool Check(string tableName, CheckTableFailBack back = null)
        {
            return Check(Query, tableName, back);
        }
        public static int Count(IQuery query, string tableName, Dictionary<string, string> ps)
        {
            string sql = "select count(0) as 'count' from " + tableName + " where 1=1 ";
            foreach (KeyValuePair<string, string> kvp in ps)
            {
                sql += " and " + kvp.Key + " ='" + kvp.Value + "' ";
            }
            var res = query.GetDataTable(sql);
            if (res.IsSuccess && res.Rows.Count == 1)
            {
                return int.Parse(Convert.ToString(res.Rows[0]["count"]));
            }
            else return 0;
        }
        public static bool Check(IQuery query, string tableName, CheckTableFailBack back = null)
        {
            string checkInit = "select * from sqlite_master where type = 'table' and name = '" + tableName + "'";
            var res = query.GetDataTable(checkInit);
            // Debug.WriteLine("check:" + tableName +"="+ res.Rows.Count);
            if (res.IsSuccess && res.Rows.Count == 0)
            {
                if (back != null)
                {
                    back();
                }
                return false;
            }
            else
            {
                return true;
            }
        }
    }
}
