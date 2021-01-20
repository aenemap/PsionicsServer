using System;
using System.Net;
using System.Net.Sockets;
using System.Numerics;
using System.Linq;

namespace PsionicsServer
{
    public class Client
    {
        public static int dataBufferSize = 4096;
        public int id;
        public Player player;
        public TCP tcp;
        public UDP udp;



        public Client(int clientId)
        {
            id = clientId;
            tcp = new TCP(id);
            udp = new UDP(id);
        }

        public class TCP
        {
            public TcpClient socket;
            private readonly int id;
            private NetworkStream stream;
            private byte[] receiveBuffer;


            public TCP(int _id)
            {
                id = _id;
            }

            public void Connect(TcpClient _socket)
            {
                socket = _socket;
                socket.ReceiveBufferSize = dataBufferSize;
                socket.SendBufferSize = dataBufferSize;

                stream = socket.GetStream();
                receiveBuffer = new byte[dataBufferSize];

                stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);

                ServerSend.Welcome(id, "Welcome to the server!");
            }

            public void SendDataProto(byte[] data, int size)
            {
                try
                {
                    if (socket != null)
                    {
                        stream.BeginWrite(data, 0, size, null, null);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error sending data to player {id} via TCP:{ex}");
                }
            }

            private void ReceiveCallback(IAsyncResult _result)
            {
                try
                {
                    int _byteLength = stream.EndRead(_result);
                    if (_byteLength <= 0)
                    {
                        Server.clients[id].Disconnect();
                        // Server.clients.Remove(id);
                        // string gameId = Server.activeGames.Where(w => w.Value.CreatedById == id).Select(s => s.Value.GameId).FirstOrDefault();
                        // Server.activeGames.Remove(gameId);
                        return;
                    }
                    byte[] _data = new byte[_byteLength];
                    Array.Copy(receiveBuffer, _data, _byteLength);

                    ThreadManager.ExecuteOnMainThread(() => {
                        ServerHandle.HandleClientRequest(_data, id);
                    });

                    stream.BeginRead(receiveBuffer, 0, dataBufferSize, ReceiveCallback, null);


                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error receiveing TCP data: {ex}");
                    Server.clients[id].Disconnect();
                    // Server.clients.Remove(id);
                    // string gameId = Server.activeGames.Where(w => w.Value.CreatedById == id).Select(s => s.Value.GameId).FirstOrDefault();
                    // Server.activeGames.Remove(gameId);
                }
            }
            public void Disconnect()
            {
                socket.Close();
                stream = null;
                socket = null;
            }
        }

        public class UDP
        {
            public IPEndPoint endPoint;
            private int id;
            public UDP(int _id)
            {
                id = _id;
            }
            public void Connect(IPEndPoint _endpoint)
            {
                endPoint = _endpoint;
                //ServerSend.UDPTest(id);
            }

            public void SendData(byte[] data, int size)
            {
                Server.SendUDPData(endPoint, data, size);
            }

            public void Disconnect()
            {
                endPoint = null;
            }
        }

        public void SendIntoGame(string _playerName)
        {
            player = new Player(id, _playerName, new Vector3(0, 0, 0));

            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (Server.clients[i].player != null)
                    {
                        if (Server.clients[i].id != id)
                        {
                            ServerSend.SpawnPlayer(id, Server.clients[i].player);
                        }
                    }
                }
            }

            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (Server.clients[i].player != null)
                    {
                        ServerSend.SpawnPlayer(Server.clients[i].id, player);
                    }
                }
            }
        }

        public void SendIntoLobby(string username){
            player = new Player(id, username, new Vector3(0, 0, 0));

            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (Server.clients[i].player != null)
                    {
                        if (Server.clients[i].id != id)
                        {
                            Console.WriteLine($"First iteration username: {Server.clients[i].player.username}");
                            ServerSend.SendPlayerToLobby(
                                id,
                                "Lobby",
                                Server.clients[i].player
                            );
                        }
                    }
                }
            }

            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (Server.clients[i].player != null)
                    {
                        Console.WriteLine($"Second iteration username: {Server.clients[i].player.username}");
                        ServerSend.SendPlayerToLobby(
                            Server.clients[i].id,
                            "Lobby",
                            player
                        );
                    }
                }
            }   
        }

        private void Disconnect()
        {
            Console.WriteLine($"{tcp.socket.Client.RemoteEndPoint} has disconnected");
            tcp.Disconnect();
            udp.Disconnect();
            Server.clients.Remove(id);
            string gameId = Server.activeGames.Where(w => w.Value.CreatedById == id).Select(s => s.Value.GameId).FirstOrDefault();
            if (gameId != null){
                Server.activeGames.Remove(gameId);
            }
        }
    }
}
