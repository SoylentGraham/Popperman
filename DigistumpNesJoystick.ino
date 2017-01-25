#include "DigiJoystick.h"


int LatchPin = 2;
int ClockPin = 5;
int DataPin[2] = { 0, 1 };

void setup() 
{
  pinMode( LatchPin, OUTPUT );
  pinMode( ClockPin, OUTPUT );
  pinMode( DataPin[0], INPUT );
  pinMode( DataPin[1], INPUT );
}

uint8_t GetPadButtons(int PadIndex)
{
  uint8_t Button = 0;

  #define wait delayMicroseconds(12)

#define latchlow digitalWrite( LatchPin, LOW)
#define latchhigh digitalWrite( LatchPin, HIGH)
#define clocklow digitalWrite( ClockPin, LOW)
#define clockhigh digitalWrite( ClockPin, HIGH)
#define dataread(pin) digitalRead(pin)

 latchlow;
  clocklow;
  latchhigh;
  wait;
  latchlow;
 
  for (int i = 0; i < 8; i++)
  {
    bool Down = dataread(DataPin[PadIndex]) ? 0 : 1;
     Button |= Down << i;
     clockhigh;
     wait;
     clocklow;
     wait;
  }

  return Button;
}

void loop() {
  // If not using plentiful DigiJoystick.delay() calls, make sure to
  //DigiJoystick.update(); // call this at least every 50ms
  // calling more often than that is fine
  // this will actually only send the data every once in a while unless the data is different
  
  DigiJoystick.setX( (byte)0xf0 );
  DigiJoystick.setY( (byte)0xf0 );
  DigiJoystick.setXROT( (byte)0xf0 );
  DigiJoystick.setYROT( (byte)0xf0 );
  DigiJoystick.setZROT( (byte)0xf0 );
  DigiJoystick.setSLIDER( (byte)0xf0 );


  uint16_t Button16 = 0;

  Button16 |= GetPadButtons(0) << 0;
  Button16 |= GetPadButtons(1) << 8;

   // we can also set buttons like this (lowByte, highByte)
   uint8_t Button07 = Button16 & 0xff;
   uint8_t Button815 = Button16 >> 8;
  DigiJoystick.setButtons( Button07, Button815 );

  // it's best to use DigiJoystick.delay() because it knows how to talk to
  // the connected computer - otherwise the USB link can crash with the 
  // regular arduino delay() function
  DigiJoystick.delay(50); // wait 50 milliseconds
  
 }

