using Microsoft.AspNetCore.Http;
using System;
using System.Text.Json;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using BrowserBackEnd.Services;
using Microsoft.Extensions.Configuration;
using BrowserBackEnd.HTTPValidation;
using System.IO;

namespace BrowserBackEnd.Midelwares
{
    class WebSocketMessage
    {
        public string FileName { get; set; }
        public uint[] PieceData { get; set; }
        public int PieceNumber { get; set; }
        public string MessageType { get; set; }
        public int BinaryBlockSize { get; set; }
    }

    public class WebSocketMiddleware
    {
        private readonly RequestDelegate _next;
        public int BinaryBlockSize { get; set; }
        public WebSocketMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            if (context.WebSockets.IsWebSocketRequest)
            {             
                var socket = await context.WebSockets.AcceptWebSocketAsync();
                try
                {
                    await HandleWebSocket(socket);
                } 
                catch(Exception ex)
                {
                    Console.WriteLine(ex.Message);
                }              
            }
            else
            {
                await _next(context);
            }

        }
        public static void Dummy(byte[] data)
        {
            var randomGuid = Guid.NewGuid().ToString();
            var currentDate = DateTime.Now.Millisecond.ToString();
            var _rootUploadPath = @"D:\dippa\upload";
            var uploadPath = _rootUploadPath + "binary" + "/" + randomGuid + "__" + currentDate;

            //File.WriteAllBytes(uploadPath, data);
            Console.WriteLine("Dummuy");
        }

        public async Task HandleWebSocket(WebSocket webSocket)
        {
            var configuration = new ConfigurationBuilder()
                    .AddJsonFile("appsettings.json")
                    .AddJsonFile("appsettings.Secrets.json")
                    .Build();

            var uploadService = new UploadService(configuration);

            var buffer = new byte[700000];
            var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

            while (!result.CloseStatus.HasValue)
            {
                

                if (result.MessageType == WebSocketMessageType.Text)
                {
                    var messageStr = Encoding.UTF8.GetString(buffer).TrimEnd('\0');
                    Console.WriteLine(messageStr);

                    WebSocketMessage message;
                    try
                    {
                        message = JsonSerializer.Deserialize<WebSocketMessage>(messageStr);
                    }
                    catch(Exception ex)
                    {
                        Console.WriteLine(ex);
                        Console.WriteLine(messageStr);
                        throw;
                    }
                    

                    if (message.MessageType == "startUpload")
                    {
                        BinaryBlockSize = message.BinaryBlockSize;
                        var body = new StartUploadBody
                        {
                            FileName = message.FileName
                        };                      
                        uploadService.StartUpload(body);
                    }
                    else if (message.MessageType == "uploadPiece")
                    {
                        var body = new UploadFilePieceArrayBody
                        {
                            FileName = message.FileName,
                            PieceData = message.PieceData,
                            PieceNumber = message.PieceNumber
                        };
                        await uploadService.UploadFilePieceArray(body);
                    }
                    else if (message.MessageType == "finishUpload")
                    {
                        var body = new FinishUploadBody
                        {
                            FileName = message.FileName
                        };
                        uploadService.FinishUpload(body);

                    }
                    else
                    {
                        throw new Exception("Invalid message type");
                    }
                }
                else
                {
                    var binaryData = buffer.Take(BinaryBlockSize).ToArray();

                    //uploadService.StoreBinarydata(binaryData);
                    Dummy(binaryData);
                }

                Array.Clear(buffer, 0, buffer.Length);

                //var dummyAnswer = Encoding.UTF8.GetBytes("{\"dummy\":1}");
                //await webSocket.SendAsync(new ArraySegment<byte>(dummyAnswer), result.MessageType, true, CancellationToken.None);
              
                result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            await webSocket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

    }
}
