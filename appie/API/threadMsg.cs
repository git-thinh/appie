using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace appie
{
    public interface IthreadMsg
    {
        void Execute(msg msg);
        void Stop();
    }


    public class threadMsgPara
    {
        private readonly ManualResetEvent _resetEvent;
        public threadMsgPara(ManualResetEvent resetEvent)
        {
            _resetEvent = resetEvent;
        }
        public ManualResetEvent ResetEvent { get { return _resetEvent; } }
    }

    public sealed class threadMsgEventArgs : EventArgs
    {
        public threadMsgEventArgs(msg msg)
        {
            Message = msg;
        }
        public msg Message { get; set; }
    }

    public class threadMsg : IthreadMsg
    {
        private readonly Thread _thread;
        private readonly ManualResetEvent _threadEvent;
        private readonly ManualResetEvent _resetEvent;
        private readonly IAPI _api;
        private msg _msg;
        private bool _exit = false;
        private event EventHandler<threadMsgEventArgs> onMessageComplete;

        public threadMsg(IAPI api, EventHandler<threadMsgEventArgs> on_message = null)
        {
            onMessageComplete = on_message;
            _api = api;
            _resetEvent = new ManualResetEvent(false);
            _threadEvent = new ManualResetEvent(false);
            _thread = new Thread(new ParameterizedThreadStart(delegate (object evt)
            {
                api.Init();
                api.Open = true;
                app.postToAPI(_API.MEDIA, _API.MEDIA_KEY_INITED, null);
                threadMsgPara tm = (threadMsgPara)evt;
                while (_exit == false)
                {
                    tm.ResetEvent.WaitOne();
                    if (_exit)
                        break;
                    else
                    {
                        msg m = api.Execute(_msg);
                        //if (onMessageComplete != null) onMessageComplete.Invoke(this, new threadMsgEventArgs(m));
                    }
                    tm.ResetEvent.Reset();
                }
            }));
            _thread.Start(new threadMsgPara(_resetEvent));
        }

        public void Execute(msg msg)
        {
            _msg = msg;
            _resetEvent.Set();
        }

        public void Stop()
        {
            _exit = true;
            _api.Close();
            _resetEvent.Set();
        }
    }
}
