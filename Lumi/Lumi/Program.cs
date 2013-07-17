using System;

namespace Lumi
{
#if WINDOWS || XBOX
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (PlatformerEngine game = new PlatformerEngine())
            {
                game.Run();
            }
        }
    }
#endif
}

