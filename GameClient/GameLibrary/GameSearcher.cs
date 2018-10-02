using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace GameLibrary
{
    public class GameSearcher
    {
        public delegate void GameRequestHandler(object sender, GameInstance instance);
        public event GameRequestHandler OnGameRequest;

        public delegate void GameAvailableHandler(object sender, AdvertiseGame ClientInfo);
        public event GameAvailableHandler OnGameFound;

        public delegate void ExceptionThrownHandler(object sender, Exception ex);
        public event ExceptionThrownHandler OnException;
        
        public Client ClientInfo { get; private set; }

        private ConcurrentDictionary<IPEndPoint, AdvertiseGame> _AvailableGames = new ConcurrentDictionary<IPEndPoint, AdvertiseGame>();

        public List<Client> AvailableGames
        {
            get
            {
                return _AvailableGames.Select(x=>x.Value.Body).ToList();
            }
        }

        private bool RunBroadcaster;
        private bool RunListener;
        private bool RunMatchingRequests;

        public GameSearcher(String Username)
        {
            ClientInfo = new Client(Username, Guid.NewGuid());
        }

        ~GameSearcher()
        {
            StopSearching();
        }

        public void SetUsername(String Name)
        {
            ClientInfo.Username = Name;
        }

        public void StartSearching()
        {
            RunBroadcaster = true;
            RunListener = true;
            RunMatchingRequests = true;

            ThreadPool.QueueUserWorkItem(ListenForClientInformation);
            ThreadPool.QueueUserWorkItem(BroadcastClientInformation);
            ThreadPool.QueueUserWorkItem(ListenForMatchingMakingRequests);

        }

        public void StopSearching()
        {
            RunBroadcaster = false;
            RunListener = false;
            RunMatchingRequests = false;

            _AvailableGames.Clear();
        }


        private IPAddress GetBroadcastAddress()
        {

            return IPAddress.Parse("230.0.0.1");

            //var myAddressBroadcast = Dns.GetHostEntryAsync((Dns.GetHostName()))
            //                                    .Result
            //                                    .AddressList
            //                                    .Where(x => x.AddressFamily == AddressFamily.InterNetwork)
            //                                    .Select(x => x)
            //                                    .ToArray().FirstOrDefault();

            //if (myAddressBroadcast != null)
            //{
            //    var oct = myAddressBroadcast.GetAddressBytes();
            //    oct[3] = 255;

            //    var IpString = String.Join(".", oct);

            //    return IPAddress.Parse(IpString);
            //}
            //else
            //{
            //    return IPAddress.Parse("192.168.1.255");
            //}


        }

        private void ListenForClientInformation(object state)
        {
            UdpClient subscriber = new UdpClient
            {
                ExclusiveAddressUse = false
            };
            subscriber.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            subscriber.Client.Bind(new IPEndPoint(IPAddress.Any, GamePort.GameSearching));
            try
            {

                subscriber.JoinMulticastGroup(GetBroadcastAddress());

                while (RunListener)
                {
                    IPEndPoint ep = null;
                    byte[] pdata = subscriber.Receive(ref ep);

                    Task.Run(()=> 
                    {
                        ProcessMatchingSearchResult(pdata, ep);
                    });
                }

            } catch (Exception e)
            {
                OnException?.Invoke(this, e);
            }
            finally
            {
                subscriber.Close();
            }
        }

        private void ProcessMatchingSearchResult(byte[] Data,IPEndPoint RemoteHost)
        {
            try
            {
                var json = Encoding.UTF8.GetString(Data);
                var gameAdvertisement = JsonConvert.DeserializeObject<AdvertiseGame>(json);
                gameAdvertisement.RemoteHost = RemoteHost;
                var clientInfo = gameAdvertisement.Body;

                if (clientInfo.UniqueMatchMakingIdentifer != ClientInfo.UniqueMatchMakingIdentifer)
                {
                    var updatedClient = _AvailableGames.AddOrUpdate(RemoteHost, gameAdvertisement, (ID, Info) => Info = gameAdvertisement);

                    OnGameFound?.Invoke(this, gameAdvertisement);
                }
            }
            catch (Exception e)
            {
                OnException?.Invoke(this, e);
            }
        }

        private void BroadcastClientInformation(object state)
        {
            UdpClient publisher = new UdpClient(GetBroadcastAddress().ToString(), GamePort.GameSearching);
            try
            {
                while (RunBroadcaster)
                {
                    var json = JsonConvert.SerializeObject(new AdvertiseGame() { Body = this.ClientInfo });
                    var binaryJson = Encoding.UTF8.GetBytes(json);
                    publisher.Send(binaryJson, binaryJson.Length);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }

            }
            catch (Exception e)
            {
                OnException?.Invoke(this, e);
            }
            finally
            {
                publisher.Close();
            }
        }

        private void ListenForMatchingMakingRequests(object state)
        {
            UdpClient listener = new UdpClient() { ExclusiveAddressUse = false };
            while (RunMatchingRequests)
            {
                try
                {

                    listener.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                    listener.Client.Bind(new IPEndPoint(IPAddress.Any, GamePort.RequestMatch));

                    IPEndPoint remoteEndpoint = new IPEndPoint(IPAddress.Any, 0);

                    byte[] binaryJson = listener.Receive(ref remoteEndpoint);
                    
                    var clientInfo = JsonConvert.DeserializeObject<AdvertiseGame>(Encoding.UTF8.GetString(binaryJson));

                    if (ClientInfo.UniqueMatchMakingIdentifer != clientInfo.Body.UniqueMatchMakingIdentifer)
                    {// xxx.xxx.xxx.:1231321231 has requested a game with you
                     // You actually have a game request and not just requesting yourself!

                        // Establish a Tcp Connection and let the game begin!
                        var client = new TcpClient();
                        try
                        {
                            client.Connect(remoteEndpoint.Address, GamePort.Game);
                            OnGameRequest?.Invoke(this, new GameInstance(client, ClientInfo, false, clientInfo));
                        }
                        catch (Exception ex)
                        {
                            throw;
                        }
                    }
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(this, ex);
                }
                finally
                {
                    listener.Close();
                }

            }
        }
        
        public void RequestGame(Guid MatchingID, WaitCallback callback)
        {
            Task.Run(()=> 
            {
                // Setup a TcpClient
                try
                {
                    var game = _AvailableGames.Where(x => x.Value.Body.UniqueMatchMakingIdentifer == MatchingID).First();

                    Socket gameRequestor = new Socket(AddressFamily.InterNetwork, SocketType.Dgram,
                    ProtocolType.Udp);

                    var json = JsonConvert.SerializeObject(new AdvertiseGame() { Body = ClientInfo });
                    var binaryJson = Encoding.UTF8.GetBytes(json);
                    var endPoint = new IPEndPoint(game.Value.RemoteHost.Address, GamePort.RequestMatch);

                    gameRequestor.SendTo(binaryJson, endPoint);

                    // Listen for a TcpConnection from this client
                    //
                    // If success return callback with all the information needed to continue the game!
                    //callback.Invoke(null);
                    
                    SetupGameListener(callback);
                }
                catch (Exception ex)
                {
                    OnException?.Invoke(this, ex);
                }
            });
        }

        private void SetupGameListener(WaitCallback callback)
        {
            try
            {
                TcpListener ServerListener = new TcpListener(IPAddress.Any, GamePort.Game)
                {
                    ExclusiveAddressUse = false
                };

                ServerListener.Server.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                ServerListener.Start();
                var remoteClient = ServerListener.AcceptTcpClient();
                callback.Invoke(new GameInstance(remoteClient, ClientInfo, true));
            }
            catch(Exception ex)
            {

            }
        }
    }

    public static class GamePort
    {
        public const int GameSearching = 8899;
        public const int RequestMatch = 8900;
        public const int Game = 8901;
    }
}
