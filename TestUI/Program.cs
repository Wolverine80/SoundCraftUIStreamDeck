using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UIsocket;
using SoundCraftUIStreamDeck;

namespace TestUI
{
    class Program
    {
        const string mixer = "ws://192.168.1.8/socket.io/1/websocket";
        static void Main(string[] args)
        {
            Console.WriteLine("start instance.");
            if (!ConMan.Instance.IsConnected)
            {
                Console.WriteLine("Error instance.");
                Console.ReadLine();
                return;
            }
            
            ConMan.client.GetMessages();
            Console.ReadLine();
        }
    }
}
