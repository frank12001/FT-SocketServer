using System;
using System.IO;
using System.Net;
using UnityEngine;

namespace FTServer.FileServer
{
    public static class File
    {
        //private const string DevServerIP = @"http://35.229.191.68:5000/fileupload";
        //private const string TaiwanServerIP = @"http://35.185.130.215:5002/fileupload";
        public const string DownloadString = "filedownload";
        private const string UploadString = "fileupload";

        public static void UploadToTaiwan(string targetIP, string absoluteFolderPath)
        {
            Uri uri = new Uri(targetIP);
            upload($"http://{uri.Host}:{uri.Port}/{UploadString}", absoluteFolderPath);
        }

        /// <summary>
        /// </summary>
        /// <param name="serverIP"></param>
        /// <param name="absoluteFolderPath"> AssetBundle 所在資料夾的絕對路徑。 Ex : D:/Slither/AssetBundle/Android </param>
        private static void upload(string serverIP, string absoluteFolderPath)
        {
            Debug.Log($"Upload file to FileServer. IP:{serverIP}, Path:{absoluteFolderPath}");

            var directory = new DirectoryInfo(absoluteFolderPath);

            //查找所有文件並上傳
            foreach(FileInfo file in directory.GetFiles())
            {
                using(WebClient client = new WebClient())
                {
                    var address = $"{serverIP}?FolderName={directory.Name}";
                    client.UploadFile(address, file.FullName);
                }
            }
        }
    }
}
