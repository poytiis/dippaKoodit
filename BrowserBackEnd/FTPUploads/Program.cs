using FluentFTP;
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using Renci.SshNet;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;
using System.Net.WebSockets;
using System.Threading;
using System.Text;
using Renci.SshNet.Messages;
using System.Net.Http;
using WebDav;
using WinSCP;
using System.Net;

namespace FTPUploads
{
    class SFTPCredentials
    {
        public string SFTPPass { get; set; }
        public string SFTPUser { get; set; }
    }

    class Program
    {
        private static readonly string _serverIP = "20.101.97.117";
        private static readonly string _serverDomainName = "dippa.test:5000";
        private static readonly string _serverDomainNameSecure = "dippa.test:5001";
        private static readonly string _averageResultFilePath = @"C:\Users\OWNER\dippa\dippa-teemup\BrowserBackEnd\FTPUploads\ftpUploadTimesAverage.csv";
        private static readonly string _resultFilePath = @"C:\Users\OWNER\dippa\dippa-teemup\BrowserBackEnd\FTPUploads\ftpUploadTimes.csv";
        private static readonly int _uploadTimes = 1;

        private static string GenerateRandomFileName(string filePath)
        {
            var random = new Random();
            var fileName = filePath.Split('/')[^1];
            var fileNameSplit = fileName.Split('.');
            return fileNameSplit[0] + random.Next(0, 100000).ToString() + fileNameSplit[1];
        }

        private static string FTPUpload(string filePath, bool useFTPS)
        {
            
            var fileSize = new FileInfo(filePath).Length;
            var randomFileName = GenerateRandomFileName(filePath);

            var secretJsonPath = @"C:\Users\OWNER\dippa\dippa-teemup\BrowserBackEnd\FTPUploads\secrets.json";
            using var reader = new StreamReader(secretJsonPath);
            var jsonString = reader.ReadToEnd();

            var credentials = JsonSerializer.Deserialize<SFTPCredentials>(jsonString);

            var startTime = DateTime.UtcNow;

            using var client = new FtpClient(_serverIP, credentials.SFTPUser, credentials.SFTPPass);
            if(useFTPS)
            {
                client.EncryptionMode = FtpEncryptionMode.Auto;
                client.ValidateAnyCertificate = true;
            }
            else
            {
                client.EncryptionMode = FtpEncryptionMode.None;
            }
            
            client.Connect();
            client.SetWorkingDirectory("files");
            client.UploadFile(filePath, randomFileName);

            var endTime = DateTime.UtcNow;
            client.Disconnect();

            var uploadTime = (endTime - startTime).TotalSeconds;
            var protocolName = useFTPS ? "FTPS" : "FTP";
            var filePathSplit = filePath.Split('/');

            return protocolName + "," + filePathSplit[^1] + "," + fileSize.ToString() + "," + uploadTime.ToString(CultureInfo.InvariantCulture) + ",C#\n";
        }

        private static string SFTPUpload(string filePath)
        {
            var randomFileName = GenerateRandomFileName(filePath);
            var secretJsonPath = @"C:\Users\OWNER\dippa\dippa-teemup\BrowserBackEnd\FTPUploads\secrets.json";
            using var reader = new StreamReader(secretJsonPath);
            var jsonString = reader.ReadToEnd();

            var credentials = JsonSerializer.Deserialize<SFTPCredentials>(jsonString);

            var fileSize = new FileInfo(filePath).Length;
            var startTime = DateTime.UtcNow;

            using var client = new SftpClient(_serverIP, 22, credentials.SFTPUser, credentials.SFTPPass);
            client.Connect();

            using var fileStream = new FileStream(filePath, FileMode.Open);
            client.UploadFile(fileStream, randomFileName);
            var endTime = DateTime.UtcNow;
            client.Disconnect();

            var uploadTime = (endTime - startTime).TotalSeconds;
            var protocolName = "SFTP";
            var filePathSplit = filePath.Split('/');

            return protocolName + "," + filePathSplit[^1] + "," + fileSize.ToString() + "," + uploadTime.ToString(CultureInfo.InvariantCulture) + ",C#\n";
        }

        private async static Task<string> WebDavUpload(string filePath)
        {
            var secretJsonPath = @"C:\Users\OWNER\dippa\dippa-teemup\BrowserBackEnd\FTPUploads\secrets.json";
            using var reader = new StreamReader(secretJsonPath);
            var jsonString = reader.ReadToEnd();

            var credentials = JsonSerializer.Deserialize<SFTPCredentials>(jsonString);


            var startTime = DateTime.UtcNow;
            var clientParams = new WebDavClientParams 
            { 
                BaseAddress = new Uri("http://dippa.test/webdav/"),
                Credentials = new NetworkCredential(credentials.SFTPUser, credentials.SFTPPass)
            };
            using (var client = new WebDavClient(clientParams))
            {
                await client.PutFile("webdav_image1M.png", new FileStream(filePath, FileMode.Open));
            }
            var endTime = DateTime.UtcNow;
            var uploadTime = (endTime - startTime).TotalSeconds;
            var fileSize = new FileInfo(filePath).Length;
            var protocolName = "WebDav";
            var filePathSplit = filePath.Split('/');
            return protocolName + "," + filePathSplit[^1] + "," + fileSize.ToString() + "," + uploadTime.ToString(CultureInfo.InvariantCulture) + ",C#\n";
        }


        private async static Task<string> WebsocketUpload(string filePath)
        {
            var startTime = DateTime.UtcNow;
            var fileSize = new FileInfo(filePath).Length;
            var filePathSplit = filePath.Split('/');
            double uploadTime = 0;
            using (var ws = new ClientWebSocket())
            {
                await ws.ConnectAsync(new Uri("ws://" + _serverDomainName), CancellationToken.None);
                var fileBytes = File.ReadAllBytes(filePath);
                await ws.SendAsync(fileBytes, WebSocketMessageType.Binary, true, CancellationToken.None);
                var endTime = DateTime.UtcNow;
                uploadTime = (endTime - startTime).TotalSeconds;

                await ws.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
               
            }
            return "WebSocket" + "," + filePathSplit[^1] + "," + fileSize.ToString() + "," + uploadTime.ToString(CultureInfo.InvariantCulture) + ",C#\n";
        }

        private async static Task<string> HTTPUpload(string filePath, bool useHTTPS = false)
        {
            var startTime = DateTime.UtcNow;
            var fileSize = new FileInfo(filePath).Length;
            var filePathSplit = filePath.Split('/');

            using var client = new HttpClient();
            var buffer = File.ReadAllBytes(filePath);

            var multipartContent = new MultipartFormDataContent();
            var byteArrayContent = new ByteArrayContent(buffer);
            //var FileNameContent = new StringContent("Image");
            //var PieceNumberContent = new StringContent("1");
            multipartContent.Add(byteArrayContent, "PieceData", "filename");
            //multipartContent.Add(FileNameContent, "FileName");
            //multipartContent.Add(PieceNumberContent, "PieceNumber");
            var url = "http://" + _serverDomainName + "/api/HTTP/UploadFilePieceForm";
            var protocol = "HTTP";
            if (useHTTPS)
            {
                url = "https://" + _serverDomainNameSecure + "/api/HTTP/UploadFilePieceForm";
                protocol = "HTTPS";
            }
            var postResponse = await client.PostAsync(url, multipartContent);
            var endTime = DateTime.UtcNow;
            var uploadTime = (endTime - startTime).TotalSeconds;
            return protocol + "," + filePathSplit[^1] + "," + fileSize.ToString() + "," + uploadTime.ToString(CultureInfo.InvariantCulture) + ",C#\n";

        }

        private static void CalcAverageTimes()
        {
            if (File.Exists(_averageResultFilePath))
            {
                File.Delete(_averageResultFilePath);
            }

            var fileContent = File.ReadAllText(_resultFilePath);
            var fileRows = fileContent.Split('\n');
            var averageDict = new Dictionary<string, float> { };
            var avaregeRusults = "protocol,file_name,file_size,upload_time,client\n";
            for (var i = 0; i < fileRows.Length; i++)
            {
                var rowSplit = fileRows[i].Split(',');
                if(rowSplit[0] == "protocol" || rowSplit[0] == "")
                {
                    continue;
                }

                var rowKey = rowSplit[0] + "," + rowSplit[1] + "," + rowSplit[2];
                var rowValueStr = rowSplit[3].Trim();
                var rowValue = float.Parse(rowValueStr, CultureInfo.InvariantCulture.NumberFormat);
                if (averageDict.ContainsKey(rowKey))
                {
                    averageDict[rowKey] = averageDict[rowKey] + rowValue;
                }
                else
                {
                    averageDict.Add(rowKey, rowValue);
                }
            }

            foreach (var average in averageDict)
            {
                var averageTime = Math.Round(average.Value / _uploadTimes, 2);
                avaregeRusults += average.Key + "," + averageTime + ",C#\n";
            }

            File.WriteAllText(_averageResultFilePath, avaregeRusults);

        }

        static async Task Main(string[] args)
        {
            var filePaths = new List<string>{ @"C:/Users/OWNER/dippa/uploadData/image1M.jpg"};
            // var filePaths = new List<string>{ @"C:/Users/OWNER/dippa/uploadData/dummy10M.txt", @"C:/Users/OWNER/dippa/uploadData/dummy100M.txt" };

            var csvRows = "protocol,file_name,file_size,upload_time,client\n";

            
            if (File.Exists(_resultFilePath))
            {
                File.Delete(_resultFilePath);
            }

           // await HTTPUpload(filePaths[0]);


            foreach (var path in filePaths)
            {
                for (var i = 0; i < _uploadTimes; i++)
                {
                    //csvRows += await HTTPUpload(path, true);
                    //csvRows += await HTTPUpload(path);

                    //csvRows += await WebsocketUpload(path);
                    csvRows += await WebDavUpload(path);

                    //csvRows += FTPUpload(path, false);
                    //csvRows += FTPUpload(path, true);
                    //csvRows += SFTPUpload(path);

                }
            }

            Console.WriteLine(csvRows);
            File.WriteAllText(_resultFilePath, csvRows);
            CalcAverageTimes();
        }
    }
}
