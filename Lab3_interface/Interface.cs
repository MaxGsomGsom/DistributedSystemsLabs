using System;

namespace Lab3_interface
{
    public delegate void MessageEventHandler(IMessage message);

    public interface IServer
    {
        event MessageEventHandler MessageFromServerEvent;
        void MessageFromClient(IMessage message);
    }

    /// <summary>
    /// Interface of client without realization for server
    /// </summary>
    public class IClient: MarshalByRefObject
    {
        public event MessageEventHandler MessageFromServerEvent;
        public string ClientName { get; private set; }

        public IClient()
        {
            ClientName = "Client" + (new Random()).Next();
        }

        public void OnServerMessage(IMessage message)
        {
            MessageFromServerEvent?.Invoke(message);
        }
    }

    public class EventProxy : MarshalByRefObject
    {
        public event MessageEventHandler MessageFromServerEvent;

        public void OnServerMessage(IMessage Message)
        {
            MessageFromServerEvent?.Invoke(Message);
        }
    }


}
