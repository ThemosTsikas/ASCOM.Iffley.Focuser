// Iffley Focuser Arduino code

#include <AccelStepper.h>

// A LED is connected to pin 13 (already on Arduino board)
int ledPin = 13;

// These are the pins that are used to drive the unipolar
// stepper motor (via the Darlington array), indexed by 1-4
#define Coil1 3
#define Coil2 5
#define Coil3 6
#define Coil4 9
byte coilPins[] = {255, Coil1, Coil2, Coil3, Coil4};

// Define a stepper and the pins it will use
AccelStepper stepper(AccelStepper::HALF4WIRE, Coil1, Coil3, Coil2, Coil4, true);
// Defaults to AccelStepper::FULL4WIRE (4 pins) on 2, 3, 4, 5

// 62.63 mm  43.03mm on Meade5000 HALF4WIRE for 10000 steps
// 19.600 mm , 1.96 microns per step
unsigned long position, speed, accel, target;

void setup()
{
  // set the mode of the pins we use
  pinMode(ledPin, OUTPUT);
  pinMode(coilPins[1], OUTPUT);
  pinMode(coilPins[2], OUTPUT);
  pinMode(coilPins[3], OUTPUT);
  pinMode(coilPins[4], OUTPUT);
  // flash the LED in a particular sequence: Morse _ _ . . .
  ledflash(100);
  ledflash(100);
  ledflash(50);
  ledflash(50);
  ledflash(50);
  // opens serial port, sets data rate to 19200 bps
  Serial.begin(19200);
  // send the identification
  Serial.println("Iffley Focuser Version 4.0");
  position = stepper.currentPosition();
  stepper.setMaxSpeed(600.0);
  speed = stepper.maxSpeed();
  stepper.setAcceleration(100.0);
  accel = 100.0;
}

void loop()
{
  // variable for the byte we read off the serial port
  int incomingByte;
 
  stepper.run();
  // this is a busy loop
  // most times there will be nothing available
  if (Serial.available() > 0) {
    // something has arrived, turn on the LED
    ledon();
    // read the byte
    incomingByte = Serial.read();
    // check for known values
    switch (incomingByte) {
      case 's':
        // read speed
        speed = Serial.parseInt();
        stepper.setMaxSpeed((float)speed);
        break;
      case 'a':
        //read accel
        accel = Serial.parseInt();
        stepper.setAcceleration((float)accel);
        break;
      case '?':
        // query, return maxspeed, maxaccel, current Pos, target Pos
        Serial.println(speed, DEC);
        Serial.println(accel, DEC);
        Serial.println(stepper.currentPosition(), DEC);
        Serial.println(stepper.targetPosition(), DEC);
        break;
      case 't':
        // set target
        target = stepper.currentPosition();
        target = Serial.parseInt();
        stepper.moveTo(target);
        break;
      case 'h':
        // stop
        stepper.stop();
        break;
      case 'z':
        //reset to zero, only if not moving
        if (stepper.distanceToGo() == 0) {
          stepper.setCurrentPosition(0);
        }
        break;
    }
  }
  ledoff();
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

