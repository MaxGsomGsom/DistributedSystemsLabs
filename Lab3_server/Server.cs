using Lab3_interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;

namespace Lab3_server
{
    public class Server : MarshalByRefObject, IServer
    {
        private TcpServerChannel serverChannel;
        private ObjRef internalRef;
        private static string serverUrl = "interface";

        public bool ServerActive { get; private set; }
        public int ServerPort { get; private set; }

        public event MessageEventHandler MessageFromServerEvent;

        //stores clients names with total duration of their tasks
        private Dictionary<string, int> clients;

        public Server(int port)
        {
            ServerActive = false;
            ServerPort = port;

            clients = new Dictionary<string, int>();
        }

        public void StartServer()
        {
            if (ServerActive)
                return;

            Hashtable props = new Hashtable();
            props["port"] = ServerPort;
            props["name"] = serverUrl;

            //Set up for remoting events properly
            BinaryServerFormatterSinkProvider serverProv =
                  new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel =
                  System.Runtime.Serialization.Formatters.TypeFilterLevel.Full;

            serverChannel = new TcpServerChannel(props, serverProv);

            try
            {
                ChannelServices.RegisterChannel(serverChannel, false);
                internalRef = RemotingServices.Marshal(this,
                                 props["name"].ToString());
                ServerActive = true;
                Console.WriteLine("Server started");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }

        }

        public void StopServer()
        {
            if (!ServerActive)
                return;

            RemotingServices.Unmarshal(internalRef);

            try
            {
                ChannelServices.UnregisterChannel(serverChannel);
                Console.WriteLine("Server stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Send message to all clients or to client with clientName
        /// </summary>
        /// <param name="message"></param>
        /// <param name="clientName">Null or name of client</param>
        private void SendMessageToClient(IMessage message, string clientName = null)
        {
            if (!ServerActive)
                return;

            //No Listeners
            if (MessageFromServerEvent == null)
                return;

            MessageEventHandler listener = null;
            Delegate[] dels = MessageFromServerEvent.GetInvocationList();

            foreach (Delegate del in dels)
            {
                IClient cli = (IClient)del.Target;
                if (clientName == null || clientName == cli.ClientName)
                    try
                    {
                        Console.WriteLine("Send: " + message.GetType().Name);
                        listener = (MessageEventHandler)del;
                        listener.Invoke(message);
                    }
                    catch
                    {
                        //Could not reach the destination, so remove it from the list
                        MessageFromServerEvent -= listener;
                        clients.Remove(cli.ClientName);
                    }
            }
        }


        /// <summary>
        /// Main method of server, processes messages of clients
        /// </summary>
        /// <param name="message"></param>
        public void MessageFromClient(IMessage message)
        {

            if (message is ConnectedMessage)
            {
                //add new client and his total duration
                var m = message as ConnectedMessage;
                if (clients.ContainsKey(m.ClientName)) clients[m.ClientName] = m.TotalTasksDuration;
                else clients.Add(m.ClientName, m.TotalTasksDuration);
                Console.WriteLine("Receive: " + message.GetType().Name + " | Connected clients: " + clients.Count);
            }
            else if (message is DisconnectedMessage)
            {
                //delete client
                var m = message as DisconnectedMessage;
                clients.Remove(m.ClientName);
                Console.WriteLine("Receive: " + message.GetType().Name + " | Connected clients: " + clients.Count);
            }
            else if (message is TasksInfoMessage)
            {
                //update total duration of tasks of client and run check of balance
                var m = message as TasksInfoMessage;
                if (clients.ContainsKey(m.ClientName)) clients[m.ClientName] = m.TotalTasksDuration;
                else clients.Add(m.ClientName, m.TotalTasksDuration);

                Console.WriteLine("Receive: " + message.GetType().Name);

                CheckBalance();
            }
            else if (message is TasksMessage)
            {
                Console.WriteLine("Receive: " + message.GetType().Name);

                //send received tasks to client with min total duration 
                var m = message as TasksMessage;
                SendTasksToFreeClient(m);
            }
        }


        void CheckBalance()
        {
            if (clients.Count < 2) return;

            var min = new KeyValuePair<string, int>(string.Empty, int.MaxValue);
            var max = new KeyValuePair<string, int>(string.Empty, int.MinValue);

            //find delta between max and min total duration of tasks of client
            foreach (var item in clients)
            {
                if (item.Value > max.Value)
                    max = item;
                if (item.Value < min.Value)
                    min = item;
            }

            int durationToRequest = max.Value - min.Value;

            //if delta duration greater than total duration of min client
            if (durationToRequest > min.Value)
            {
                //send request of tasks to client with max total duration
                SendMessageToClient(new BalancingRequestMessage(durationToRequest / 2), max.Key);
            }
        }


        void SendTasksToFreeClient(TasksMessage message)
        {
            var min = new KeyValuePair<string, int>(string.Empty, int.MaxValue);

            //find client with min duration
            foreach (var item in clients)
            {
                if (item.Value < min.Value)
                    min = item;
            }

            //calc duration of tasks in message
            int duration = 0;
            foreach (var item in message.Tasks)
            {
                duration += item.Duration;
            }

            //add this duration to total duration of min client, and sub it from total duration of sender
            clients[message.ClientName] -= duration;
            clients[min.Key] += duration;

            SendMessageToClient(message, min.Key);
        }
    }
}
