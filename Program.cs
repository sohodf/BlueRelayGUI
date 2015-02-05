using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;
using BlueRelayControllerDLL;

namespace BlueRelayController
{
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        [DllImport("kernel32.dll")]
        private static extern bool AttachConsole(int dwProcessId);
        [DllImport("kernel32.dll")]
        private static extern bool FreeConsole();

        private const int ATTACH_PARENT_PROCESS = -1;

        private static void Main(string[] args)
        {
            AttachConsole(ATTACH_PARENT_PROCESS);
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new RelayControllerMain());
            }
            else
            {
                

                if (args[0] == "?")
                {
                    Console.WriteLine("");
                    Console.WriteLine("'get' returns the serials of the connected relays");
                    Console.WriteLine(" Usage: exec <relay serial> <port (1-8)> <state (true/false)>");

                }
                else if (args[0] == "get")
                {
                    RelayCommands RC = new RelayCommands();
                    if (RC.GetConnectedRelays().Equals(null))
                    {
                        Console.WriteLine("No connected relays found");
                    }

                    else
                    {
                        Console.WriteLine("");
                        foreach (string serial in RC.GetConnectedRelays())
                        {
                            if (!String.IsNullOrEmpty(serial))
                                Console.WriteLine(serial);
                        }

                    }

                }
                else
                {
                    RelayActions RA = new RelayActions();
                    try
                    {
                        RA.SetRelayPort(args[0], int.Parse(args[1]), bool.Parse(args[2]));
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.Message);
                    }
                    
                    
                }

            }
            
            FreeConsole();
            Application.Exit();
            System.Windows.Forms.SendKeys.SendWait("{ENTER}");
            return;
        }
    }

}
