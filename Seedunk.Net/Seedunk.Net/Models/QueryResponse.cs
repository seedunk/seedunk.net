 
using System.Data;
namespace Seedunk.Net.Models
{
    public class QueryResponse : QueryResponseBase<DataTable>
    {
        public QueryResponse() { }
        public QueryResponse(string msg) : base(msg) { 
        
        }
        public QueryResponse(DataTable res) : base(res) { 
        
        }
        public DataRowCollection Rows  {
            get {
                if (IsSuccess && Content != null)
                {
                    return Content.Rows;
                }
                else
                {
                    return null;
                }
            }
            
         }
        
    }
}
