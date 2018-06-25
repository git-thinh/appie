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

        //public Message(IJob sender, object input = null, string JOB_NAME_RECEIVER = null)
        //{
        //    SenderId = sender.f_getId();
        //    if (string.IsNullOrEmpty(JOB_NAME_RECEIVER))
        //        ReceiverId = new int[] { JOB_NAME.JOB_TARGET_FORM_UI_RECEIVER_ID };
        //    else
        //        ReceiverId = sender.StoreJob.f_job_getIdsByName(JOB_NAME_RECEIVER);

        //    Id = Guid.NewGuid();
        //    Output = new MessageResult();
        //    Input = input;
        //}

        //public Message(IJob sender, string messageText, string JOB_NAME_RECEIVER = null)
        //{
        //    SenderId = sender.f_getId();
        //    if (string.IsNullOrEmpty(JOB_NAME_RECEIVER))
        //        ReceiverId = new int[] { JOB_NAME.JOB_TARGET_FORM_UI_RECEIVER_ID };
        //    else
        //        ReceiverId = sender.StoreJob.f_job_getIdsByName(JOB_NAME_RECEIVER);

        //    Id = Guid.NewGuid();
        //    Output = new MessageResult();
        //    Output.MessageText = messageText;
        //}

        //public Message(IJob sender, bool success, object data = null, bool send_to_UI = false, string JOB_NAME_RECEIVER = null)
        //{
        //    SenderId = sender.f_getId();
        //    if (string.IsNullOrEmpty(JOB_NAME_RECEIVER))
        //        ReceiverId = new int[] { JOB_NAME.JOB_TARGET_FORM_UI_RECEIVER_ID };
        //    else
        //        ReceiverId = sender.StoreJob.f_job_getIdsByName(JOB_NAME_RECEIVER);

        //    Id = Guid.NewGuid();
        //    Output = new MessageResult();
        //    Output.Ok = success;
        //    Output.SetData(data);
        //}

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
