using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace core2
{
    [ProtoContract]
    public class mAddress
    {
        [ProtoMember(1)]
        public string Line1 { get; set; }
        [ProtoMember(2)]
        public string Line2 { get; set; }
    }

    [ProtoContract]
    public class mPerson
    {
        [ProtoMember(1)]
        public int Id { get; set; }

        [ProtoMember(2)]
        public string Name { set; get; }

        [ProtoMember(3)]
        public mAddress Address { get; set; }

        [ProtoMember(4)]
        public string Email { get; set; }

    }

    public class demo_protobuf
    {
        public static void run()
        {
            //var person = new mPerson
            //{
            //    Id = 12345,
            //   // Name = "Fred",
            //    Address = new mAddress
            //    {
            //        Line1 = "Flat 1",
            //        Line2 = "The Meadows"
            //    }
            //};
            //var person2 = new mPerson
            //{
            //    Id = 12345,
            //    //Name = "Nguyễn Văn Thịnh",
            //    Address = new mAddress
            //    {
            //        Line1 = "Flat 1",
            //        Line2 = "The Meadows"
            //    }
            //};

            //using (var file = File.Create("person.bin"))
            //{
            //    Serializer.Serialize<mPerson[]>(file,new mPerson[] { person, person2 });
            //}

            List<mPerson> newPerson;
            using (var file = File.OpenRead("person.bin"))
            {
                newPerson = Serializer.Deserialize<List<mPerson>>(file);
            }


        }//
    }

}
