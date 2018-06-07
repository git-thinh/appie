using AxWMPLib;
using System;
using System.Collections.Generic;
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
            m_media.settings.volume = 100;
            //m_media.uiMode = "none";

            //m_media.URL = "http://localhost:17909/?key=MP41332697176";
            m_media.URL = "https://r7---sn-jhjup-nbol.googlevideo.com/videoplayback?sparams=clen%2Cdur%2Cei%2Cgir%2Cid%2Cinitcwndbps%2Cip%2Cipbits%2Citag%2Clmt%2Cmime%2Cmm%2Cmn%2Cms%2Cmv%2Cpcm2%2Cpl%2Cratebypass%2Crequiressl%2Csource%2Cexpire&ip=113.20.96.116&ratebypass=yes&id=o-AOLJZSgGBWxgcZ-LY1egw0c_LmFlE8xK4tYaWJkfIec3&c=WEB&fvip=1&expire=1528362756&mm=31%2C29&ms=au%2Crdu&ei=o6IYW-afL82u4AKBibLoBA&pl=23&itag=18&mt=1528341082&mv=m&signature=4173045360861DAD91316A65BEA4AA7D68F7FF99.BCEA5E84BA94D5CA9D2C8F57236DEE2D75DD95FF&source=youtube&requiressl=yes&mime=video%2Fmp4&gir=yes&clen=246166&mn=sn-jhjup-nbol%2Csn-i3b7knld&initcwndbps=216250&ipbits=0&pcm2=yes&dur=5.596&key=yt6&lmt=1467908538636985";

        }
    }
}
