using System;
using System.Collections.Generic;
using System.Text;

namespace GameLibrary
{
    public class Client
    {
        public String Username { get; set; }
        public readonly Guid UniqueMatchMakingIdentifer;

        public Client(String Username, Guid UniqueMatchMakingIdentifer)
        {
            this.Username = Username;
            this.UniqueMatchMakingIdentifer = UniqueMatchMakingIdentifer;
        }
    }
}
