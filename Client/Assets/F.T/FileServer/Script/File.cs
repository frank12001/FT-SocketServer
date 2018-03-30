using System.IO;
using System.Net;

namespace FTServer.FileServer
{
    public class File
    {
        public static File Instance { get { return (instance != null) ? instance : instance = new File(); } }
        private static File instance;
        private File() { }

        private const string ServerIP = @"http://35.229.191.68/fileupload";
        /// <summary>
        /// 上傳
        /// </summary>
        /// <param name="absoluteFolderPath"> AssetBundle 所在資料夾的絕對路徑。 Ex : D:/Slither/AssetBundle/Android </param>
        public void Upload(string absoluteFolderPath)
        {
            _Upload(absoluteFolderPath, ServerIP);
        }
        private void _Upload(string absoluteFolderPath, string serverIP)
        {
            DirectoryInfo folder = new DirectoryInfo(absoluteFolderPath);
            //查找所有文件並上傳
            foreach (FileInfo NextFile in folder.GetFiles())
            {
                using (WebClient client = new WebClient())
                {
                    client.UploadFile(string.Format("{0}?FolderName={1}", serverIP, folder.Name), NextFile.FullName);
                }
            }
        }
    }
}
