using System;
using System.Collections.Generic;
using System.Text;
using Urho3DNet;


namespace ironballs
{
    internal class Team
    {
        internal List<Node> Balls = new List<Node>();
        internal bool IsPlayer = false;
        internal bool IsActive = false;
        internal string Name = "team1";
        internal Team(string name)
        {
            Name = name;
        }
    }
}
