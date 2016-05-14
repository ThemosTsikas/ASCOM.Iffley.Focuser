// Iffley Focuser Arduino code

// A LED is connected to pin 13 (already on Arduino board)
int ledPin = 13;

// These are the pins that are used to drive the unipolar
// stepper motor (via the Darlington array), indexed by 1-4
byte coilPins[] = {
  -1,3,5,6,9};

// variable that keeps track of the position of the focuser.
// this is reported back to the port after every received 
// command
unsigned int position;

// the maximum position supported by this sketch
const unsigned int maxposition = 65535u;

// phases encodes the sequence of steps and half-steps
// first we turn on the first coil, then first and second, 
// then second etc
// in binary: 0001 0011 0010 0110 0100 1100 1000 1001
byte phases[]={  
  1,3,2,6,4,12,8,9};

// we do this when the serial port is opened
void setup()
{
  // set the mode of the pins we use
  pinMode(ledPin,OUTPUT);
  pinMode(coilPins[1],OUTPUT);
  pinMode(coilPins[2],OUTPUT);
  pinMode(coilPins[3],OUTPUT);
  pinMode(coilPins[4],OUTPUT);
  // flash the LED in a particular sequence: Morse _ _ . . . 
  ledflash(200);    
  ledflash(200);
  ledflash(100); 
  ledflash(100); 
  ledflash(100); 
  // opens serial port, sets data rate to 19200 bps
  Serial.begin(19200);	
  // send the identification
  Serial.println("Iffley Focuser Version 2.0");
  // set the position to zero
  position = 0;
  // set the coils to phase corresponding to position
  phaserinit(position);
  // send the position (in decimal) as a string
  Serial.println(position,DEC);
}

// keep doing this until reset
void loop() 
{
  // variable for the byte we read off the serial port
  int incomingByte;
  
  // this is a busy loop
  // most times there will be nothing available
  if (Serial.available()>0) {
    // something has arrived, turn on the LED
    ledon();
    // read the byte
    incomingByte = Serial.read();
    // check for known values
    switch(incomingByte) {
    case 'f':
    // a command to move forward, do it and update the position
      position = forward(position);
      break;
    case 'b':
    // a command to move backward, do it and update the position
      position = backward(position);
      break;
    case 'z':
    // a command to reset, set phase to 0 and update the position
      position = 0;
      phaserinit(position);
      break;
    }
    // now ALWAYS report the position
    Serial.println(position,DEC); 
    // turn the LED off
    ledoff();
  }
}

// the next two functions keep position in [0,maxposition]
// move forward and return the new position
unsigned int forward(unsigned int pos)
{
  // variable for new position
  unsigned int newposition;
  // add one but don't allow overshoot
  newposition = (pos==maxposition)?maxposition:pos+1;
  // set the phase 
  phaser(pos,newposition);
  // return the new position
  return(newposition);
}

unsigned int backward(unsigned int pos)
{
  unsigned int newposition;
  // subtract one but don't allow negative
  newposition = (pos==0)?0:pos-1;
  // set the phase
  phaser(pos,newposition);
  // return the new position
  return(newposition);
}

// turn the LED on
void ledon()
{
  digitalWrite(ledPin, HIGH);
}

// turn the LED off
void ledoff()
{  
  digitalWrite(ledPin, LOW);
}

// flash the LED for a certain time (in 1/1000th seconds)
void ledflash(int time)
{
  ledon();
  delay(time);
  ledoff();
  delay(time);
}

// set the coil phase corresponding to a position [0,maxposition]
void phaserinit(unsigned int pos)
{ 
  byte x1, x2, x3, x4;
  // divide the position by 8 and take the remainder ( pos % 8)
  // that is the phase, use it to index the array phases to 
  // tell which coils should be on
  // the & operation picks the correct bit
  // 1 if 1st coil should be ON, 0 otherwise
  x1 = phases[pos %8] & 1;
  // 2 if 2nd coil should be ON, 0 otherwise
  x2 = phases[pos %8] & 2;
  // 4 if 3rd coil should be ON, 0 otherwise
  x3 = phases[pos %8] & 4;
  // 8 if 4th coil should be ON, 0 otherwise
  x4 = phases[pos %8] & 8;
  // set the pins accordingly
  digitalWrite(coilPins[1],x1==1?HIGH:LOW);
  digitalWrite(coilPins[2],x2==2?HIGH:LOW);
  digitalWrite(coilPins[3],x3==4?HIGH:LOW);
  digitalWrite(coilPins[4],x4==8?HIGH:LOW);
}

// like phaserinit but only touch the pins that change
void phaser(unsigned int from, unsigned int to)
{
  byte will1,will2,will3,will4,was1,was2,was3,was4;
  from = from % 8;
  to = to % 8;
  will1 = phases[to] & 1;
  will2 = (phases[to] & 2);
  will3 = (phases[to] & 4);
  will4 = (phases[to] & 8);
  was1 = phases[from] & 1;
  was2 = (phases[from] & 2);
  was3 = (phases[from] & 4);
  was4 = (phases[from] & 8);
  // if the new setting is different then change the pin
  if (was1 != will1) digitalWrite(coilPins[1], will1==1?HIGH:LOW);
  if (was2 != will2) digitalWrite(coilPins[2], will2==2?HIGH:LOW);
  if (was3 != will3) digitalWrite(coilPins[3], will3==4?HIGH:LOW);
  if (was4 != will4) digitalWrite(coilPins[4], will4==8?HIGH:LOW);
}





















