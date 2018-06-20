using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace appie
{
    public class MessageResult
    {
        public int PageNumber { set; get; }
        public int PageSize { set; get; }
        public int Counter { set; get; }
        public int Total { set; get; }

        public bool Ok { set; get; }
        public string MessageText { set; get; }

        public MessageResult() {
            PageNumber = 1;
            PageSize = 10;
            Counter = 0;
            Total = 0;
            Ok = false;
        }

        private object data;

        public object GetData() { return data; }
        public void SetData(object _data) { data = _data; }
    }
    
    public class Message
    {
        readonly Guid Id;
        public MessageResult Output { set; get; }
        public object Input { set; get; }

        public Message(object input = null)
        {
            Id = Guid.NewGuid();
            Output = new MessageResult();
            Input = input;
        }

        public Message(string messageText)
        {
            Id = Guid.NewGuid();
            Output = new MessageResult();
            Output.MessageText = messageText;
        }

        public Message(bool success, object data = null)
        {
            Id = Guid.NewGuid();
            Output = new MessageResult();
            Output.Ok = success;
            Output.SetData(data);
        }

        public Guid GetId()
        {
            return Id;
        }
    }

}
