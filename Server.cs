using PsionicsCardGameProto;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PsionicsServer
{
    public class Server
    {

        public static int MaxPlayers { get; private set; }
        public static int Port { get; private set; }
        public static Dictionary<int, Client> clients = new Dictionary<int, Client>();
        public static Dictionary<string, Game> activeGames = new Dictionary<string, Game>();
        private static TcpListener tcpListener;
        private static UdpClient udpListener;

        public static void Start(int _maxPlayers, int _port)
        {
            MaxPlayers = _maxPlayers;
            Port = _port;

            Console.WriteLine("Starting Server ...");
            // InitializeServerData();

            tcpListener = new TcpListener(IPAddress.Any, Port);
            tcpListener.Start();
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            udpListener = new UdpClient(Port);
            udpListener.BeginReceive(UDPReceiveCallback, null);

            Console.WriteLine($"Server started on port {Port}");
        }

        private static void TCPConnectCallback(IAsyncResult _result)
        {
            TcpClient _client = tcpListener.EndAcceptTcpClient(_result);
            tcpListener.BeginAcceptTcpClient(new AsyncCallback(TCPConnectCallback), null);

            Console.WriteLine($"Incoming connection from {_client.Client.RemoteEndPoint}...");
            int clientId = clients.Count + 1;
            if (clients.Count < MaxPlayers)
            {
                clients.Add(clientId, new Client(clientId));
                // for (int i = 1; i <= clients.Count; i++)
                // {
                //     Console.WriteLine(clients[i].id);
                // }
                clients[clientId].tcp.Connect(_client);
                return;
            }
            Console.WriteLine($"{_client.Client.RemoteEndPoint} failed to connect: Server full!");
        }

        private static void UDPReceiveCallback(IAsyncResult _result)
        {
            try
            {
                IPEndPoint _clientEndPoint = new IPEndPoint(IPAddress.Any, 0);
                byte[] _data = udpListener.EndReceive(_result, ref _clientEndPoint);
                udpListener.BeginReceive(UDPReceiveCallback, null);

                if (_data.Length < 4)
                {
                    return;

                }

                ThreadManager.ExecuteOnMainThread(() => {
                    ClientRequest clientRequest = ClientRequest.Parser.ParseFrom(_data);
                    switch (clientRequest.RequestCase)
                    {
                        case ClientRequest.RequestOneofCase.UdpWelcome:
                            int _clientId = clientRequest.UdpWelcome.ClientId;
                            if (_clientId == 0)
                            {
                                return;
                            }
                            if (clients[_clientId].udp.endPoint == null)
                            {
                                clients[_clientId].udp.Connect(_clientEndPoint);
                                return;
                            }
                            break;
                        case ClientRequest.RequestOneofCase.PlayerMovement:
                            int clientId = clientRequest.PlayerMovement.ClientId;
                            if (clientId == 0)
                            {
                                return;
                            }
                            ServerHandle.PlayerMovement(clientRequest);
                            break;
                        default:
                            break;
                    }

                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error receiveng UDP data: {ex}");
            }
        }

        public static void SendUDPData(IPEndPoint _clientEndPoint, byte[] data, int size)
        {
            try
            {
                if (_clientEndPoint != null)
                {
                    udpListener.BeginSend(data, size, _clientEndPoint, null, null);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending data to {_clientEndPoint} via UDP: {ex}");
            }
        }

        private static void InitializeServerData()
        {
            for (int i = 1; i <= MaxPlayers; i++)
            {
                clients.Add(i, new Client(i));
            }

            Console.WriteLine("Initialized packets");
        }
    }
}
