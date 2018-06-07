using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace appie
{
    //Used for WM_COPYDATA for string messages
    public struct COPYDATASTRUCT
    {
        public IntPtr dwData;
        public int cbData;
        [MarshalAs(UnmanagedType.LPStr)]
        public string lpData;
    }

    [ProtoContract]
    public class msgOutput
    {
        [ProtoMember(1)]
        public bool Ok = false;

        [ProtoMember(2)]
        public int Total = 0;

        public object Data { set; get; }
    }

    [ProtoContract]
    [ProtoInclude(8, typeof(msgOutput))]
    public class msg
    {
        [ProtoMember(1)]
        public string API = string.Empty;

        [ProtoMember(2)]
        public string KEY = string.Empty;

        [ProtoMember(3)]
        public string Log = string.Empty;

        [ProtoMember(4)]
        public string Token = string.Empty;

        [ProtoMember(5)]
        public int PageNumber = 1;

        [ProtoMember(6)]
        public int PageSize = 10;

        [ProtoMember(7)]
        public int Counter = 0;

        [ProtoMember(8)]
        public msgOutput Output { set; get; }

        public object Input { set; get; }

        public msg()
        {
            Output = new msgOutput();
        }

        public msg clone(object input)
        {
            msg m = Serializer.DeepClone<msg>(this);
            m.Input = input;
            return m;
        }
    }

}
