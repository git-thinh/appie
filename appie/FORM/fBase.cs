using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace appie
{
    public class fBase : Form, IFORM //, IMessageFilter
    {
        public IJobStore JobStore { get; }
        public event EventReceiveMessage OnReceiveMessage;

        readonly ListThreadSafe<Guid> StoreMessages;
        readonly System.Threading.Timer timer_api = null;

        public fBase(IJobStore store)
        {
            StoreMessages = new ListThreadSafe<Guid>();
            JobStore = store;
            store.f_form_Add(this);
            this.FormClosing += (se, ev) => { store.f_form_Remove(this); };

            timer_api = new System.Threading.Timer(new System.Threading.TimerCallback((obj) =>
            {
                IFORM form = (IFORM)obj;
                if (StoreMessages.Count > 0)
                {
                    Guid[] ids = StoreMessages.ToArray(true);
                    OnReceiveMessage?.Invoke(form, ids);
                }
            }), this, 100, 100);
        }

        public void f_receiveMessage(Guid[] ids)
        {
            //StoreMessages.Enqueue(id);

        }
    }
}
