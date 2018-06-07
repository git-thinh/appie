using AxWMPLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace appie
{
   public class fMedia: Form
    {
        private AxWindowsMediaPlayer m_media;

        public fMedia()
        {
            // MEDIA
            m_media = new AxWindowsMediaPlayer();
            m_media.Name = "m_media";
            m_media.Dock = DockStyle.Fill;
            m_media.Enabled = true;
            m_media.PlayStateChange += new _WMPOCXEvents_PlayStateChangeEventHandler(this.f_media_event_PlayStateChange);
            this.Controls.Add(m_media);
             
            this.Shown += f_media_Shown;
        }

        private void f_media_event_PlayStateChange(object sender, _WMPOCXEvents_PlayStateChangeEvent e)
        { 
        }

        private void f_media_Shown(object sender, EventArgs e)
        {
            this.Closing += new CancelEventHandler(this.Form1_Closing);  // Set EventHandler

            m_media.settings.volume = 100;
            //m_media.uiMode = "none";

            m_media.URL = "1.m4a";
            //m_media.URL = "2.mp4";
            //m_media.URL = "http://localhost:17909/?key=MP41332697176";
            //m_media.URL = "https://r7---sn-jhjup-nbol.googlevideo.com/videoplayback?sparams=clen%2Cdur%2Cei%2Cgir%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&ip=113.20.96.116&ratebypass=yes&id=o-AOLJZSgGBWxgcZ-LY1egw0c_LmFlE8xK4tYaWJkfIec3&c=WEB&fvip=1&expire=1528362756&mm=31%2C29&ms=au%2Crdu&ei=o6IYW-afL82u4AKBibLoBA&pl=23&itag=18&mt=1528341082&mv=m&signature=4173045360861DAD91316A65BEA4AA7D68F7FF99.BCEA5E84BA94D5CA9D2C8F57236DEE2D75DD95FF&source=youtube&requiressl=yes&mime=video%2Fmp4&gir=yes&clen=246166&mn=sn-jhjup-nbol%2Csn-i3b7knld&initcwndbps=216250&ipbits=0&pcm2=yes&dur=5.596&key=yt6&lmt=1467908538636985";

        }

        public void Form1_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            // Memory Leaks in Window Media Player + c#
            // https://www.codeproject.com/Questions/155836/Memory-Leaks-in-Window-Media-Player-c
            // https://www.codeproject.com/Questions/610465/MemoryplusLeaksplusinplusWindowplusMediaplusPlayer
            // Dispose of the Windows Media Player as the form is closing
            //
            // Note: The Form.Closed and Form.Closing events are not raised when the
            //   Application.Exit method is called to exit an application.    
            //   If Application.Exit is to be used, you should call the Form.Close method 
            //   before calling the Application.Exit method.
            //
            if (this.Controls.ContainsKey("m_media"))
            {
                this.Controls.RemoveByKey("m_media"); // Remove from Conrols collection
                m_media.close(); // Close the Windows Media Player control
                m_media.Dispose(); // Dispose
                m_media = null;
            }
            GC.Collect(); // Start .NET CLR Garbage Collection
            GC.WaitForPendingFinalizers(); // Wait for Garbage Collection to finish
        }

    }
}
