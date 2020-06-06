using System;

namespace Generator
{
#if WINDOWS || LINUX
    /// <summary>
    ///     The main class.
    /// </summary>
    public static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            var KeepLogAfterExit = false;  // TODO: Replace with writing logs to disk
            if (KeepLogAfterExit && Globals.Logging)
                try
                {
                    using (var game = new GameControl())
                    {
                        game.Run();
                    }
                }
                catch (Exception e)
                {
                    Globals.Log(e);
                    Globals.Log("Hit an error. Please hit enter to continue.");
                    Console.ReadLine();
                    throw;
                }
            else
                using (var game = new GameControl())
                {
                    game.Run();
                }
        }
    }
#endif
}