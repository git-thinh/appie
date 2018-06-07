using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace appie
{
    public interface IAPI
    {
        bool Open { set; get; }

        void Init();
        msg Execute(msg msg);
        void Close();
    }
}
