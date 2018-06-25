using Newtonsoft.Json;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace appie
{
    public enum MLOG_TYPE
    {
        NONE = 0,
        LOG_ALL = 1,
        LOG_UI = 2,
        LOG_CONSOLE = 3,
        LOG_TRACE = 4
    }

    public class MessageResult
    {
        public int PageNumber { set; get; }
        public int PageSize { set; get; }
        public int Counter { set; get; }
        public int Total { set; get; }

        public bool Ok { set; get; }
        public string MessageText { set; get; }

        public MessageResult()
        {
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

    public enum SENDER_TYPE
    {
        /// <summary>
        /// no response because sender was hidden
        /// </summary>
        HIDE_SENDER, // NO_RESPONSE
        IS_FORM,
        IS_JOB,
    }

    public enum MESSAGE_ACTION
    {
        ITEM_ADD_NEW,
        ITEM_EDIT,
        ITEM_REMOVE,
        ITEM_SEARCH,
    }

    public enum MESSAGE_TYPE
    {
        REQUEST,
        RESPONSE,
    }

    public class Message
    {
        readonly Guid Id = Guid.Empty;
        readonly int SenderId;
        readonly int[] JobReceiveID;
        readonly SENDER_TYPE SenderType;
        readonly MESSAGE_ACTION Action;
        public MESSAGE_TYPE Type { set; get; }
        public string JobName { set; get; }

        public MessageResult Output { set; get; }
        public object Input { set; get; }

        public Message(int senderId, int[] job_receive_IDs, MESSAGE_ACTION action, object input = null, SENDER_TYPE senderType = SENDER_TYPE.IS_FORM)
        {
            this.Type = MESSAGE_TYPE.REQUEST;
            Action = action;
            SenderType = senderType;
            SenderId = senderId;
            JobReceiveID = job_receive_IDs;

            Id = Guid.NewGuid();
            Output = new MessageResult();
            Input = input;
        }
        
        public Guid GetMessageId()
        {
            return Id;
        }

        public int GetSenderId()
        {
            return SenderId;
        }

        public int[] GetReceiverId()
        {
            return JobReceiveID;
        }

        public MESSAGE_ACTION getAction()
        {
            return Action;
        }
        public SENDER_TYPE getSenderType()
        {
            return SenderType;
        }
    }

}
