using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace appie
{
    public class api_base
    {
        static ConcurrentQueue<msg> cache_api = new ConcurrentQueue<msg>();
        static System.Threading.Timer timer_api = null;

        static ConcurrentQueue<msg> cache_msg = new ConcurrentQueue<msg>();
        static System.Threading.Timer timer_msg = null;

        static IFORM fom = null;

        public api_base()
        {
            if (timer_api == null)
            {
                timer_api = new System.Threading.Timer(new System.Threading.TimerCallback((obj) =>
                {
                    if (fom == null) fom = app.get_Main();
                    if (cache_api.Count > 0)
                    {
                        msg m = cache_api.Dequeue();
                        if (fom != null && m != null)
                            fom.api_responseMsg(null, new threadMsgEventArgs(m));
                    }
                }), fom, 100, 100);
            }

            if (timer_msg == null)
            {
                timer_msg = new System.Threading.Timer(new System.Threading.TimerCallback((obj) =>
                {
                    if (fom == null) fom = app.get_Main();
                    if (cache_msg.Count > 0)
                    {
                        msg m = cache_msg.Dequeue();
                        if (fom != null && m != null)
                            fom.api_responseMsg(null, new threadMsgEventArgs(m));
                    }
                }), fom, 500, 500);
            }
        }

        public void notification_toMain(msg m)
        {
            cache_msg.Enqueue(m);
        }

        public void response_toMain(msg m)
        {
            cache_api.Enqueue(m);
        }

        public void response_toMainRuntime(msg m)
        {
            if (fom == null) fom = app.get_Main();
            if (fom != null)
                fom.api_responseMsg(null, new threadMsgEventArgs(m));
        }

    }
}
