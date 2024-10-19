using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using static System.Runtime.InteropServices.JavaScript.JSType;

var domain = "https://localhost:5001/";
domain = "https://dippa.test:5001/";


async Task UploadFile(HttpClient client, string filePath, string fileName, string method, int readingBlockSize = 1048576)
{

    using StringContent jsonContent = new(
       JsonSerializer.Serialize(new
       {
           FileName = fileName
       }),
       Encoding.UTF8,
       "application/json");

    var url = domain + "api/HTTP/StartUpload";

    var resp = await client.PostAsync(url, jsonContent);
    string body = await resp.Content.ReadAsStringAsync();

    byte[] buffer = new byte[readingBlockSize];
    int bytesRead;
    int noOfFiles = 0;
    using (FileStream fs = File.Open(filePath, FileMode.Open, FileAccess.Read))
    using (BufferedStream bs = new(fs))
    {
        while ((bytesRead = bs.Read(buffer, 0, readingBlockSize)) != 0)
        {           
            if (method == "base64")
            {
                var content = Convert.ToBase64String(buffer);

                using var postContent = new StringContent(
                   JsonSerializer.Serialize(new
                   {
                       PieceData = content,
                       FileName = fileName,
                       PieceNumber = noOfFiles
                   }),
                   Encoding.UTF8,
                   "application/json");

                var postRes = await client.PostAsync(domain + "api/HTTP/UploadFilePieceBase64", postContent);

            }
            else if (method == "uintArray")
            {
                var uintArray = BitConverter.ToUInt32(buffer, 0);

                var samples = new uint[buffer.Length];
                Buffer.BlockCopy(buffer, 0, samples, 0, buffer.Length);

                using var postContent = new StringContent(
                   JsonSerializer.Serialize(new
                   {
                       FileName = fileName,
                       PieceNumber = noOfFiles,
                       PieceData = samples

                   }),
                   Encoding.UTF8,
                   "application/json");

                var postRes = await client.PostAsync(domain + "api/HTTP/UploadFilePieceArray", postContent);
            }

            else if (method == "byteArray")
            {
                using var postContent = new StringContent(
                  JsonSerializer.Serialize(new
                  {
                      FileName = fileName,
                      PieceNumber = noOfFiles,
                      PieceData = buffer

                  }),
                  Encoding.UTF8,
                  "application/json");

                var postRes = await client.PostAsync(domain + "api/HTTP/UploadFilePieceByteArray", postContent);
            }

            else if (method == "formData")
            {
                var multipartContent = new MultipartFormDataContent();
                var byteArrayContent = new ByteArrayContent(buffer);
                var FileNameContent = new StringContent(fileName);
                var PieceNumberContent = new StringContent(noOfFiles.ToString());
                multipartContent.Add(byteArrayContent, "PieceData", "filename");
                multipartContent.Add(FileNameContent, "FileName");
                multipartContent.Add(PieceNumberContent, "PieceNumber");
                var postResponse = await client.PostAsync(domain + "api/HTTP/UploadFilePieceForm", multipartContent);
            }

            noOfFiles++;
        }

        using var finishContent = new StringContent(
                  JsonSerializer.Serialize(new
                  {
                      FileName = fileName
                  }),
                  Encoding.UTF8,
                  "application/json");
        await client.PostAsync(domain + "api/HTTP/FinishUpload", finishContent);
    }


}


var fileName = "dummy10M.txt";
var filePath = "C:\\Users\\OWNER\\dippa\\uploadData\\" + fileName;
var fileSize = new FileInfo(filePath).Length;
var resultFilePath = "C:\\Users\\OWNER\\dippa\\upload\\HTTPVersions.csv";

var clients = new List<HttpClient> {
  
    new HttpClient()
    {
        DefaultRequestVersion = HttpVersion.Version11,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
    },
      new HttpClient()
    {
        DefaultRequestVersion = HttpVersion.Version30,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
    },
    new HttpClient()
    {
        DefaultRequestVersion = HttpVersion.Version20,
        DefaultVersionPolicy = HttpVersionPolicy.RequestVersionExact
    }

};

var blockSize = 10485760;
var csvData = "blockSize,bodyForm,HTTPVersion,time,fileSize\n";

var bodyForms = new List<string> { "formData", "base64" , "byteArray" };

bodyForms = new List<string> { "formData" };

foreach (var httpClient in clients)
{
    foreach (var form in bodyForms)
    {
        var startTime = DateTime.Now;
        await UploadFile(httpClient, filePath, fileName, form, blockSize);
        var endtTime = DateTime.Now;
        var timeDiff = endtTime - startTime;
        csvData += blockSize.ToString() + "," + form + "," + httpClient.DefaultRequestVersion.ToString() + "," + timeDiff.ToString() + ","+ fileSize.ToString() + "\n";
    }
}

File.WriteAllText(resultFilePath, csvData);

foreach (var httpClient in clients)
{
    httpClient.Dispose();
}






