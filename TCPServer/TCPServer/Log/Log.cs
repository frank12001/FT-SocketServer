using System.IO;


namespace startOnline.Log
{
    public class Log
    {
        /// <summary>
        /// log 文字到預設目錄
        /// </summary>
        /// <param name="content"></param>
        static public void ToTxt(string content)
        {
            string str = System.Environment.CurrentDirectory; //取得現在執行檔案的位置
            //將位置改成 startOnline 資料夾裡
            string str2 = str.Remove(str.LastIndexOf("CounterPublisher"));
            str2 += @"startOnline\";
            str2 += @"TestPhotonServerLog.txt"; //檔名
            //寫入資料串流
            StreamWriter writer = new StreamWriter(str2, true);
            writer.WriteLine(System.DateTime.Now + "  " + content);
            writer.Close();
        }

        static public string GetTxt()
        {
            string result = "";

            string str = System.Environment.CurrentDirectory; //取得現在執行檔案的位置
            //將位置改成 startOnline 資料夾裡
            string str2 = str.Remove(str.LastIndexOf("CounterPublisher"));
            str2 += @"startOnline\";
            str2 += @"TestPhotonServerLog.txt"; //檔名

            StreamReader reader = new StreamReader(str2);
            while (!reader.EndOfStream)
            {
                string lineFromThePlan = reader.ReadLine();
                result += lineFromThePlan + "\r\n";
            }
            reader.Close();
            return result;

        }
    }
}
