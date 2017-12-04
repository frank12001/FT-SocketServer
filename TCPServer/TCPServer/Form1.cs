using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Net.Sockets;
using TCPServer.playar.Rooms.Operator;
using TCPServer.ClientInstance;
using TCPServer.ClientInstance.Packet;
using startOnline;
using startOnline.DataBase;
using startOnline.playar.Rooms;


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
        private const uint inputBufferSize = 102400;//88320;
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

            roomOperator = new Operator(this);
            printLine(" Server 開機");
            printLine("Setup Finish");

        }

        #region Room Function
        public Room Room_Create(PlayarPeer peer, string serialId, RoomTypes roomType)
        {
            return RoomOperator.Room_Create(peer, serialId, roomType);
        }
        /// <summary>
        /// 創建房間 (從既有的房間創建)
        /// </summary>
        /// <param name="source">既有的房間</param>
        /// <param name="roomType">新房的種類</param>
        /// <returns>新房間</returns>
        public Room Room_Create(Room source, RoomTypes roomType)
        {
            return RoomOperator.Room_Create(source,roomType);
        }
        /// <summary>
        /// join assign room 
        /// </summary>
        /// <param name="roomIndexInApplication">the room's guid</param>
        /// <param name="peer">Joiner's peer</param>
        /// <param name="playid">return playid if join sucess</param>
        /// <returns>if not sucess return null</returns>
        public Room Room_Join(string roomIndexInApplication, PlayarPeer peer, out byte playid)
        {
            return RoomOperator.Room_Join(roomIndexInApplication, peer,out playid);
        }
        public void Room_Remove(string index)  //給其他人的移除功能
        {
            RoomOperator.Room_Remove(index);
        }
        public bool Room_IsRoomExist(string roomIndexInApplication)
        {
            return RoomOperator.Room_IsRoomExist(roomIndexInApplication);
        }
        /// <summary>
        /// 回傳房間總數
        /// </summary>
        /// <returns></returns>
        public int GetRoomsCount()
        {
            return RoomOperator.Rooms.Count;
        }

        #endregion


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
                MessageBox.Show(exc.Message);
            }

            return ipv4Ret;
        }

        private void btnStartListening_Click(object sender, EventArgs e)
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
                                        //IPAddress.Any
            mTCPListener = new TcpListener(ipaddr, nPort);

            mTCPListener.Start();

            mTCPListener.BeginAcceptTcpClient(onCompleteAcceptTcpClient, mTCPListener);

            printLine("start listening");
        }
        /// <summary>
        /// 當接收到 Client 連線時呼叫
        /// </summary>
        /// <param name="iar"></param>
        void onCompleteAcceptTcpClient(IAsyncResult iar)
        {
            TcpListener tcpl = (TcpListener)iar.AsyncState;
            TcpClient tclient = null;
            ClientNode cNode = null;

            try
            {
                //將這次連線進來的 TcpClient 存起來
                tclient = tcpl.EndAcceptTcpClient(iar); ;

                printLine("Client Connected..");

                //將這次  TcpClient 存入後就繼續監聽，是否有人連近來
                tcpl.BeginAcceptTcpClient(onCompleteAcceptTcpClient, tcpl);
                //實力化 ClientNode ，傳入 連線進來的 TcpClient 
                //cNode = new ClientNode(tclient, new byte[InputBufferSize],
                //                        new byte[InputBufferSize], tclient.Client.RemoteEndPoint.ToString(),this);
                cNode = new PlayarPeer(this,tclient, new byte[InputBufferSize],
                                        new byte[InputBufferSize], tclient.Client.RemoteEndPoint.ToString(),this);

                //開啟 TcpClient 的輸入串流
                tclient.ReceiveBufferSize = (int)InputBufferSize;
                //tclient.SendBufferSize = (int)InputBufferSize;
                tclient.GetStream().BeginRead(cNode.Rx, 0, cNode.Rx.Length, cNode.onCompleteReadFromTCPClientStream, tclient);

                //將 ClientNode 加入 List
                lock (mlClientSocks)
                {
                    //使用此 TcpClient 製作 ClientNode 
                    mlClientSocks.Add(cNode);
                    //將 ClientNode 放到 畫面中顯示
                    lbClients.Items.Add(cNode.ToString());
                }
            }
            catch (Exception exc)
            {
                MessageBox.Show(exc.Message, "Errot", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
