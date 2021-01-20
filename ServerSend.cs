using Google.Protobuf;
using PsionicsCardGameProto;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PsionicsServer
{
    public class ServerSend
    {
        private static void SendTCPDataProto(int _toClient, byte[] data, int size)
        {
            Server.clients[_toClient].tcp.SendDataProto(data, size);
        }

        private static void SendTCPDataFromList(List<Client> toClients, byte[] data, int size){
            foreach(Client client in toClients){
                Server.clients[client.id].tcp.SendDataProto(data, size);
            }
        }

        private static void SendUDPData(int _toClient, byte[] data, int size)
        {
            Server.clients[_toClient].udp.SendData(data, size);
        }

        private static void SendTCPDataToAll(byte[] data, int size)
        {
            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (Server.clients.ContainsKey(i))
                    {
                        Server.clients[i].tcp.SendDataProto(data, size);
                    }
                }
            }
        }

        private static void SendTCPDataToAll(int _exceptClient, byte[] data, int size)
        {
            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (i != _exceptClient)
                    {
                        Server.clients[i].tcp.SendDataProto(data, size);
                    }
                }

            }
        }

        private static void SendUDPDataToAll(byte[] data, int size)
        {
            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (Server.clients.ContainsKey(i))
                    {
                        Server.clients[i].udp.SendData(data, size);
                    }
                }
            }
        }

        private static void SendUDPDataToAll(int _exceptClient, byte[] data, int size)
        {
            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (i != _exceptClient)
                    {
                        Server.clients[i].udp.SendData(data, size);
                    }
                }
            }
        }

        #region Packets
        public static void Welcome(int _toClient, string _msg)
        {
            Welcome welcome = new Welcome
            {
                Id = _toClient,
                Message = _msg
            };
            ServerResponse serverResponse = new ServerResponse
            {
                Welcome = welcome
            };
            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);
            SendTCPDataProto(_toClient, data, byteLength);
        }

        public static void SpawnPlayer(int _toClient, Player _player)
        {
            ServerPlayer player = new ServerPlayer
            {
                Id = _player.id,
                Username = _player.username,
                PlayerPosition = new PlayerPosition
                {
                    X = _player.position.X,
                    Y = _player.position.Y,
                    Z = _player.position.Z,

                },
                PlayerRotation = new PlayerRotation
                {
                    X = _player.rotation.X,
                    Y = _player.rotation.Y,
                    Z = _player.rotation.Z,
                    W = _player.rotation.W,
                }
            };

            ServerResponse serverResponse = new ServerResponse
            {
                Player = player
            };

            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);
            SendTCPDataProto(_toClient, data, byteLength);
        }

        public static void PlayerPosition(Player _player)
        {
            PlayerPosition playerPosition = new PlayerPosition
            {
                ClientId = _player.id,
                X = _player.position.X,
                Y = _player.position.Y,
                Z = _player.position.Z,
            };

            ServerResponse serverResponse = new ServerResponse
            {
                PlayerPosition = playerPosition
            };

            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);
            SendUDPDataToAll(data, byteLength);
        }

        public static void PlayerRotation(Player _player)
        {
            PlayerRotation playerRotation = new PlayerRotation
            {
                ClientId = _player.id,
                X = _player.rotation.X,
                Y = _player.rotation.Y,
                Z = _player.rotation.Z,
                W = _player.rotation.W,
            };

            ServerResponse serverResponse = new ServerResponse
            {
                PlayerRotation = playerRotation
            };

            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);
            SendUDPDataToAll(data, byteLength);
        }

        public static void ChangeSceneToClient(int clientId, string sceneName)
        {
            ChangeScene changeScene = new ChangeScene
            {
                ClientId = clientId,
                SceneName = sceneName
            };

            ServerResponse serverResponse = new ServerResponse
            {
                ChangeScene = changeScene
            };
            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);
            SendTCPDataProto(clientId, data, byteLength);
        }

        public static void SendPlayerToLobby(int clientId, string sceneName, Player player){
            PlayerToLobby playerToLobby = new PlayerToLobby{
                ClientId = clientId,
                SceneName = sceneName,
                Player = new LobbyPlayer{
                    Id = player.id,
                    Username = player.username
                }
            };

            foreach(Game game in Server.activeGames.Values){
                playerToLobby.ActiveGames.Add(new GameMessage{
                    GameId = game.GameId,
                    GameName = game.GameName,
                    CreatedById = game.CreatedById,
                    CreatedByUsername = game.CreatedByUsername,
                    GameStatus = game.GameStatus
                });
            }

            ServerResponse serverResponse = new ServerResponse{
                PlayerToLobby = playerToLobby
            };

            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);
            SendTCPDataProto(clientId, data, byteLength);
        }

        public static void SendChatMessage(int clientId, ChatMessage chmessage){
            ChatMessage chatMessage = new ChatMessage{
                ClientId = chmessage.ClientId,
                Message = chmessage.Message,
                Username = chmessage.Username
            };

            ServerResponse serverResponse = new ServerResponse{
                ChatMessage = chatMessage
            };

            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);
            SendTCPDataProto(clientId, data, byteLength);
        }

        public static void SendGameCreated(int clientId, string gameId){

            GameCreated gameCreated = new GameCreated{
                GameId = gameId
            };

            foreach(Game game in Server.activeGames.Values){
                gameCreated.ActiveGames.Add(new GameMessage{
                    GameId = game.GameId,
                    GameName = game.GameName,
                    CreatedById = game.CreatedById,
                    CreatedByUsername = game.CreatedByUsername,
                    GameStatus = GameStatus.Open
                });
            }

            ServerResponse serverResponse = new ServerResponse{
                GameCreated = gameCreated
            };

            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);
            SendTCPDataToAll(data, byteLength);
            // SendTCPDataProto(clientId, data, byteLength);
        }

        public static void SendGameIsReadyToStart(string gameId){
            GameIsReadyToStart gameIsReadyToStart = new GameIsReadyToStart();
            gameIsReadyToStart.GameName = Server.activeGames[gameId].GameName;
            foreach(Client client in Server.activeGames[gameId].clientsInGame){
                gameIsReadyToStart.PlayersInGame.Add(new PlayersInGame{
                    PlayerId = client.player.id,
                    PlayerName = client.player.username,
                    IsGameCreator = client.player.isGameCreator
                });
            }

            ServerResponse serverResponse = new ServerResponse{
                GameIsReadyToStart = gameIsReadyToStart
            };

            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);

            SendTCPDataFromList(Server.activeGames[gameId].clientsInGame, data, byteLength);
            // SendTCPDataProto(clientId, data, byteLength);
        }

        public static void SendUpdateActiveGames(){
            UpdateActiveGames updateActiveGames = new UpdateActiveGames();

            foreach(Game game in Server.activeGames.Values){
                updateActiveGames.ActiveGames.Add(new GameMessage{
                    GameId = game.GameId,
                    GameName = game.GameName,
                    CreatedById = game.CreatedById,
                    CreatedByUsername = game.CreatedByUsername,
                    GameStatus = game.GameStatus
                });
            }

            ServerResponse serverResponse = new ServerResponse{
                UpdateActiveGames = updateActiveGames
            };


            int byteLength = serverResponse.CalculateSize();
            byte[] data = new byte[byteLength];
            serverResponse.WriteTo(data);

            SendTCPDataToAll(data, byteLength);
        }
        #endregion
    }
}
