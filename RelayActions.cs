using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using BlueRelayControllerDLL;
using FTD2XX_NET;
using System.Collections;
using System.Windows.Forms;
using System.Linq.Expressions;

namespace BlueRelayController
{
    public class RelayActions
    {
       private const string BLUE_RELAY_MUTEX = "BlueRelayMutex";
       Mutex mutex = new Mutex();
       private static int MUTEX_TIMEOUT = 5000;
       RelayCommands relay = new RelayCommands();
       private byte[] _sentBytes = new byte[2];
       private uint _receivedBytes;
       private FTDI.FT_STATUS _ftStatus;
       private RelayState m_RelayState;
        
        /// <summary>
        /// turns a relay on. only called from "set relay port"
        /// </summary>
        /// <param name="ftdi">the relay to preform actions on</param>
        /// <param name="relayNum">tge number of the relay to set</param>
       public void RelayOn(FTDI ftdi, int relayNum)
       {
           byte[] _sentBytes = new byte[2];
           int pow = relayNum - 1;
           int command = (int)Math.Pow(2, pow);
           byte toSend = (byte)command;
           _sentBytes[0] = (byte)(_sentBytes[0] | toSend);

           if (ftdi.Write(_sentBytes, 1, ref _receivedBytes) != FTDI.FT_STATUS.FT_OK)
           {
               throw new Exception("Failed to turn on relay");
           }
       }


       //returns a string array of all the connected devices
       public string[] GetConnectedRelays()
       {
           ArrayList serials = new ArrayList();
           FTDI relay = new FTDI();
           FTDI.FT_DEVICE_INFO_NODE[] list = new FTDI.FT_DEVICE_INFO_NODE[100];
           relay.GetDeviceList(list);
           foreach (FTDI.FT_DEVICE_INFO_NODE tmp in list)
           {
               if (tmp != null)
               {
                   serials.Add(tmp.SerialNumber);
               }
           }
           string[] serialsArray = new string[serials.Count];
           int i = 0;
           foreach (string serial in serials)
           {
               serialsArray[i] = serial;
               i++;
           }
           return serialsArray;
       }



       //handles complete atomic setting of a relay
       public void SetRelayPort(string relaySerial, int relayPort, bool status)
       {
           Mutex mutex = null;
           try
           {
               mutex = AcquireMutex(BLUE_RELAY_MUTEX);
               byte[] sentBytes = new byte[2];
               uint receivedBytes = 0;
               FTDI activeRelay = new FTDI();

               try
               {
                   activeRelay = OpenRelay(relaySerial);
               }
               catch (Exception e)
               {
                   Console.WriteLine(e.Message);
                   //return FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED;
               }

               lock (activeRelay)
               {
                   try
                   {
                       //get current relay status
                       byte currentStatus = (byte)GetRelayStatus(activeRelay);
                       // calculate the exact commands to be sent
                       int pow = relayPort - 1;
                       //int command = Power(2, pow);
                       int command = (int)Math.Pow((double)2, (double)pow);
                       //XOR between current status and requested bit
                       sentBytes[0] = (byte)(command ^ currentStatus);

                       FTDI.FT_STATUS ftdiStatus = activeRelay.Write(sentBytes, 1, ref receivedBytes);

                       if (ftdiStatus != FTDI.FT_STATUS.FT_OK)
                       {
                           throw new Exception("Bad status: " + ftdiStatus);
                       }
                   }

                   catch (Exception RelayNotOpen)
                   {
                       Console.WriteLine(RelayNotOpen.Message);
                   }
                   finally
                   {
                       CloseRelay(activeRelay);
                       if (mutex != null)
                           mutex.ReleaseMutex();
                   }
               }

               //return FTDI.FT_STATUS.FT_OK;
           }
           catch (Exception mna)
           {
               Console.WriteLine(mna.Message);
           }
           finally
           {
               if (mutex != null)
                   mutex.ReleaseMutex();
               Console.WriteLine("[SetRelayPort] Mutex released");
           }

       }


       /// <summary>
       /// Acquire a system-wide mutex with the specified name.
       /// </summary>
       /// <param name="mutexName">The system-wide mutex name</param>
       /// <returns></returns>
       private Mutex AcquireMutex(string mutexName)
       {
           Console.WriteLine("[AcquireMutex] Trying to acquire mutex: {0}", mutexName);

           bool createdNew;

           Mutex mutex = new Mutex(true, mutexName, out createdNew);

           // (from MSDN)
           // This thread owns the mutex only if it both requested  
           // initial ownership and created the named mutex. Otherwise, 
           // it can request the named mutex by calling WaitOne. 
           if (!createdNew)
           {
               if (!mutex.WaitOne(MUTEX_TIMEOUT))
                   throw new Exception("Mutex not acquired.");
           }

           Console.WriteLine("[AcquireMutex] Mutex acquired: {0}", mutexName);
           return mutex;
       }



       //assumption - every relay action is atomic.
       //a each relay action consists of open, set and close operations.
       public FTD2XX_NET.FTDI OpenRelay(string relaySerial)
       {
           FTDI newRelay = new FTDI();
           try
           {

               Console.WriteLine(" --> OpenRelay: " + relaySerial);

               if (newRelay.OpenBySerialNumber(relaySerial) != FTDI.FT_STATUS.FT_OK)
               {
                   Console.WriteLine("relay.OpenBySerialNumber() failed.");
               }


               _ftStatus = newRelay.SetBaudRate(921600);
               if (_ftStatus != FTDI.FT_STATUS.FT_OK)
               {
                   // Wait for a key press
                   throw new Exception("Failed to set baudrate (error " + _ftStatus + ")");
               }

               _ftStatus = newRelay.SetBitMode(255, 4);
               if (_ftStatus != FTDI.FT_STATUS.FT_OK)
               {
                   // Wait for a key press
                   throw new Exception("Failed to set bit mode (error " + _ftStatus + ")");
               }

           }
           catch (Exception FailedToOpenRelay)
           {
               Console.WriteLine(FailedToOpenRelay.Message);
               return null;
           }
           return newRelay;
       }
        /// <summary>
        /// closes the recieved relay
        /// </summary>
        /// <param name="relay">ftdi instance</param>
       public void CloseRelay(FTDI relay)
       {
           try
           {
               string deviceSerialNum = "";
               relay.GetSerialNumber(out deviceSerialNum);
               Console.WriteLine(" <-- CloseRelay: " + deviceSerialNum);
               if (relay.Close() != FTDI.FT_STATUS.FT_OK)
               {
                   MessageBox.Show("relay.Close() failed.");
               }

               //if (relay.IsOpen)
               //MessageBox.Show("failed to close relay after writing");
           }
           catch (Exception RelayNotOpen)
           {
               Console.WriteLine(RelayNotOpen.Message);
           }
       }

        /// <summary>
        ///  gets relay status after opening a connectio  to it
        /// </summary>
        /// <param name="relay">recieves an ftdi relay instance</param>
        /// <returns>uint representing the status of the relay</returns>
       public uint GetRelayStatus(FTDI relay)
       {
           byte relayStatus = 0;
           try
           {
               relay.GetPinStates(ref relayStatus);
           }
           catch (Exception RelayNotOpen)
           {
               Console.WriteLine(RelayNotOpen.Message);
               return 260;
           }
           return (uint)relayStatus;
       }

       /// <summary>
       /// executes an action using mutex
       /// </summary>
       /// <param name="actionToDo">the method to run with mutex</param>
       public void MutexWrapper(Action actionToDo)
       {
           Mutex mutex = null;

           try
           {
               mutex = AcquireMutex(BLUE_RELAY_MUTEX);
               actionToDo.Invoke();
           }
           catch (Exception noMutex)
           {
               Console.WriteLine(noMutex.Message);
           }
           finally
           {
               if (mutex != null)
                    mutex.ReleaseMutex();
               Console.WriteLine("[MutexWrapper] Mutex released");
           }
       }

       /// <summary>
       /// executes an action using mutex
       /// </summary>
       /// <param name="actionToDo">the method to run with mutex</param>
       public T MutexWrapperWithRetval<T>(Func<T> actionToDo)
       {
           Mutex mutex = null;

           try
           {
               mutex = AcquireMutex(BLUE_RELAY_MUTEX);
               return actionToDo();
           }
           catch (Exception noMutex)
           {
               Console.WriteLine(noMutex.Message);
               return default(T);
           }
           finally
           {
               if (mutex != null)
               {
                   mutex.ReleaseMutex();
                   Console.WriteLine("[MutexWrapperWithRetval] Mutex released");
               }
           }
       }

       //handles complete atomic setting of a relay
       public void WriteByteToRelay(string relaySerial, byte toWrite)
       {
           Mutex mutex = null;
           FTDI activeRelay = new FTDI();
           try
           {
               mutex = AcquireMutex(BLUE_RELAY_MUTEX);
               byte[] sentBytes = new byte[2];
               uint receivedBytes = 0;

               try
               {
                   activeRelay = OpenRelay(relaySerial);
               }
               catch (Exception e)
               {
                   Console.WriteLine(e.Message);
                   //return FTDI.FT_STATUS.FT_DEVICE_NOT_OPENED;
               }

               lock (activeRelay)
               {
                   try
                   {
                       sentBytes[0] = toWrite;

                       FTDI.FT_STATUS ftdiStatus = activeRelay.Write(sentBytes, 1, ref receivedBytes);

                       if (ftdiStatus != FTDI.FT_STATUS.FT_OK)
                       {
                           throw new Exception("Bad status: " + ftdiStatus);
                       }
                   }

                   catch (Exception RelayNotOpen)
                   {
                       Console.WriteLine(RelayNotOpen.Message);
                   }
                   //finally
                   //{
                   //    CloseRelay(activeRelay);
                   //    if (mutex != null)
                   //        mutex.ReleaseMutex();
                   //}
               }

               //return FTDI.FT_STATUS.FT_OK;
           }
           catch (Exception mna)
           {
               Console.WriteLine(mna.Message);
           }
           finally
           {
               if (mutex != null)
                   mutex.ReleaseMutex();
               CloseRelay(activeRelay);
               Console.WriteLine("[SetRelayPort] Mutex released");
           }

       }

    }

}
