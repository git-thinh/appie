using System; 
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace appie
{
    public class api_base
    {
        static readonly object _lock_api = new object();
        static Queue<msg> cache_api = new Queue<msg>();
        static System.Threading.Timer timer_api = null;

        static readonly object _lock_msg = new object();
        static Queue<msg> cache_msg = new Queue<msg>();
        static System.Threading.Timer timer_msg = null;

        static IFORM fom = null;

        public api_base()
        {
            if (timer_api == null)
            {
                timer_api = new System.Threading.Timer(new System.Threading.TimerCallback((obj) =>
                {
                    if (fom == null) fom = app.get_Main();
                    lock (_lock_api)
                    {
                        if (cache_api.Count > 0)
                        {
                            msg m = cache_api.Dequeue();
                            if (fom != null)
                                fom.api_responseMsg(null, new threadMsgEventArgs(m));
                        }
                    }
                }), fom, 100, 100);
            }

            if (timer_msg == null)
            {
                timer_msg = new System.Threading.Timer(new System.Threading.TimerCallback((obj) =>
                {
                    if (fom == null) fom = app.get_Main();
                    lock (_lock_msg)
                    {
                        if (cache_msg.Count > 0)
                        {
                            msg m = cache_msg.Dequeue();
                            if (fom != null)
                                fom.api_responseMsg(null, new threadMsgEventArgs(m));
                        }
                    }
                }), fom, 500, 500);
            }
        }

        public void notification_toMain(msg m)
        {
            lock (_lock_msg) cache_msg.Enqueue(m);
        }

        public void response_toMain(msg m)
        {
            lock (_lock_api) cache_api.Enqueue(m);
        }

        public void response_toMainRuntime(msg m)
        {
            if (fom == null) fom = app.get_Main();
            if (fom != null)
                fom.api_responseMsg(null, new threadMsgEventArgs(m));
        }

    }
}
