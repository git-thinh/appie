using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

namespace appie
{
    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public class fBase : Form, IFORM //, IMessageFilter
    {
        public IntPtr HandleBase { get; }
        public IJobStore JobStore { get; }
        public fBase(IJobStore store)
        {
            JobStore = store;
            store.f_form_Add(this);
            this.FormClosing += (se, ev) => { store.f_form_Remove(this); };
            this.HandleBase = this.Handle;

            Application.AddMessageFilter(new TestMessageFilter(this));
        }

        public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        {
            if (m.Msg == 40)
            {
                string data = m.WParam.ToString();
                return true;
            }
            else
            {
                return false;
            }

            //if (m.Msg == NativeMethods.WM_COPYDATA)
            //{
            //    // Extract the file name
            //    NativeMethods.COPYDATASTRUCT copyData = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.COPYDATASTRUCT));
            //    int dataType = (int)copyData.dwData;
            //    if (dataType == 2)
            //    {
            //        string id = Marshal.PtrToStringAnsi(copyData.lpData);
            //        MessageBox.Show(id);
            //    }
            //    else
            //    {
            //        MessageBox.Show(String.Format("Unrecognized data type = {0}.", dataType), "SendMessageDemo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}

            return false;
        }
    }

    [SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
    public class TestMessageFilter : IMessageFilter
    {
        private fBase FOwner; //instance of the form in which you want to handle this pre-processing
        const int WM_KEYUP = 0x101;
        public TestMessageFilter(fBase aOwner)
        {
            FOwner = aOwner;
        }

        public bool PreFilterMessage(ref System.Windows.Forms.Message m)
        {

            if (m.Msg == 40)
            {
                string data = m.WParam.ToString();
                return true;
            }
            else
            {
                return false;
            }

            //if (m.Msg == NativeMethods.WM_COPYDATA)
            //{
            //    // Extract the file name
            //    NativeMethods.COPYDATASTRUCT copyData = (NativeMethods.COPYDATASTRUCT)Marshal.PtrToStructure(m.LParam, typeof(NativeMethods.COPYDATASTRUCT));
            //    int dataType = (int)copyData.dwData;
            //    if (dataType == 2)
            //    {
            //        string id = Marshal.PtrToStringAnsi(copyData.lpData);
            //        MessageBox.Show(id);
            //    }
            //    else
            //    {
            //        MessageBox.Show(String.Format("Unrecognized data type = {0}.", dataType), "SendMessageDemo", MessageBoxButtons.OK, MessageBoxIcon.Error);
            //    }
            //}

            return false;

            ////bool ret = true;
            ////if (m.Msg == WM_KEYUP)
            ////{
            ////    Keys key = (Keys)(int)m.WParam & Keys.KeyCode;
            ////    if (key == Keys.Y)
            ////    {
            ////        if ((int)Control.ModifierKeys == ((int)Keys.Control + (int)Keys.Alt))
            ////        {
            ////            //got it......
            ////        }
            ////        else
            ////        {
            ////            ret = false;
            ////        }

            ////    }

            ////    return ret;
            ////}
            ////else
            ////{
            ////    return false;
            ////}



        }
    }
}
