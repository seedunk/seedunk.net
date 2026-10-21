namespace  Seedunk.Net.Models
{   
     public class QueryResponseBase<T>
    {

        public bool IsSuccess { get; set; } = false; 
        public T Content { get; set; }

        public string Message { get; set; } = "";

        public QueryResponseBase() { }
        public QueryResponseBase(T content)
        {
            IsSuccess = true;
            Content = content;
        }
        public QueryResponseBase(  string message)
        {
            IsSuccess = false; 
            Message = message;
        }
    }
}
