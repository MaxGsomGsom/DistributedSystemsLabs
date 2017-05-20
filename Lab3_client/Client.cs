using Lab3_interface;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Channels;
using System.Runtime.Remoting.Channels.Tcp;
using System.Runtime.Serialization.Formatters;
using System.Threading.Tasks;

namespace Lab3_client
{
    class Client
    {
        IServer remoteServer;
        TcpChannel tcpChan;
        BinaryClientFormatterSinkProvider clientProv;
        BinaryServerFormatterSinkProvider serverProv;
        private static string serverUrl = "interface";

        public string ServerIpPort { get; private set; }
        public bool Connected { get; private set; }
        public IClient ClientInterface { get; private set; }

        private ConcurrentQueue<ComputingTask> tasks;

        public Client(string ipPort, IEnumerable<ComputingTask> tasks)
        {
            ClientInterface = new IClient();
            ClientInterface.MessageFromServerEvent += OnServerMessage;

            this.tasks = new ConcurrentQueue<ComputingTask>(tasks);

            Connected = false;
            ServerIpPort = ipPort;

            clientProv = new BinaryClientFormatterSinkProvider();
            serverProv = new BinaryServerFormatterSinkProvider();
            serverProv.TypeFilterLevel = TypeFilterLevel.Full;

            Hashtable props = new Hashtable();
            props["name"] = ClientInterface.ClientName;
            props["port"] = 0; //First available port

            tcpChan = new TcpChannel(props, clientProv, serverProv);
            ChannelServices.RegisterChannel(tcpChan, false);

            RemotingConfiguration.RegisterWellKnownClientType(
              new WellKnownClientTypeEntry(typeof(IServer), "tcp://" + ServerIpPort + "/" + serverUrl));

        }

        /// <summary>
        /// Connect to server
        /// </summary>
        public void Connect()
        {
            if (Connected)
                return;

            try
            {
                //bind client interface event to server
                remoteServer = (IServer)Activator.GetObject(typeof(IServer), "tcp://" + ServerIpPort + "/" + serverUrl);
                remoteServer.MessageFromServerEvent += ClientInterface.OnServerMessage;

                Connected = true;
                SendMessageToServer(new ConnectedMessage(ComputeTotalDuration(), ClientInterface.ClientName));
                Console.WriteLine("[= Connected to server | Client name: " + ClientInterface.ClientName + " =]");
            }
            catch (Exception ex)
            {
                Connected = false;
                Console.WriteLine(ex.Message);
            }
        }

        /// <summary>
        /// Disconnect from server
        /// </summary>
        public void Disconnect()
        {
            if (!Connected)
                return;

            //send all tasks to server before disconnect
            SendTasksToServer(new BalancingRequestMessage(int.MaxValue));
            SendMessageToServer(new DisconnectedMessage(ClientInterface.ClientName));

            try
            {
                //unbind client interface event from server
                remoteServer.MessageFromServerEvent -= ClientInterface.OnServerMessage;
            }
            catch { }

            ChannelServices.UnregisterChannel(tcpChan);

            Console.WriteLine("[= Disconnected from server =]");
        }

        /// <summary>
        /// Send any message to server
        /// </summary>
        /// <param name="message"></param>
        public void SendMessageToServer(IMessage message)
        {
            if (!Connected)
                return;

            try
            {
                Console.WriteLine("Send: " + message.GetType().Name + " | Tasks duration: " + ComputeTotalDuration() + " ms");
                remoteServer.MessageFromClient(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                Connected = false;
            }
        }


        /// <summary>
        /// Processes messages of server
        /// </summary>
        /// <param name="message"></param>
        public void OnServerMessage(IMessage message)
        {
            if (message is BalancingRequestMessage)
            {
                var m = message as BalancingRequestMessage;
                SendTasksToServer(m);
            }
            else if (message is TasksMessage)
            {
                //add received tasks to queue
                var m = message as TasksMessage;
                foreach (var item in m.Tasks)
                {
                    tasks.Enqueue(item);
                }

                Console.WriteLine("Receive: " + message.GetType().Name + " | Tasks duration: " + ComputeTotalDuration() + " ms");
            }
        }

        /// <summary>
        /// Send tasks from queue to server for balancing
        /// </summary>
        /// <param name="m"></param>
        private void SendTasksToServer(BalancingRequestMessage m)
        {
            List<ComputingTask> tasksToSend = new List<ComputingTask>();
            int totalDuration = 0;

            //collect some tasks from queue with total duration received in message
            while (true)
            {
                ComputingTask item;
                if (tasks.TryPeek(out item))
                {
                    if (item.Duration + totalDuration < m.MaxTasksDuration)
                    {
                        if (tasks.TryDequeue(out item))
                        {
                            tasksToSend.Add(item);
                            totalDuration += item.Duration;
                        }
                    }
                    else break;
                }
                else break;
            }

            //send collected tasks
            if (totalDuration > 0)
            {
                Console.WriteLine("Receive: " + m.GetType().Name);
                SendMessageToServer(new TasksMessage(tasksToSend, ClientInterface.ClientName));
            }
        }

        /// <summary>
        /// Add task to queue
        /// </summary>
        /// <param name="task"></param>
        public void AddTaskToCompute(ComputingTask task)
        {
            //send update to server and add task to queue
            tasks.Enqueue(task);
            SendMessageToServer(new TasksInfoMessage(ComputeTotalDuration(), ClientInterface.ClientName));
        }

        /// <summary>
        /// Async compute tasks from queue
        /// </summary>
        public async void StartComputingTasks()
        {
            await Task.Run(() =>
            {
                ComputingTask compTask;

                //do tasks, send update to server after every ended task
                while (true)
                {
                    if (tasks.TryDequeue(out compTask))
                    {
                        compTask.Run();
                        Console.WriteLine("[= Task completed in " + compTask.Duration + " ms =]");
                        SendMessageToServer(new TasksInfoMessage(ComputeTotalDuration(), ClientInterface.ClientName));
                    }
                }
            });
        }

        int ComputeTotalDuration()
        {
            int total = 0;
            foreach (var item in tasks)
            {
                total += item.Duration;
            }

            return total;
        }
    }
}
