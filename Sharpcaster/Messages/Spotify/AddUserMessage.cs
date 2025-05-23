﻿using System.Runtime.Serialization;

namespace Sharpcaster.Messages.Spotify
{
    [DataContract]
    public class AddUserMessage : MessageWithId
    {
        [DataMember(Name = "payload")]
        public AddUserMessagePayload Payload { get; set; }
        public AddUserMessage()
        {
            Type = "addUser";
        }
    }

    [DataContract]
    public class AddUserMessagePayload
    {
        [DataMember(Name = "blob")]
        public string Blob { get; set; }
        [DataMember(Name = "tokenType")]
        public string TokenType { get; set; }
    }
}
