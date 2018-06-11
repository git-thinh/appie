using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace appie
{
    public interface IApiChannel
    {
        void Execute(msg msg);
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
        private readonly IAPI _api;

        public ApiChannel(IAPI api)
        {
            _api = api;
            _api.Canceler = new ApiChannelCanceler();

            _thread = new Thread(new ParameterizedThreadStart(delegate (object evt)
            {
                ApiChannelCanceler canceler = (ApiChannelCanceler)evt;
                api.Init();
                api.Open = true;
                api.Run();
            }));
            _api.Id = _thread.ManagedThreadId;
            _thread.Start();
        }

        public void Execute(msg msg)
        { 
        }

        public void Stop()
        { 
            _api.Close(); 
        }
    }
}
