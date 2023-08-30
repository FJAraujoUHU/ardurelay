using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO.Ports;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RelayControl
{
    public class RelayController
    {
        private SerialPort port;
        private bool connected = false;
        public List<Relay> relays { get; }
        private static readonly string ackSign = Properties.Settings.Default.AckSign;


        public RelayController()
        {
            port = new SerialPort();
            port.BaudRate = Properties.Settings.Default.BaudRate;
            port.Parity = Properties.Settings.Default.Parity;
            port.DataBits = Properties.Settings.Default.DataBits;
            port.StopBits = Properties.Settings.Default.StopBits;
            port.Handshake = Properties.Settings.Default.Handshake;
            port.ReadTimeout = Properties.Settings.Default.Timeout;
            port.WriteTimeout = Properties.Settings.Default.Timeout;
            relays = new List<Relay>();
        }

        public RelayController(string comPort) : this()
        {
            this.Connect(comPort);
        }

        public void Connect(string comPort)
        {
            if (relays.Count > 0) relays.Clear();
            port.PortName = comPort;
            port.Open();
            port.WriteLine("GET RELAYS");
            int nRelays = int.Parse(port.ReadLine());
            
            for (int i = 0; i < nRelays; i++)
            {
                relays.Add(new Relay(i + 1));
            }
            this.Poll();

            connected = true;
        }

        public void Disconnect()
        {
            port.Close();
            connected = false;
        }

        public bool IsConnected()
        {
            return connected;
        }

        public void Poll()
        {
            port.WriteLine("GETINFO ALL");
            string info = port.ReadLine();

            MatchCollection relayInfo = Regex.Matches(info, @"[^[,\[][^]]*[\]]");

            for (int i = 0; i < relayInfo.Count; i++)
            {
                relays[i].Update(relayInfo[i].Value);
            }

        }

        public string Turn(bool on)   // ALL
        {
            string state = on ? "ON" : "OFF";
            port.WriteLine(String.Format("TURN {0} ALL", state));

            return CheckAck();
        }

        public string Turn(int relay, bool on)
        {
            if (relay == -1) return this.Turn(on); // -1 is alias for ALL

            string state = on ? "ON" : "OFF";
            port.WriteLine(String.Format("TURN {0} {1}", state, relay));

            return CheckAck();
        }



        public string Toggle()
        {
            port.WriteLine("TOGGLE ALL");

            return CheckAck();
        }

        public string Toggle(int relay)
        {
            if (relay == -1) return this.Toggle();

            port.WriteLine(String.Format("TOGGLE {0}", relay));

            return CheckAck();
        }



        public string Pulse(uint duration)
        {
            port.WriteLine(String.Format("PULSE ALL {0}", duration));

            return CheckAck();
        }

        public string Pulse(int relay, uint duration)
        {
            if (relay == -1) return this.Pulse(duration);

            port.WriteLine(String.Format("PULSE {0} {1}", relay, duration));

            return CheckAck();
        }

        public string Pulse(uint duration, int count)
        {
            port.WriteLine(String.Format("PULSE ALL {0} {1}", duration, count));

            return CheckAck();
        }

        public string Pulse(int relay, uint duration, int count)
        {
            if (relay == -1) return this.Pulse(duration, count);

            port.WriteLine(String.Format("PULSE {0} {1} {2}", relay, duration, count));

            return CheckAck();
        }

        public string Execute(string command)
        {
            port.WriteLine(command);
            return CheckAck();
        }



        private string CheckAck()
        {
            String ack = port.ReadLine().Trim('\n', '\r').ToUpper();

            if (!ack.Equals(ackSign))
            {
                if (ack.StartsWith("!ERR"))
                {
                    ack = ack.Remove(0, 5).Trim();
                }
                throw new CommandNotAcknowledgedException(ack);
            }
            return ack;
        }

    }

    public class CommandNotAcknowledgedException : Exception
    {
        public CommandNotAcknowledgedException() { }
        public CommandNotAcknowledgedException(string message) : base(message) { }
        public CommandNotAcknowledgedException(string message, Exception innerException) : base(message, innerException)
        {
        }
        protected CommandNotAcknowledgedException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
