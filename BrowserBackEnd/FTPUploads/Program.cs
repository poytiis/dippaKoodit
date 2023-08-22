using FluentFTP;
using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using Renci.SshNet;
using System.Text.Json;
using System.Threading.Tasks;
using System.Linq;

namespace FTPUploads
{
    class SFTPCredentials
    {
        public string SFTPPass { get; set; }
        public string SFTPUser { get; set; }
    }

    class Program
    {
        private static readonly string _serverIP = "52.190.63.78";
        private static readonly string _averageResultFilePath = @"D:\dippa-teemup\BrowserBackEnd\FTPUploads\ftpUploadTimesAverage.csv";
        private static readonly string _resultFilePath = @"D:\dippa-teemup\BrowserBackEnd\FTPUploads\ftpUploadTimes.csv";
        private static readonly int _uploadTimes = 5;

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
            
            var startTime = DateTime.UtcNow;

            using var client = new FtpClient(_serverIP);
            if(useFTPS)
            {
                client.EncryptionMode = FtpEncryptionMode.Auto;
                client.ValidateAnyCertificate = true;
            }
            
            client.Connect();
            client.SetWorkingDirectory("upload");
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
            var secretJsonPath = @"D:\dippa-teemup\BrowserBackEnd\FTPUploads\secrets.json";
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

        static void Main(string[] args)
        {
            var filePaths = new List<string>{ "D:/dippa/uploadData/kilo.txt", "D:/dippa/uploadData/mega.txt", "D:/dippa/uploadData/tenMega.txt" };

            var csvRows = "protocol,file_name,file_size,upload_time,client\n";

            
            if (File.Exists(_resultFilePath))
            {
                File.Delete(_resultFilePath);
            }

            FTPUpload(filePaths[0], false);
            foreach (var path in filePaths)
            {
                for (var i = 0; i < _uploadTimes; i++)
                {
                    csvRows += FTPUpload(path, false);
                    csvRows += FTPUpload(path, true);
                    csvRows += SFTPUpload(path);
                }
            }

            Console.WriteLine(csvRows);
            File.WriteAllText(_resultFilePath, csvRows);
            CalcAverageTimes();
        }
    }
}
