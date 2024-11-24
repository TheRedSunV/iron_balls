using System;
using System.Collections.Generic;
using System.Text;

namespace ironballs
{
    internal static class MySetup
    {
        private static int _players;
        internal static int Players
        {
            get { return _players; }
            set 
            {
                if (value < 0) _players = 0;
                else if (value > 4) _players = 4;
                else _players = value;
            }
        }
    }
}
