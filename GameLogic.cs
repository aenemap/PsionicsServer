using System;
using System.Collections.Generic;
using System.Text;

namespace PsionicsServer
{
    public class GameLogic
    {
        public static void Update()
        {
            for (int i = 1; i <= Server.clients.Count; i++)
            {
                if (Server.clients.ContainsKey(i))
                {
                    if (Server.clients[i].player != null)
                    {
                        Server.clients[i].player.Update();
                    }
                }
            }
            ThreadManager.UpdateMain();
        }
    }
}
