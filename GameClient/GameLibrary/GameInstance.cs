using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace GameLibrary
{
    public class GameInstance : IDisposable
    {
        private readonly Socket UnderlyingConnection;
        private readonly TcpClient Client;
        private NetworkStream NStream;
        private StreamReader Reader;
        private StreamWriter Writer;

        public readonly Client LocalClient;
        public readonly AdvertiseGame RemoteClient;

        public readonly bool IsHost;

        public GameInstance(TcpClient Client, Client LocalClient, bool Owner, AdvertiseGame RemoteClient = null)
        {
            this.LocalClient = LocalClient;

            this.RemoteClient = RemoteClient;

            this.IsHost = Owner;

            this.Client = Client;
            UnderlyingConnection = Client.Client;

            NStream = Client.GetStream();
            Reader = new StreamReader(NStream);
            Writer = new StreamWriter(NStream) { AutoFlush = true };
        }

        // Needs to check if the IsHost prop is true
        // Then will get the remote user information
        // Setup who has what symbol
        // Create the game board
        // Decide the first move
        // Send signal for ready state
        private void SetupGameState()
        {
            if (IsHost)
            {
                Task.Run(()=> 
                {
                    // Go off and setup everything!
                });
            }
        }


        public void EnterWaitingState()
        {
            if (IsHost)
            {
                SetupGameState();
            }
            else
            {
                // Inform the host I am ready!
            }
        }

        public void Dispose()
        {
            if (NStream != null)
            {
                NStream.Dispose();
            }
            if (Reader != null)
            {
                Reader.Dispose();
            }

            if (Writer != null)
            {
                Writer.Dispose();
            }
        }
    }
}
