using System;
using System.ServiceModel;

namespace YachtDice.Server.Tests.InMemory
{
    internal sealed class FakeCallbackChannel : ICommunicationObject
    {
        private const string LostConnectionMessage = "The client is not connected anymore.";

        public event EventHandler Closed
        {
            add { }
            remove { }
        }

        public event EventHandler Closing
        {
            add { }
            remove { }
        }

        public event EventHandler Faulted
        {
            add { }
            remove { }
        }

        public event EventHandler Opened
        {
            add { }
            remove { }
        }

        public event EventHandler Opening
        {
            add { }
            remove { }
        }

        public CommunicationState State { get; set; } = CommunicationState.Opened;

        public bool IsBroken { get; set; }

        public int ReceivedCount { get; private set; }

        public void Receive()
        {
            if (IsBroken)
            {
                throw new CommunicationException(LostConnectionMessage);
            }

            ReceivedCount++;
        }

        public void Abort()
        {
            State = CommunicationState.Closed;
        }

        public void Close()
        {
            State = CommunicationState.Closed;
        }

        public void Close(TimeSpan timeout)
        {
            State = CommunicationState.Closed;
        }

        public void Open()
        {
            State = CommunicationState.Opened;
        }

        public void Open(TimeSpan timeout)
        {
            State = CommunicationState.Opened;
        }

        public IAsyncResult BeginClose(AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult BeginOpen(AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public IAsyncResult BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            throw new NotSupportedException();
        }

        public void EndClose(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        public void EndOpen(IAsyncResult result)
        {
            throw new NotSupportedException();
        }
    }
}
