using System;
using System.Collections.Generic;
using System.IO;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using TCPServer.ClientInstance;
using TCPServer.ClientInstance.Packet;
using TCPServer.Projects.Palace;
using TCPServer.Projects.Stellar;
using TCPServer.Rooms.Operator;


namespace TCPServer
{
    public partial class Form1 : Form , IApplication
    {

        private List<ClientNode> mlClientSocks;
        public List<ClientNode> MlClientSockets { get { return mlClientSocks; } }

        TcpListener mTCPListener;


        /// <summary>
        /// 給予 ClientNode的 TcpClient，接收輸入串流陣列的大小
        /// </summary>
        private const uint inputBufferSize = 4096;//88320;
        public uint InputBufferSize { get { return inputBufferSize; } }

        #region 給 ClientNode 的介面接口

        public System.Windows.Forms.ListBox LbClients { get { return this.lbClients; } }
        public void PrintLine(string _strPrint)
        {
            this.printLine(_strPrint);
        }

        #endregion

        public Operator RoomOperator { get { return roomOperator; } }
        private Operator roomOperator;

        public Form1()
        {
            InitializeComponent();
            mlClientSocks = new List<ClientNode>(2);
            CheckForIllegalCrossThreadCalls = false;

            Setup();
        }

        protected void Setup()
        {
            //roomOperator = new Operator(this);
            //roomOperator = new PokerQueueOperator(this);
            roomOperator = new PalaceQueueOperator(this);
            printLine(" Server 開機");
            printLine("Setup Finish");
        }

        IPAddress findMyIPV4Address()
        {
            string strThisHostName = string.Empty;
            IPHostEntry thisHostDNSEntry = null;
            IPAddress[] allIPsOfThisHost = null;
            IPAddress ipv4Ret = null;

            try
            {
                strThisHostName = System.Net.Dns.GetHostName();

                printLine(strThisHostName);

                thisHostDNSEntry = System.Net.Dns.GetHostEntry(strThisHostName);

                allIPsOfThisHost = thisHostDNSEntry.AddressList;

                for (int idx = allIPsOfThisHost.Length-1; idx > 0; idx--)
                {
                    if (allIPsOfThisHost[idx].AddressFamily == AddressFamily.InterNetwork)
                    {
                        ipv4Ret = allIPsOfThisHost[idx];
                    }
                }
            }
            catch (Exception exc)
            {
                printLine(exc.Message);
                //MessageBox.Show(exc.Message);
            }

            return ipv4Ret;
        }

        private async void btnStartListening_Click(object sender, EventArgs e)
        {
            IPAddress ipaddr;
            int nPort;

            if (int.TryParse(tbPort.Text, out nPort))
            {
                nPort = 23000;
            }
            if (!IPAddress.TryParse(tbIPAddress.Text, out ipaddr))
            {
                MessageBox.Show("Invalid IP address supplied.");
                return;
            }
                                 //IPAddress.Any //ipaddr
            mTCPListener = new TcpListener(IPAddress.Any, nPort);

            mTCPListener.Start();

            printLine("start listening");

            while (true)
            {
                //mTCPListener.BeginAcceptTcpClient(onCompleteAcceptTcpClient, mTCPListener);
                TcpClient tcpc = await mTCPListener.AcceptTcpClientAsync();
                ClientNode cNode = cNode = new PalacePeer(this, tcpc, new byte[InputBufferSize],
                    new byte[InputBufferSize], tcpc.Client.RemoteEndPoint.ToString(), this);

                //開啟 TcpClient 的輸入串流
                tcpc.ReceiveBufferSize = (int) InputBufferSize;
                tcpc.GetStream().BeginRead(cNode.Rx, 0, cNode.Rx.Length, cNode.onCompleteReadFromTCPClientStream, tcpc);

                //使用此 TcpClient 製作 ClientNode 
                mlClientSocks.Add(cNode);
                //將 ClientNode 放到 畫面中顯示
                lbClients.Items.Add(cNode.ToString());

            }

        }       



        #region 顯示文字

        public void printLine(string _strPrint)
        {
            if(load)
                tbConsoleOutput.Invoke(new Action<string>(doInvoke), _strPrint);
        }

        public void doInvoke(string _strPrint)
        {
            tbConsoleOutput.Text = _strPrint + Environment.NewLine + tbConsoleOutput.Text;
        }

        #endregion 

        private void btnSend_Click(object sender, EventArgs e)
        {
            ClientNode cn = mlClientSocks.Find(x => x.strId == lbClients.SelectedItem.ToString());
            Dictionary<byte, object> dic = new Dictionary<byte, object>()
            {
                {1,new EventData(1,new Dictionary<byte, object>()){ ForTest = "2" } },
            };
            cn.SendEvent(new EventData(5, dic) { ForTest = tbPayload.Text });
            //cn.SendEvent(new TestPacket(new Dictionary<byte, object>()) { Text = tbPayload.Text });
            
        }

        private void btnFindIPv4IP_Click(object sender, EventArgs e)
        {
            IPAddress ipa = null;
            ipa = findMyIPV4Address();
            if (ipa != null)
            {
                tbIPAddress.Text = ipa.ToString();
            }
        }

        bool load = false;
        private void Form1_Load(object sender, EventArgs e)
        {
            load = true;
        }
    }
}
