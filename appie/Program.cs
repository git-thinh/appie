using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Windows.Forms;

namespace appie
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = 1000;
            // active SSL 1.1, 1.2, 1.3 for WebClient request HTTPS
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Ssl3 | (SecurityProtocolType)3072 | (SecurityProtocolType)0x00000C00 | SecurityProtocolType.Tls;

            Application.EnableVisualStyles();
            Application.Run(new fMain());
        }
    }
}
