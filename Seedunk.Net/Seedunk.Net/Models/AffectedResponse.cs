namespace  Seedunk.Net.Models
{
    public class AffectedResponse : QueryResponseBase<int>
    {
        public AffectedResponse() { }
        public AffectedResponse(string msg) : base(msg) { 
        
        }
        public AffectedResponse(int res) : base(res) { 
        
        }
        
    }
}
