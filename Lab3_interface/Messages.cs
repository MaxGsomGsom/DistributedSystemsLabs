using System;
using System.Collections.Generic;
using System.Threading;

namespace Lab3_interface
{
    [Serializable]
    public class ComputingTask
    {
        /// <summary>
        /// Main parameter of task, means how much time required to compute task
        /// </summary>
        public int Duration { get; private set; }

        public ComputingTask(int duration)
        {
            Duration = duration;
        }

        /// <summary>
        /// Do some work
        /// </summary>
        public virtual void Run()
        {
            Thread.Sleep(Duration);
        }
    }

    public interface IMessage { }

    #region server messages

    [Serializable]
    public class BalancingRequestMessage : IMessage
    {
        /// <summary>
        /// Maximum tasks duration that client must send to server for balancing
        /// </summary>
        public int MaxTasksDuration { get; private set; }

        public BalancingRequestMessage(int duration)
        {
            MaxTasksDuration = duration;
        }
    }

    #endregion

    #region client messages

    [Serializable]
    public class TasksInfoMessage : IMessage
    {
        /// <summary>
        /// Duration of all tasks of client
        /// </summary>
        public int TotalTasksDuration { get; private set; }
        public string ClientName { get; private set; }

        public TasksInfoMessage(int duration, string clientName)
        {
            TotalTasksDuration = duration;
            ClientName = clientName;
        }
    }

    [Serializable]
    public class ConnectedMessage : TasksInfoMessage
    {
        public ConnectedMessage(int duration, string clientName):base(duration,clientName) { }
    }

    [Serializable]
    public class DisconnectedMessage : IMessage
    {
        public string ClientName { get; private set; }

        public DisconnectedMessage(string clientName)
        {
            ClientName = clientName;
        }
    }

    #endregion

    #region server and client messages

    [Serializable]
    public class TasksMessage : IMessage
    {
        public List<ComputingTask> Tasks { get; private set; }
        public string ClientName { get; private set; }

        public TasksMessage(IEnumerable<ComputingTask> tasks, string clientName)
        {
            Tasks = new List<ComputingTask>(tasks);
            ClientName = clientName;
        }
    }

    #endregion
}
