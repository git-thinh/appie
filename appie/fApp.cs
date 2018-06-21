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
                f_send(txt.Text);
            };
            this.Controls.AddRange(new Control[] {
                txt,
                btn,
            });
            this.Shown += (se, ev) =>
            {
                this.WindowState = FormWindowState.Maximized;
            };
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern string SendMessage(int hWnd, int msg, string wParam, IntPtr lParam);

        void f_send(string data) {

            int ParentFormHandle = this.HandleBase.ToInt32();
            SendMessage(ParentFormHandle, 40, "Hello World!", IntPtr.Zero);

            //string windowTitle = "SendMessage Demo";
            ////// Find the window with the name of the main form
            ////IntPtr ptrWnd = NativeMethods.FindWindow(null, windowTitle);
            //IntPtr ptrWnd = this.HandleBase;
            //if (ptrWnd == IntPtr.Zero)
            //{
            //    MessageBox.Show(String.Format("No window found with the title '{0}'.", windowTitle), "SendMessage Demo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //}
            //else
            //{
            //    IntPtr ptrCopyData = IntPtr.Zero;
            //    try
            //    {
            //        // Create the data structure and fill with data
            //        NativeMethods.COPYDATASTRUCT copyData = new NativeMethods.COPYDATASTRUCT();
            //        copyData.dwData = new IntPtr(2);    // Just a number to identify the data type
            //        copyData.cbData = data.Length + 1;  // One extra byte for the \0 character
            //        copyData.lpData = Marshal.StringToHGlobalAnsi(data);

            //        // Allocate memory for the data and copy
            //        ptrCopyData = Marshal.AllocCoTaskMem(Marshal.SizeOf(copyData));
            //        Marshal.StructureToPtr(copyData, ptrCopyData, false);

            //        // Send the message
            //        NativeMethods.SendMessage(ptrWnd, NativeMethods.WM_COPYDATA, IntPtr.Zero, ptrCopyData);
            //    }
            //    catch (Exception ex)
            //    {
            //        MessageBox.Show(ex.ToString(), "SendMessage Demo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //    finally
            //    {
            //        // Free the allocated memory after the contol has been returned
            //        if (ptrCopyData != IntPtr.Zero)
            //            Marshal.FreeCoTaskMem(ptrCopyData);
            //    }
            //}
        }



    }
}
