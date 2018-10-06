using System;
using System.Collections.Generic;
using System.Text;

namespace MainApplication
{
    static class Const
    {
        public const byte PACKET_START = 0xAB;
        public const byte PACKET_END = 0xCD;
        public const byte COMMAND_ACK = 0x01;
        public const byte CAPTURE_PERIOD_NUM = 0;
        public const byte X_VAR_NAME = 1;
        public const byte NUM_DATA_PTS = 2;        
        public const byte AUTO_REPEAT = 3;
        public const byte START_AT_ZERO = 4;
        public const byte START_STOP = 5;
        public const byte SAVE = 6;
        public const byte TITLE = 0;
        public const byte GRID = 1;
        public const byte SYMBOL = 2;
        public const byte REMOVE_PLOT = 3;
        public const byte VAR_NAME = 0;
        public const byte VAR_SIZE = 4;
        public const byte DEBUG_MODE = 1;
    }
}
