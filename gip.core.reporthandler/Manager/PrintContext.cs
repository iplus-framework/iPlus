using System.Net.Sockets;
using System.Text;
using System.Windows.Documents;

namespace gip.core.reporthandler
{
    public class PrintContext
    {
        public byte[] Main { get; set; }

        public FlowDocument FlowDocument { get; set; }
        public NetworkStream NetworkStream { get; set; }

        public TcpClient TcpClient { get; set; }

        public Encoding Encoding { get; set; }

        public int ColumnMultiplier { get; set; }
        public int ColumnDivisor { get; set; }

    }
}
