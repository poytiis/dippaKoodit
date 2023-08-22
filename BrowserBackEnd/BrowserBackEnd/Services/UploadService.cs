using BrowserBackEnd.HTTPValidation;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Azure.Storage.Blobs;
using MySqlConnector;

namespace BrowserBackEnd.Services
{
    public interface IUploadService
    {
        void StartUpload(StartUploadBody startUpload);
        Task UploadFilePieceArray(UploadFilePieceArrayBody filePiece);
        Task UploadFilePieceForm(UploadFilePieceForm filePiece);
        Task UploadFilePieceBase64(UploadFileBase64Body filePiece);
        void FinishUpload(FinishUploadBody finishUpload);
        void StoreBinarydata(byte[] data);
        Task UploadFilePieceByteArray(UploadFilePieceByteArrayBody filePiece);
    }

    public class UploadService : IUploadService
    {
        private readonly string _rootUploadPath;


        public UploadService(IConfiguration configuraion)
        {
            _rootUploadPath = configuraion.GetValue<string>("UploadFolderPath");
        }

        public void StoreBinarydata(byte[] data)
        {
            var randomGuid = Guid.NewGuid().ToString();
            var currentDate = DateTime.Now.Millisecond.ToString();
            var uploadPath = _rootUploadPath + "binary" + "/" + randomGuid + "__" + currentDate;

            File.WriteAllBytes(uploadPath, data);
        }

        public async Task UploadFilePieceForm(UploadFilePieceForm filePiece)
        {
           if(filePiece.PieceData.Length > 0)
           {
                var uploadPath = _rootUploadPath + filePiece.FileName + "/" + filePiece.FileName + "__" + filePiece.PieceNumber.ToString();
                using var fileStream = new FileStream(uploadPath, FileMode.Create);

                await filePiece.PieceData.CopyToAsync(fileStream);           
           }
        }
     
        public void StartUpload(StartUploadBody startUpload)
        {
            var uploadPath = _rootUploadPath + startUpload.FileName;
            Directory.CreateDirectory(uploadPath);
            var allFiles = Directory.GetFiles(uploadPath + "/");

            foreach (var fileName in allFiles)
            {
                File.Delete(fileName);
            }
        }

        public async Task UploadFilePieceArray(UploadFilePieceArrayBody filePiece)
        {
            var uploadPath = _rootUploadPath + filePiece.FileName + "/" + filePiece.FileName + "__" + filePiece.PieceNumber.ToString();

            var fileDataAsBytes = filePiece.PieceData.Select(i => (byte)i).ToArray();
            await File.WriteAllBytesAsync(uploadPath, fileDataAsBytes);
        }

        public async Task UploadFilePieceByteArray(UploadFilePieceByteArrayBody filePiece)
        {
            var uploadPath = _rootUploadPath + filePiece.FileName + "/" + filePiece.FileName + "__" + filePiece.PieceNumber.ToString();
            await File.WriteAllBytesAsync(uploadPath, filePiece.PieceData);
        }

        public async Task UploadFilePieceBase64(UploadFileBase64Body filePiece)
        {
            var uploadPath = _rootUploadPath + filePiece.FileName + "/" + filePiece.FileName + "__" + filePiece.PieceNumber.ToString();
            var dataAsBytes = Convert.FromBase64String(filePiece.PieceData);

            await File.WriteAllBytesAsync(uploadPath, dataAsBytes);
        }

        public void FinishUpload(FinishUploadBody finishUpload)
        {
            var uploadFolder = _rootUploadPath + finishUpload.FileName + "/";
            var allFiles = Directory.GetFiles(uploadFolder);
            var allBytes = Array.Empty<byte>();
            var fileCount = allFiles.Count();

            for(var i = 1; i <= fileCount; i++)
            {
                var fileName = uploadFolder + finishUpload.FileName + "__"  + i.ToString();
                Console.WriteLine(fileName);
                var fileBytes = File.ReadAllBytes(fileName);

                byte[] ret = new byte[allBytes.Length + fileBytes.Length];
                Buffer.BlockCopy(allBytes, 0, ret, 0, allBytes.Length);
                Buffer.BlockCopy(fileBytes, 0, ret, allBytes.Length, fileBytes.Length);

                allBytes = ret;
            }

            var uploadPath = uploadFolder + finishUpload.FileName;
            File.WriteAllBytes(uploadPath, allBytes);

            foreach(var fileName in allFiles)
            {
                File.Delete(fileName);
            }

        }
    }
}
