using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace appie
{
    public class fApp : fBase
    {
        public fApp(IJobStore store) : base(store)
        {
            TextBox txt = new TextBox() { Dock = DockStyle.Fill, Multiline = true, ScrollBars = ScrollBars.Vertical };
            Button btn = new Button() { Text = "Test", Dock = DockStyle.Top };
            btn.Click += (se, ev) =>
            { 
            };
            this.Controls.AddRange(new Control[] {
                txt,
                btn,
            });
            this.Shown += (se, ev) =>
            {
                this.WindowState = FormWindowState.Maximized;
            };

            this.OnReceiveMessage += f_event_OnReceiveMessage;
        }

        private void f_event_OnReceiveMessage(IFORM form, Guid[] ids)
        {

        }
    }
}
