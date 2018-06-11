using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace appie
{
    public interface IAPI
    {
        int Id { set; get; } 
        bool Open { set; get; }
        ApiChannelCanceler Canceler { set; get; }

        void Init();
        void PostMessage(object data);
        void Run();
        void Pause();
        void Close();
    }
}
