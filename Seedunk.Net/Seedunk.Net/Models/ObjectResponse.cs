namespace Seedunk.Net.Models
{
    public class ObjectResponse : QueryResponseBase<object>
    {
        public ObjectResponse() { }
        public ObjectResponse(string msg) : base(msg) { 
        
        }
        public ObjectResponse(object res) : base(res) { 
        
        }
        public int ToInt() {
            return int.Parse(Content.ToString());
        }
        
    }
}
