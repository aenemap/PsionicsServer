using PsionicsCardGameProto;
using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace PsionicsServer
{
    public class ServerHandle
    {

        public static void WelcomeReceived(int _fromClient, WelcomeReceived welcomeReceived)
        {
            int _clientIdCheck = welcomeReceived.Id;
            string name = welcomeReceived.ClientName;
            string _username = welcomeReceived.Username;

            Console.WriteLine($"{Server.clients[_fromClient].tcp.socket.Client.RemoteEndPoint} connected successfully and is now player {_fromClient}");

            if (_fromClient != _clientIdCheck)
            {
                Console.WriteLine($"Player \"{_username}\" (ID: {_fromClient}) has assumed the wrong clientID ({_clientIdCheck})");
            }

            Server.clients[_fromClient].SendIntoLobby(_username);

            // ServerSend.SendPlayerToLobby(_fromClient, "Lobby", _username);

            // ServerSend.ChangeSceneToClient(_fromClient, "Lobby");

            // Server.clients[_fromClient].SendIntoGame(_username);
        }

        public static void PlayerMovement(ClientRequest clientRequest)
        {
            bool[] _inputs = new bool[clientRequest.PlayerMovement.Inputs.Count];
            for (int i = 0; i < _inputs.Length; i++)
            {
                _inputs[i] = clientRequest.PlayerMovement.Inputs[i];
            }

            Quaternion rotation = new Quaternion(
                clientRequest.PlayerMovement.PlayerRotation.X,
                clientRequest.PlayerMovement.PlayerRotation.Y,
                clientRequest.PlayerMovement.PlayerRotation.Z,
                clientRequest.PlayerMovement.PlayerRotation.W
            );
            if (Server.clients.ContainsKey(clientRequest.PlayerMovement.ClientId))
            {
                Server.clients[clientRequest.PlayerMovement.ClientId].player.SetInput(_inputs, rotation);
            }
        }

        public static void ChangeSceneToAllClients(ClientRequest clientRequest)
        {
            foreach (Client client in Server.clients.Values)
            {
                ServerSend.ChangeSceneToClient(client.id, clientRequest.ChangeScene.SceneName);
            }
        }

        public static void SendChatMessageToAll(ClientRequest clientRequest){
            foreach (Client client in Server.clients.Values)
            {
                if (client.id != clientRequest.ChatMessage.ClientId){
                    ServerSend.SendChatMessage(client.id, clientRequest.ChatMessage);
                }
            }
        }

        public static void AddNewGame(ClientRequest clientRequest){
            string newGameId = Guid.NewGuid().ToString();
            Game newGame = new Game(
                newGameId,
                clientRequest.CreateGame.GameName,
                clientRequest.CreateGame.CreatedById,
                clientRequest.CreateGame.CreatedByUsername,
                Server.clients[clientRequest.CreateGame.CreatedById]
            );
            Server.activeGames.Add(newGameId, newGame);
            Server.clients[clientRequest.CreateGame.CreatedById].player.isGameCreator = true;
            foreach(Game game in Server.activeGames.Values){
                Console.WriteLine($"gameId: {game.GameId} - gameName: {game.GameName}");
            }
            ServerSend.SendGameCreated(clientRequest.CreateGame.CreatedById, newGameId);
        }

        public static void JoinGameAsOpponent(ClientRequest clientRequest){
            Server.activeGames[clientRequest.JoinGame.GameId].clientsInGame.Add(Server.clients[clientRequest.JoinGame.ClientId]);
            if (Server.activeGames[clientRequest.JoinGame.GameId].clientsInGame.Count == 2){
                Server.activeGames[clientRequest.JoinGame.GameId].GameStatus = GameStatus.Full;
            }
            ServerSend.SendGameIsReadyToStart(clientRequest.JoinGame.GameId);
            ServerSend.SendUpdateActiveGames();
        }

        public static void SendPlayersToGameScene(ClientRequest clientRequest){
            foreach(Client client in Server.activeGames[clientRequest.SendIntoGame.GameId].clientsInGame){
                ServerSend.ChangeSceneToClient(client.id, "MainGameScene");
            }
        }

        public static void HandleClientRequest(byte[] _data, int id)
        {
            ClientRequest clientRequest = ClientRequest.Parser.ParseFrom(_data);
            switch (clientRequest.RequestCase)
            {
                case ClientRequest.RequestOneofCase.WelcomeReceived:
                    WelcomeReceived(id, clientRequest.WelcomeReceived);
                    break;
                case ClientRequest.RequestOneofCase.PlayerMovement:
                    int clientId = clientRequest.PlayerMovement.ClientId;
                    if (clientId == 0)
                    {
                        return;
                    }
                    PlayerMovement(clientRequest);
                    break;
                case ClientRequest.RequestOneofCase.ChangeScene: 
                    ChangeSceneToAllClients(clientRequest);
                    break;
                case ClientRequest.RequestOneofCase.ChatMessage:
                    SendChatMessageToAll(clientRequest);
                    break;
                case ClientRequest.RequestOneofCase.CreateGame:
                    AddNewGame(clientRequest);
                    break;
                case ClientRequest.RequestOneofCase.JoinGame:
                    JoinGameAsOpponent(clientRequest);
                    break;
                case ClientRequest.RequestOneofCase.SendIntoGame:
                    SendPlayersToGameScene(clientRequest);
                    break;
                default:
                    Console.WriteLine("Uknown Client Request");
                    break;

            }
        }
    }
}
