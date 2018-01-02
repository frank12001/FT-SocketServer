using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using startOnline;
using TCPServer.Projects.Stellar;

namespace TCPServer.Rooms.Operator
{
    public class BaseQueueInfo
    {
        public string Key;
        public PeerBase Peer;
        public string Guid;
    }

    public class QueueOperator<T> : Operator where T : BaseQueueInfo
    {
        private byte HowMuchPlayersJoinRoom = 6;
        public Dictionary<string,List<BaseQueueInfo>> _Queue;

        public QueueOperator(Form1 form1,byte howMuchPlayersJoinRoom) : base(form1)
        {
            _Queue = new Dictionary<string, List<BaseQueueInfo>>();
            HowMuchPlayersJoinRoom = howMuchPlayersJoinRoom;
        }

        /// <summary>
        /// 當加入排隊後，達到遊玩人數的回傳
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public virtual Room QueueJoinSuccess(List<BaseQueueInfo> infos)
        {
            return null;
        }
        /// <summary>
        /// 當加入排隊後，未達遊玩人數時呼叫
        /// </summary>
        /// <param name="infos"></param>
        /// <returns></returns>
        public virtual Room QueueJoinFail(List<BaseQueueInfo> infos)
        {
            return null;
        }
        /// <summary>
        /// 加入列隊
        /// </summary>
        /// <param name="info">排隊的資訊</param>
        /// <returns></returns>
        public virtual Room QueueJoin(T info)
        {
            lock ("QueueJoin")
            {
                Room room = null;
                //解出分類 Key
                var queryKey = info.Key;
                List<BaseQueueInfo> queue = null;
                if (!_Queue.TryGetValue(queryKey, out queue)) //如果沒有這個分類的列隊
                {
                    //創一個新列隊，並將自己加入
                    queue = new List<BaseQueueInfo>() { info };
                    _Queue.Add(queryKey, queue);
                }
                else
                {
                    //將自己加入列隊
                    queue.Add(info);
                }
                //檢查此列隊的人數，是否達到足夠開房
                if (queue.Count.Equals(HowMuchPlayersJoinRoom))
                {
                    room = QueueJoinSuccess(queue);
                }
                else
                {
                    room = QueueJoinFail(queue);
                }
                return room;
            }
        }

        /// <summary>
        /// 離開排隊
        /// </summary>
        /// <param name="guid">該 Peer 的 Guid</param>
        /// <returns></returns>
        public bool ExitQueue(string guid)
        {
            foreach (KeyValuePair<string, List<BaseQueueInfo>> pair in _Queue)
            {
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    if (pair.Value[i].Guid.Equals(guid))
                    {
                        pair.Value.Remove(pair.Value[i]);
                        return true;
                    }
                }
            }

            return false;
        }
    }
}
