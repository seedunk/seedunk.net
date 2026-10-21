
using System.Collections.Generic;

namespace Seedunk.Net.IModels
{
    public interface IDataBase<T1>:IQuery
    { 
        bool TryQueryRow(string sql, out Dictionary<string, object> row);
         
        int QueryCount(Dictionary<string, object> where);
         
        List<T1> QueryResult(Dictionary<string, object> where, Dictionary<string, string> orderBys, int limit, int span);

        List<T1> QueryResult(string sql);

     
        bool TryGetByPrimaryValue(string value, out T1 row);
        bool TryGet(Dictionary<string, object> wheres, out T1 row);
        bool TryGetRow(Dictionary<string, object> wheres, out T1 row);
        bool TryGetRow(string sql, out T1 row);
        
        bool TryDelete(string val);

        bool TryAdd(T1 obj); 
        bool TryAdd(T1 obj, out T1 newObject);
        bool TryAdd(T1 obj, out string lastId);
        bool TryAdd(Dictionary<string, object> kv, out string lastId);

        bool TryUpdate(T1 obj);

    }
}
