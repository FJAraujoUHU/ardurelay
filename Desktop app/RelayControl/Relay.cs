using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;

namespace RelayControl
{
    public class Relay
    {
        public enum Mode
        {
            TURN = 0,
            TOGGLE,
            PULSE,
            GET,
            GETINFO,
            RESTART
        };

        public string name { get; set; }
        public bool state { get; set; } = false;
        public Mode mode { get; set; } = Mode.TOGGLE ;
        public ulong pulseDuration { get; set; } = 0;
        public ulong pulseTimer { get; set; } = 0;
        public int pulseCountdown { get; set; } = 0;

        public Relay(int num) { this.name = "RLA" + num; }

        public Relay(string name, bool state, Mode mode, ulong pulseDuration, ulong pulseTimer, int pulseCountdown)
        {
            this.name = name;
            this.state = state;
            this.mode = mode;
            this.pulseDuration = pulseDuration;
            this.pulseTimer = pulseTimer;
            this.pulseCountdown = pulseCountdown;
        }

        public Relay(string fromGetInfo)
        {
            this.Update(fromGetInfo);
        }

        public void Update(string fromGetInfo)
        {
            // If it's as-is from serial, remove the brackets
            string aux = (fromGetInfo.ElementAt(0) == '[') ? fromGetInfo.Substring(1, fromGetInfo.Length - 2) : fromGetInfo;
            this.name = aux.Substring(0, aux.IndexOf('='));

            List<string> states = new List<string>();

            foreach (Match m in Regex.Matches(aux, @"=([^[,\]]*[^[,\]])"))
            {
                states.Add(m.Groups[1].Value);
            }

            this.state = states[1].ToUpper().Equals("ON");

            switch (states[2].ToUpper())
            {
                case "TURN":
                    this.mode = Mode.TURN; break;
                case "TOGGLE":
                    this.mode = Mode.TOGGLE; break;
                case "PULSE":
                    this.mode = Mode.PULSE; break;
                case "GET":
                    this.mode = Mode.GET; break;
                case "GETINFO":
                    this.mode = Mode.GETINFO; break;
                case "RESTART":
                    this.mode = Mode.RESTART; break;
                default:
                    this.mode = Mode.TOGGLE; break;
            }

            if (this.mode == Mode.PULSE && states.Count == 6)
            {
                this.pulseDuration = ulong.Parse(states[3]);
                this.pulseCountdown = int.Parse(states[4]);
                this.pulseTimer = ulong.Parse(states[5]);
            }
            else
            {
                this.pulseDuration = 0;
                this.pulseCountdown = 0;
                this.pulseTimer = 0;
            }
        }




    }
}
