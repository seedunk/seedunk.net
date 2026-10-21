
using System.Collections.Generic;

namespace Seedunk.Net.IModels
{
    public interface IModelData<T1> where T1:class,IModel
    { 
       
        int GetCount(Dictionary<string, object> where);
         
        List<T1> GetResult(Dictionary<string, object> where, Dictionary<string, string> orderBys, int limit, int span);

        bool TryGet(Dictionary<string, object> where, out T1 row);
          
        bool TryAdd(T1 obj, out object lastId); 
        
        bool TryUpdate(T1 obj);
        
        bool TryDelete(T1 obj);

    }
}
