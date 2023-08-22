using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;

namespace BrowserBackEnd.Hubs
{
    public struct WebSocketActions
    {
        public static readonly string MESSAGE_RECEIVED = "messageReceived";
        public static readonly string USER_LEFT = "userLeft";
        public static readonly string USER_JOINED = "userJoined";
    }

    public class WebSocketUploadHub : Hub
    {
        public async Task Send()
        {
            await Clients.All.SendAsync(WebSocketActions.MESSAGE_RECEIVED,  "dummy");
        }

        public async Task Receive(string data)
        {
            Console.WriteLine(data);
        }

        public void UploadFilePiece(string data)
        {
            Console.WriteLine(data.Length);
        }

        public void UploadFilePieceArray(List<uint> data)
        {
            Console.WriteLine(data.Count);
        }
    }
}
