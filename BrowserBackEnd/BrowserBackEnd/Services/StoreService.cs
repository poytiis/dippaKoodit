using Azure.Storage.Blobs;
using BrowserBackEnd.HTTPValidation;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using MySqlConnector;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace BrowserBackEnd.Services
{
    public interface IStoreService
    {
        Task<string> RunStoreComparison();
        Task RunStoreLargeFilesInSQLVersusFilesystem();
    }

    public class StoreService : IStoreService
    {
        private readonly string _resultFilePath;
        private readonly string _fileStorageStorePath;
        private readonly string _diskStorageStorePath;
        private readonly string _SQLConnectionString;
        private DateTime _startTime;
        private string _currentStoretype;
        private string _currentStoreContent;
        private long _fileSize;

        private readonly int _fileCountInFileSystem = 100;
        private readonly int _rowCountInSQL = 5000;

        private readonly CosmosClient _cosmosClient;
        private readonly BlobContainerClient _blobContainerClient;

        private readonly IConfiguration _configuraion;

        public StoreService(IConfiguration configuraion)
        {
            _configuraion = configuraion;

            _resultFilePath = configuraion.GetValue<string>("ResultFilePath");
            _fileStorageStorePath = configuraion.GetValue<string>("FileStorageFilePath");
            _diskStorageStorePath = configuraion.GetValue<string>("DiskStorageFilePath");
            _SQLConnectionString = configuraion.GetValue<string>("SQLConnectionString");

            var cosmosUrl = configuraion.GetValue<string>("CosmosUrl");
            var cosmosPrivateKey = configuraion.GetValue<string>("CosmosPrivateKey");
            var blobConnectionString = configuraion.GetValue<string>("FilePieceUploadStorageAccountConnectionString");

            var headerLine = "storeType,storeContent,storeTme\n";
            File.WriteAllText(_resultFilePath, headerLine);
           
            _cosmosClient = new CosmosClient(cosmosUrl, cosmosPrivateKey);          
            _blobContainerClient = new BlobContainerClient(blobConnectionString, "file-upload");
        }

        public async Task<string> RunStoreComparison()
        {
            var fileNames = new List<string> { "kilo.txt", "mega.txt", "tenMega" }; // , "large-1.jpg"
            var filePaths = new List<string> { @"D:\dippa\uploadData\kilo.txt", @"D:\dippa\uploadData\mega.txt", @"D:\dippa\uploadData\tenMega.txt" }; // , @"D:\dippa\uploadData\large-1.jpg"

            if (File.Exists(_resultFilePath))
            {
                File.Delete(_resultFilePath);
            }

            await BlobStorageStoreFile(fileNames[0], filePaths[0]);


            var sameFileCount = 3;

            for (var i = 0; i < fileNames.Count; i++)
            {
                var fileBytes = await File.ReadAllBytesAsync(filePaths[i]);
                var fileSize = new FileInfo(filePaths[i]).Length;
                

                for (var j = 0; j < sameFileCount; j++)
                {
                    HandleStoreStart("blobStorage", fileNames[i], fileSize);
                    await BlobStorageStoreFile(fileNames[i], filePaths[i]);
                    HandleStoreFinish();

                    HandleStoreStart("fileStorage", fileNames[i], fileSize);
                    await FileStorageStoreFile(fileBytes, fileNames[i]);
                    HandleStoreFinish();

                    HandleStoreStart("diskStorage", fileNames[i], fileSize);
                    await DiskStorageStoreFile(fileBytes, fileNames[i]);
                    HandleStoreFinish();

                    HandleStoreStart("SQL", fileNames[i], fileSize);
                    await SQLStoreFile(fileBytes, fileNames[i]);
                    HandleStoreFinish();

                    HandleStoreStart("Cosmos", fileNames[i], fileSize);
                    await CosmosStoreFile(fileBytes, fileNames[i]);
                    HandleStoreFinish();
                }
            }

           return File.ReadAllText(_resultFilePath);
        }

        private void HandleStoreStart(string storeType, string storeContent, long fileSize)
        {
            _startTime = DateTime.UtcNow;
            _currentStoretype = storeType;
            _currentStoreContent = storeContent;
            _fileSize = fileSize;
        }

        private void HandleStoreFinish()
        {
            var storeTime = (DateTime.UtcNow - _startTime).TotalSeconds;
            var newCsvRow = _currentStoretype + "," + _currentStoreContent + "," + storeTime.ToString("N4", CultureInfo.InvariantCulture) + "," + _fileSize;

            using var writer = File.AppendText(_resultFilePath);
            writer.WriteLine(newCsvRow);
        }

        private async Task BlobStorageStoreFile(string fileName, string filePath)
        {
            try
            {
                var blobId = fileName;
                var blobCliet = _blobContainerClient.GetBlobClient(blobId);
                await blobCliet.UploadAsync(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        private async Task FileStorageStoreFile(byte[] data, string fileName)
        {
            await File.WriteAllBytesAsync(_fileStorageStorePath + fileName, data);
        }

        private async Task DiskStorageStoreFile(byte[] data, string fileName) 
        {
            await File.WriteAllBytesAsync(_diskStorageStorePath + fileName, data);
        }

        private async Task SQLStoreFileWithLink(int id, string filePath, string fileName)
        {
            using var connection = new MySqlConnection(_SQLConnectionString);
            connection.Open();

            var comm = connection.CreateCommand();
            var query = "INSERT INTO SQLStorageWithLink VALUES(@id, @fileName, @fileInfo, @filePath)";
            comm.CommandText = query;

            comm.Parameters.AddWithValue("@fileName", fileName);
            comm.Parameters.AddWithValue("@filePath", filePath);
            comm.Parameters.AddWithValue("@fileInfo", "random");
            comm.Parameters.AddWithValue("@id", id);
            try
            {
                await comm.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private async Task SQLStoreFile( byte[] data, string fileName, int id = 0)
        {
            using var connection = new MySqlConnection(_SQLConnectionString);
            connection.Open();

            var comm = connection.CreateCommand();
            
            var query = "INSERT INTO SQLStorage VALUES(@id, @fileName, @fileInfo, @fileData)";
            if (id == 0)
            {
                query = "INSERT INTO SQLStorage VALUES(DEFAULT, @fileName, @fileInfo, @fileData)";
            }
            comm.CommandText = query;
            var info = data.Length.ToString();

            comm.Parameters.AddWithValue("@fileName", fileName);
            comm.Parameters.AddWithValue("@fileData", data);
            comm.Parameters.AddWithValue("@fileInfo", info);
            if(id != 0)
            {
                comm.Parameters.AddWithValue("@id", id);
            }
            
            try
            {
                await comm.ExecuteNonQueryAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
            finally
            {
                connection.Close();
            }
        }

        private async Task CosmosStoreFile(byte[] data, string fileName)
        {
            var container = _cosmosClient.GetContainer("file-upload", "piece-upload");
            var random = new Random();
            var randomNum = random.Next(0, 10000);
            var dataObject = new { id = fileName + randomNum.ToString(), FileName = fileName, Data = data };
            try
            {
                await container.CreateItemAsync(dataObject, new PartitionKey(dataObject.FileName));
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        private async Task InitSQLAndFileSystem()
        {
            using var connection = new MySqlConnection(_SQLConnectionString);
            connection.Open();
            var deleteBlobQuery = new MySqlCommand("DELETE FROM SQLStorage WHERE ID > 0", connection);
            var deleteLinkQuery = new MySqlCommand("DELETE FROM SQLStorageWithLink WHERE ID > 0", connection);
            deleteBlobQuery.ExecuteNonQuery();
            deleteLinkQuery.ExecuteNonQuery();

            var smallFilePath = _configuraion.GetValue<string>("SmallFilepath");
            var mediumFilePath = _configuraion.GetValue<string>("MediumFilepath");
            var largeFilePath = _configuraion.GetValue<string>("LargeFilepath");

            var smallFile = await File.ReadAllBytesAsync(smallFilePath);
            var mediumFile = await File.ReadAllBytesAsync(mediumFilePath);
            var largeFile = await File.ReadAllBytesAsync(largeFilePath);


            var storePathRoot = _configuraion.GetValue<string>("SQLFileSystemStoreRoot");
            var random = new Random();

            for (var i = 0; i < _fileCountInFileSystem; i++)
            {
                var smallfileName = storePathRoot + "smallFile" + i.ToString() + ".txt";
                await File.WriteAllBytesAsync(smallfileName, smallFile);

                var mediumfileName = storePathRoot + "smallFile" + i.ToString() + ".txt";
                await File.WriteAllBytesAsync(mediumfileName, mediumFile);
            }

            for (var i = 0; i < _rowCountInSQL; i++)
            {
                var smallfileName = "smallFile" + i.ToString() + ".txt";
                await SQLStoreFile( smallFile, smallfileName, i);

                var randomFileNumber = random.Next(_fileCountInFileSystem);
                var fileName = "smallFile" + randomFileNumber.ToString() + ".txt";
                var fileLink = storePathRoot + fileName;
                await SQLStoreFileWithLink(i, fileLink, fileName);
            }

        }

        private async Task SQLReadBlobFiles()
        {
            var random = new Random();
            var randomId = random.Next(_fileCountInFileSystem);
            using var connection = new MySqlConnection(_SQLConnectionString);
            connection.Open();

            var query = "SELECT fileData FROM SQLStorage WHERE id = " + randomId.ToString();
            var comm = connection.CreateCommand();
            comm.CommandText = query;
            var reader = await comm.ExecuteReaderAsync();
            while(reader.Read())
            {
                // var blob = reader["fileData"];
                var bytes = new byte[1048];
                reader.GetBytes(reader.GetOrdinal("fileData"), 0, bytes, 0, 1048);
                Console.WriteLine(bytes[0]);
            }
        }
        private async Task SQLReadBlobFilesViaLink()
        {
            var random = new Random();
            var randomId = random.Next(1, _fileCountInFileSystem -1);
            using var connection = new MySqlConnection(_SQLConnectionString);
            connection.Open();

            var query = "SELECT filePath FROM SQLStorageWithLink WHERE id = " + randomId.ToString();
            var comm = connection.CreateCommand();
            comm.CommandText = query;
            var reader = await comm.ExecuteReaderAsync();
            while (reader.Read())
            {
                var path = reader["filePath"].ToString();
                var data = await File.ReadAllBytesAsync(path);
                Console.WriteLine(data.Length);
            }
        }

        private async Task SQLReaFileInfo(string tableName)
        {
            var random = new Random();
            var randomId = random.Next(1, _rowCountInSQL -1);
            using var connection = new MySqlConnection(_SQLConnectionString);
            connection.Open();

            var query = "SELECT fileInfo FROM " + tableName + " WHERE id = " + randomId.ToString();
            var comm = connection.CreateCommand();
            comm.CommandText = query;
            var reader = comm.ExecuteReader();
            while (reader.Read())
            {
                var info = reader["fileInfo"];
                Console.WriteLine(info);
            }
        }

        private async Task<string> SQLWriteAndDeleteFiles(string tableName)
        {
            var random = new Random();
           
            

            var smallFilePath = _configuraion.GetValue<string>("SmallFilepath");
            var mediumFilePath = _configuraion.GetValue<string>("MediumFilepath");
            var largeFilePath = _configuraion.GetValue<string>("LargeFilepath");

            var smallFile = await File.ReadAllBytesAsync(smallFilePath);
            var mediumFile = await File.ReadAllBytesAsync(mediumFilePath);
            var largeFile = await File.ReadAllBytesAsync(largeFilePath);


            var storePathRoot = _configuraion.GetValue<string>("SQLFileSystemStoreRoot");

            var randomId = random.Next(_rowCountInSQL* 2, _rowCountInSQL * 3);
            var fileName = "smallFile" + randomId.ToString() + ".txt";
            if(tableName == "SQLStorage")
            {
                await SQLStoreFile(smallFile, fileName, randomId);
            }
            else
            {
                var fileLink = storePathRoot + fileName;
                await SQLStoreFileWithLink(randomId, fileLink, fileName);

                await File.WriteAllBytesAsync(fileLink, smallFile);
            }
           

            using var connection = new MySqlConnection(_SQLConnectionString);
            connection.Open();
            MySqlCommand query = null;
            if(tableName == "SQLStorage")
            {
                query = new MySqlCommand("DELETE FROM SQLStorage WHERE ID = " + randomId.ToString(), connection);
            } else
            {
                query = new MySqlCommand("DELETE FROM SQLStorageWithLink WHERE ID = " + randomId.ToString(), connection);
                File.Delete(storePathRoot + fileName);
            }



            query.ExecuteNonQuery();

            return "";
        }

        public async Task RunStoreLargeFilesInSQLVersusFilesystem()
        {

            //await InitSQLAndFileSystem();
            var resultsFile = "operation,type,time,fileSize\n";
            var fileSize = 1048;

           
            var concurrentReads = 50;
            var blobTasks = new Task[concurrentReads];
            var blobReadStartTime = DateTime.UtcNow;

            for (var i = 0; i < concurrentReads; i++)
            {
                blobTasks[i] = Task.Factory.StartNew(() => SQLReadBlobFiles());
            }

            var blobReadEndTime = DateTime.UtcNow;
            var blobReadTime =  blobReadEndTime - blobReadStartTime;

            Task.WaitAll(blobTasks);
            Console.WriteLine(blobReadTime.TotalSeconds);
            resultsFile += "ReadBlobFIles,blob" + blobReadTime.TotalSeconds.ToString() + "," + fileSize + "\n";

            var linkTasks = new Task[concurrentReads];
            var linkReadStartTime = DateTime.UtcNow;

            for (var i = 0; i < concurrentReads; i++)
            {
                linkTasks[i] = Task.Factory.StartNew(() => SQLReadBlobFilesViaLink());
            }

            Task.WaitAll(linkTasks);
            var linkReadSTime = DateTime.UtcNow - linkReadStartTime;
            Console.WriteLine(linkReadSTime.TotalSeconds);
            resultsFile += "ReadBlobFIles,link" + linkReadSTime.TotalSeconds.ToString() + "," + fileSize + "\n";

            var infoReadTask = new Task[concurrentReads];
            var infoReadTaskStartTime = DateTime.UtcNow;

            for (var i = 0; i < concurrentReads; i++)
            {
                infoReadTask[i] = Task.Factory.StartNew(() => SQLReaFileInfo("SQLStorage"));
            }

            Task.WaitAll(infoReadTask);
            var infoReadTaskTime = DateTime.UtcNow - infoReadTaskStartTime;

            resultsFile += "ReadFileInfos,blob" + infoReadTaskTime.TotalSeconds.ToString() + "," + fileSize + "\n";


            var infoReadWithLinkTask = new Task[concurrentReads];
            var infoReadWithLinkTaskStartTime = DateTime.UtcNow;

            for (var i = 0; i < concurrentReads; i++)
            {
                infoReadWithLinkTask[i] = Task.Factory.StartNew(() => SQLReaFileInfo("SQLStorageWithLink"));
            }

            Task.WaitAll(infoReadWithLinkTask);
            var infoReadWithLinkTaskTime = DateTime.UtcNow - infoReadWithLinkTaskStartTime;

            resultsFile += "ReadFileInfos,link" + infoReadWithLinkTaskTime.TotalSeconds.ToString() + "," + fileSize + "\n";

            Console.WriteLine(resultsFile);

            var ReadAndWriteTask = new Task[concurrentReads];
            var ReadAndWriteTaskStartTime = DateTime.UtcNow;

            for (var i = 0; i < concurrentReads; i++)
            {
                ReadAndWriteTask[i] = Task.Factory.StartNew(() => SQLWriteAndDeleteFiles("SQLStorage"));
            }

            Task.WaitAll(ReadAndWriteTask);
            var ReadAndWriteTaskTime = DateTime.UtcNow - infoReadWithLinkTaskStartTime;

            resultsFile += "WriteAndDelete,blob" + ReadAndWriteTaskTime.TotalSeconds.ToString() + "," + fileSize + "\n";


            var ReadAndWriteLinkTask = new Task[concurrentReads];
            var ReadAndWriteLinkTaskStartTime = DateTime.UtcNow;

            for (var i = 0; i < concurrentReads; i++)
            {
                ReadAndWriteLinkTask[i] = Task.Factory.StartNew(() => SQLWriteAndDeleteFiles("SQLStorageWithLink"));
            }

            Task.WaitAll(ReadAndWriteLinkTask);
            var ReadAndWriteLinkTime = DateTime.UtcNow - infoReadWithLinkTaskStartTime;

            resultsFile += "WriteAndDelete,link" + ReadAndWriteTaskTime.TotalSeconds.ToString() + "," + fileSize + "\n";

            Console.WriteLine(resultsFile);





        }
    }
}
