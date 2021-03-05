using System.Collections.Generic;
using System;

namespace listening_party_server.Models {

    public class Room {
        public Guid RoomID { get; set; }
        public bool IsProtected { get; set; }
        public string Password { get; set; }
        public User Host { get; set; }
        public List<User> Guests { get; set; }
    }

}