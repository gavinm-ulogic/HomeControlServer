using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using HomeControlServer.Providers;

namespace HomeControlServer.Models
{
    public class Relay : IComparable<Relay>
    {
        public string name { get; set; }
        public string address { get; set; }

        public Relay()
        {
        }

        public Relay(string name, string address)
        {
            this.name = name;
            this.address = address;
        }

        public int CompareTo(Relay other)
        {
            // If other is not a valid object reference, this instance is greater.
            if (other == null) return 1;

            return this.address.CompareTo(other.address);
        }

        public int state
        {
            get
            {
                return (RelayControl.GetRelayState(this.address)) ? 1 : 0 ;
            }
            set { ;}
        }
    }
}