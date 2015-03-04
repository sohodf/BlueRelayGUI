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
            
            if (args.Length == 0)
            {

                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new RelayControllerMain());
            }
            else
            {
                AttachConsole(ATTACH_PARENT_PROCESS);

                if (args[0] == "?" || args[0] == "/?" || args[0] == "help")
                {
                    Console.WriteLine("");
                    Console.WriteLine("'get' returns the serials of the connected relays");
                    Console.WriteLine(" To change a specific port use:");
                    Console.WriteLine(" Usage: exec <relay serial> <port (1-8)> <state (true/false)>");
                    Console.WriteLine(" To toggle all ports at once use:");
                    Console.WriteLine(" Usage: exec <relay serial> allon/alloff");

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
                        string[] temp = RC.GetConnectedRelays();
               
                        
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

                    if (args[1] == "alloff")
                        try
                        {
                            RA.WriteByteToRelay(args[0], 0);
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine(e.Message);
                        }

                    else if (args[1] == "allon")
                        try
                        {
                            RA.WriteByteToRelay(args[0], 255);
                        }
                        catch (Exception e)
                        {

                            Console.WriteLine(e.Message);
                        }
                    
                    else
                    
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
            if (System.Windows.Forms.Application.MessageLoop)
            {
                // Use this since we are a WinForms app
                System.Windows.Forms.Application.Exit();
            }
            else
            {
                // Use this since we are a console app
                System.Windows.Forms.SendKeys.SendWait("{ENTER}");
                System.Environment.Exit(1);
            }
            System.Windows.Forms.SendKeys.SendWait("{ENTER}");
        }
    }

}
