using System;
using System.Collections.Generic;
using System.Text;

namespace PsionicsServer
{
    public class Constants
    {
        public const int TICKS_PER_SEC = 30;
        public const int MS_PER_TICK = 1000 / TICKS_PER_SEC;
    }

    public class GameStatus
    {
        public const string Open = "Open";
        public const string Full = "Full";
        public const string GameStarted = "Game Started";
        public const string GameEnded = "Game Ended";
    }
}
