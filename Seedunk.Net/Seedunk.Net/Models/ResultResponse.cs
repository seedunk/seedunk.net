namespace Seedunk.Net.Models
{
    public class ResultResponse : QueryResponseBase<bool>
    {
        public ResultResponse() { }
        public ResultResponse(string msg) : base(msg) { 
        
        }
        public ResultResponse(bool res) : base(res) { 
        
        }
        
    }
}
