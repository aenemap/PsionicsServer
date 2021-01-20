using System;
using System.Collections.Generic;

namespace PsionicsServer
{
    public class Game
    {
        public string GameId;
        public string GameName;
        public int CreatedById;
        public string CreatedByUsername;
        public List<Client> clientsInGame;
        public string GameStatus;

        public Game(string gameId, string gameName,int createdById, string createdByUsername, Client client){
            this.GameId = gameId;
            this.GameName = gameName;
            this.CreatedById = createdById;
            this.CreatedByUsername = createdByUsername;
            this.clientsInGame = new List<Client>();
            this.clientsInGame.Add(client);
        }
    }
}