/*
Commands:
turn ON/OFF X -> turns ON relay X
toggle X -> toggles the state of relay X: if it is on, it will be off, and viceversa
pulse X Y [Z] -> turns on relay X for Y milliseconds Z times -> if Z is not present, pulse indifinitely

get X -> returns the state of X
get relays -> returns the number of relays
getinfo X -> returns detailed info about relay X: state, mode, physical pin



relays are indexed from 1 and not 0;
TRUE/1 can be used in place of ON
FALSE/0 can be used in place of OFF.
ALL can be used as index to apply the command to all relays

*/


//Relay setup
const uint8_t firstPin = 2;      // Pin connected to Relay 1
const uint8_t nRelays = 8;       // Number of relays connected consecutively to Relay 1 inclusive
const bool invertOutput = true;  // Set to true to invert NC/NO

//Serial setup
const long baud = 57600;  // Data speed (baud) (Default = 9600)

//Command format setup
const uint8_t comSize = 64;   // Max size for commands (Default = buffer sized)
const char endMarker = '\n';  // Termination marker for commands (Default = new line)

//Global variables
enum Mode { TURN = 0,
            TOGGLE,
            PULSE,
            GET,
            GETINFO,
            RESTART };
const char* modeStr[] = { "TURN", "TOGGLE", "PULSE", "GET", "GETINFO", "RESTART" };
enum Action { RELAY_ON,
              RELAY_OFF,
              RELAY_ALL,
              RELAY_SINGLE,
              RELAY_PULSING,
              ERROR };
const uint8_t nModes = 6;
void (*resetFunc)(void) = 0;  //Resets the board

struct Pulse {
  unsigned long timer = 0;
  unsigned long duration = 0;
  int countdown = 0;
};



Mode currMode[nRelays];   // Mode of operation of the relays
Pulse pulse[nRelays];     //Pulse control structures
char command[comSize];    // Next command in line
char args[4][64];         // Command arguments
bool newCommand = false;  // Stores if there's a new command ready to be read
bool ledAvailable;        // Stores if the onboard LED pin is used by relays. If not, the LED will display processing state.

volatile byte state = LOW;

void setup() {

  // Relay/pins startup
  for (uint8_t i = firstPin; i < firstPin + nRelays; i++) {
    pinMode(i, OUTPUT);
    digitalWrite(i, invertOutput);
  }

  for (uint8_t i = 0; i < nRelays; i++) {
    currMode[i] = TOGGLE;
  }

  ledAvailable = (LED_BUILTIN >= (firstPin + nRelays)) || (LED_BUILTIN < firstPin);
  if (ledAvailable) {
    pinMode(LED_BUILTIN, OUTPUT);
    digitalWrite(LED_BUILTIN, LOW);
  }


  //Serial startup
  Serial.begin(baud);
  while (!Serial) {
    // waits for USB Serial to connect
  }

  //Flush serial buffer
  while (Serial.available() > 0) {
    Serial.read();
  }

  //Init command mem
  for (uint8_t i = 0; i < comSize; i++)
    command[i] = '\0';

  //Make sure args are empty
  for (uint8_t i = 0; i < 4; i++)
    for (uint8_t j = 0; j < 64; j++)
      args[i][j] = '\0';
}



void loop() {
  recvWithEndMarker();

  // State check/timers
  for (uint8_t i = 0; i < nRelays; i++) {
    if (currMode[i] == PULSE) {  // If it's pulsing, do the checks
      if (pulse[i].timer <= millis()) {
        //Change state
        bool state = !getRelay(i);
        setRelay(i, state);

        if (state == false) {  // If period is over, set up next one
          switch (pulse[i].countdown) {
            case 0:
              {
                pulse[i].timer = 0;
                pulse[i].duration = 0;
                currMode[i] = TOGGLE;
              }
              break;
            default:
              {
                pulse[i].countdown--;
              }
            case -1:
              {
                pulse[i].timer += pulse[i].duration;
              }
              break;
          }
        } else {
          pulse[i].timer += pulse[i].duration;
        }
      }
    }
  }


  // command processing
  if (newCommand) {
    setLed(true);
    strupr(command);  // Change to uppercase

    //Decode
    char* part;

    part = strtok(command, " ");

    byte nArgs = 0;
    while (part != NULL && nArgs < 4) {
      strcpy(args[nArgs++], part);
      part = strtok(NULL, " ");
    }

    uint8_t opcode = 0xFF;
    for (uint8_t i = 0; i < nModes && opcode == 0xFF; i++) {
      if (strcmp(args[0], modeStr[i]) == 0) opcode = i;
    }


    // Execute
    switch (opcode) {
      case TURN:
        {
          if (nArgs < 3) {
            Serial.println("!ERR: not enough arguments");
            break;
          }

          // ON or OFF?
          Action act = 0xFF;

          if (strcmp(args[1], "ON") == 0 || strcmp(args[1], "TRUE") == 0 || strcmp(args[1], "1") == 0)
            act = RELAY_ON;
          if (strcmp(args[1], "OFF") == 0 || strcmp(args[1], "FALSE") == 0 || strcmp(args[1], "0") == 0)
            act = RELAY_OFF;

          if (act == 0xFF) {
            Serial.print("!ERR: ");
            Serial.print(args[1]);
            Serial.println(" is not a valid action, was expecting [ON,OFF]");
            break;
          }

          // Which relay?
          if (strcmp(args[2], "ALL") == 0) {  //If ALL relays
            for (uint8_t i = 0; i < nRelays; i++) {
              currMode[i] = TOGGLE;
              setRelay(i, (act == RELAY_ON));
            }

          } else {
            int relay = atoi(args[2]);
            if (relay < 1 || relay > nRelays) {  //If relay couldn't be parsed or was invalid
              Serial.print("!ERR: ");
              Serial.print(args[2]);
              Serial.print(" is not a valid index, was expecting [1-");
              Serial.print(nRelays);
              Serial.println("]");
              break;
            }
            currMode[--relay] = TOGGLE;  //TOGGLE and TURN are actually the same mode
            setRelay(relay, (act == RELAY_ON));
          }
          Serial.println("OK");
        }
        break;
      case TOGGLE:
        {
          if (nArgs < 2) {
            Serial.println("!ERR: not enough arguments");
            break;
          }

          // Which relay?
          if (strcmp(args[1], "ALL") == 0) {  //If ALL relays
            for (uint8_t i = 0; i < nRelays; i++) {
              currMode[i] = TOGGLE;
              setRelay(i, !getRelay(i));
            }

          } else {
            int relay = atoi(args[1]);
            if (relay < 1 || relay > nRelays) {  //If relay couldn't be parsed or was invalid
              Serial.print("!ERR: ");
              Serial.print(args[1]);
              Serial.print(" is not a valid index, was expecting [1-");
              Serial.print(nRelays);
              Serial.println("]");
              break;
            }
            currMode[--relay] = TOGGLE;
            setRelay(relay, !getRelay(relay));
          }
          Serial.println("OK");
        }
        break;
      case PULSE:
        {
          if (nArgs < 3) {
            Serial.println("!ERR: not enough arguments");
            break;
          }

          int relay = (strcmp(args[1], "ALL") == 0) ? -1 : atoi(args[1]);  //-1 == ALL, 0 == ERR, Rest = possibly valid
          if (relay < -1 || relay == 0 || relay > nRelays) {
            Serial.print("!ERR: ");
            Serial.print(args[1]);
            Serial.print(" is not a valid index, was expecting [1-");
            Serial.print(nRelays);
            Serial.println("]");
            break;
          }

          unsigned long duration = strtoul(args[2], NULL, 0);
          if (duration == 0) {
            Serial.print("!ERR: ");
            Serial.print(args[2]);
            Serial.print(" is not a valid duration");
            break;
          }

          int count = -1;
          if (nArgs > 3) {  //If there is a count
            count = atoi(args[3]);

            if (count == 0 || count < -1) {
              Serial.print("!ERR: ");
              Serial.print(args[3]);
              Serial.print(" is not a valid count");
              break;
            }
            count--;
          }


          if (relay != -1) {  // If only one relay
            relay--;          //transform into 0-based index
            currMode[relay] = PULSE;
            pulse[relay].duration = duration;
            pulse[relay].countdown = count;
            pulse[relay].timer = millis() + duration;
            setRelay(relay, true);
            Serial.println("OK");
            break;
          } else {  // If ALL relays
            for (uint8_t i = 0; i < nRelays; i++) {
              currMode[i] = PULSE;
              pulse[i].duration = duration;
              pulse[i].countdown = count;
              pulse[i].timer = millis() + duration;
              setRelay(i, true);
            }
            Serial.println("OK");
            break;
          }
        }
        break;
      case GET:
        {
          // Which relay?
          if (strcmp(args[1], "ALL") == 0) {  //If ALL relays
            Serial.print("[");
            for (uint8_t i = 0; i < nRelays - 1; i++) {
              Serial.print("RLA");
              Serial.print(i + 1);
              if (getRelay(i)) Serial.print("=ON,");
              else Serial.print("=OFF,");
            }
            Serial.print("RLA");
            Serial.print(nRelays);
            if (getRelay(nRelays - 1)) Serial.println("=ON]");
            else Serial.println("=OFF]");
            break;


          } if (strcmp(args[1], "RELAYS") == 0) { //If asking for number of relays
            Serial.println(nRelays);
            break;
          }

            int relay = atoi(args[1]);
            if (relay < 1 || relay > nRelays) {  //If relay couldn't be parsed or was invalid
              Serial.print("!ERR: ");
              Serial.print(args[1]);
              Serial.print(" is not a valid index, was expecting [1-");
              Serial.print(nRelays);
              Serial.println("]");
              break;
            }
            Serial.print("[RLA");
            Serial.print(relay);
            if (getRelay(relay - 1)) Serial.println("=ON]");
            else Serial.println("=OFF]");
          
        }
        break;
      case GETINFO:
        {
          // Which relay?
          if (strcmp(args[1], "ALL") == 0) {  //If ALL relays
            Serial.print("[");
            for (uint8_t i = 0; i < nRelays - 1; i++) {
              Serial.print("RLA");
              Serial.print(i + 1);
              //Board pin
              Serial.print("=[pin=D");
              Serial.print(i + firstPin);
              //Relay state
              Serial.print(",state=");
              if (getRelay(i)) Serial.print("ON,mode=");
              else Serial.print("OFF,mode=");
              //Mode
              Serial.print(modeStr[currMode[i]]);
              if (currMode[i] == PULSE) {
                Serial.print(",duration=");
                Serial.print(pulse[i].duration);
                Serial.print(",countdown=");
                Serial.print(pulse[i].countdown);
                Serial.print(",nextChange=");
                Serial.print(pulse[i].timer - millis());
              }
              Serial.print("],");
            }

            Serial.print("RLA");
            Serial.print(nRelays);
            //Board pin
            Serial.print("=[pin=D");
            Serial.print(firstPin + nRelays - 1);
            //Relay state
            Serial.print(",state=");
            if (getRelay(nRelays - 1)) Serial.print("ON,mode=");
            else Serial.print("OFF,mode=");
            //Mode
            Serial.print(modeStr[currMode[nRelays - 1]]);
            if (currMode[nRelays - 1] == PULSE) {
              Serial.print(",duration=");
              Serial.print(pulse[nRelays - 1].duration);
              Serial.print(",countdown=");
              Serial.print(pulse[nRelays - 1].countdown);
              Serial.print(",nextChange=");
              Serial.print(pulse[nRelays - 1].timer - millis());
            }
            Serial.println("]]");

          } else {  // Just one
            int relay = atoi(args[1]);
            if (relay < 1 || relay > nRelays) {  //If relay couldn't be parsed or was invalid
              Serial.print("!ERR: ");
              Serial.print(args[1]);
              Serial.print(" is not a valid index, was expecting [1-");
              Serial.print(nRelays);
              Serial.println("]");
              break;
            }

            Serial.print("[RLA");
            Serial.print(relay--);
            //Board pin
            Serial.print("=[pin=D");
            Serial.print(firstPin + relay);
            //Relay state
            Serial.print(",state=");
            if (getRelay(relay)) Serial.print("ON,mode=");
            else Serial.print("OFF,mode=");
            //Mode
            Serial.print(modeStr[currMode[relay]]);
            if (currMode[relay] == PULSE) {
              Serial.print(",duration=");
              Serial.print(pulse[relay].duration);
              Serial.print(",countdown=");
              Serial.print(pulse[relay].countdown);
              Serial.print(",nextChange=");
              Serial.print(pulse[relay].timer - millis());
            }
            Serial.println("]]");
          }
        }
        break;
      case RESTART:
        {
          Serial.println("OK");
          delay(100);  // To make sure the receipt gets send
          resetFunc();
        }
        break;
      default:
        Serial.print("!ERR: Opcode 0x");
        Serial.print(opcode, HEX);
        Serial.println(" unknown");
    }

    // Flags to fetch new command
    nextCommand();
    setLed(false);
  }
}


void recvWithEndMarker() {  // Serial reader, use newCommand = false to ask for next line.
  static byte ndx = 0;
  char rc;

  while (Serial.available() > 0 && newCommand == false) {
    rc = Serial.read();

    if (rc != endMarker) {
      command[ndx] = toupper(rc);
      ndx++;
      if (ndx >= comSize) {
        ndx = comSize - 1;
      }
    } else {
      command[ndx] = '\0';  // terminate the string
      ndx = 0;
      newCommand = true;
    }
  }
}

void setLed(bool state) {  // Uses LED if pin available
  if (ledAvailable) digitalWrite(LED_BUILTIN, state);
}

void setRelay(uint8_t relay, bool state) {  //Sets relay state, taking invertOutput into consideration

  bool realState = invertOutput ? !state : state;  // check if it should flip High/Low

  if (realState) {
    digitalWrite(relay + firstPin, HIGH);
  } else {
    digitalWrite(relay + firstPin, LOW);
  }
}

bool getRelay(uint8_t relay) {  // Reads relay state, taking invertOutput into consideration
  if (digitalRead(relay + firstPin) == LOW) return invertOutput;
  return !invertOutput;
}

void nextCommand() {  // Clears command and arguments and gets ready for next command
  for (uint8_t i = 0; i < comSize; i++) {
    command[i] = '\0';
  }
  for (uint8_t i = 0; i < 4; i++)
    for (uint8_t j = 0; j < 64; j++)
      args[i][j] = '\0';
  newCommand = false;
}