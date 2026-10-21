 
namespace  Seedunk.Net.Models
{
    public class TCPAddress
    {
        public string Ip { get; set; }
        public ushort Port { get; set;  }
        public ushort ConnectTimeout { get; set; } = 1000;
        public ushort ReadTimeout { get; set; } = 1000;
        public ushort WriteTimeout { get; set; } = 1000;
    }
}
