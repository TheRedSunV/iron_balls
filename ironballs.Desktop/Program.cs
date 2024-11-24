using System;
using Urho3DNet;

namespace ironballs
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            Launcher.Run(_ => new UrhoApplication(_));
        }
    }
}
