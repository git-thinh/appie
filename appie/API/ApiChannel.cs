using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace appie
{
    public interface IApiChannel
    {
        void RecieveDataFormWorker(object data);
        void Stop();
    }

    public class ApiChannelCanceler
    {
        object _cancelLocker = new object();
        bool _cancelRequest;
        public bool IsCancellationRequested
        {
            get { lock (_cancelLocker) return _cancelRequest; }
        }

        public void Cancel() { lock (_cancelLocker) _cancelRequest = true; }

        public void ThrowIfCancellationRequested()
        {
            if (IsCancellationRequested) throw new OperationCanceledException();
        }
    }

    public class ApiChannel : IApiChannel
    {
        private readonly Thread _thread;  
        private readonly IApiWorker _worker;

        public ApiChannel(IApiWorker worker)
        {
            worker.Channel = this; 
            _thread = new Thread(new ParameterizedThreadStart(delegate (object evt)
            {
                IApiWorker w = (IApiWorker)evt;
                w.Run();
            }));
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.IsBackground = true;
            worker.ThreadId = _thread.ManagedThreadId;
            _worker = worker;
            _thread.Start(_worker);  
        }

        public void PostDataToWorker(object data)
        {
            if (data == null) return;
            _worker.PostDataToWorker(data);
        }

        public void RecieveDataFormWorker(object data)
        { 
        } 

        public void Stop()
        { 
            _worker.Stop();
            _thread.Join();
        }
    }
}
