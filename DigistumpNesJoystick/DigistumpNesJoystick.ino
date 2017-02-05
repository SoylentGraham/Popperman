//  digistump board manager
//  http://digistump.com/package_digistump_index.json
#include "DigiJoystick.h"

//  gr: order of Data,Data,LatchClock rendered data2 not working... this order is okay
//    maybe something funny about digispark pins...
int LatchPin = 0;
int ClockPin = 1;
int DataPin[2] = { 2, 5 };

void setup() 
{
  pinMode( LatchPin, OUTPUT );
  pinMode( ClockPin, OUTPUT );
  pinMode( DataPin[0], INPUT );
  pinMode( DataPin[1], INPUT );
}

void GetPadButtons(uint8_t& Buttons0,uint8_t& Buttons1)
{
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
    //  data is low when grounded (button pressed), so reversed
    bool Down0 = dataread(DataPin[0]) ? 0 : 1;
    bool Down1 = dataread(DataPin[1]) ? 0 : 1;
    Buttons0 |= Down0 << i;
    Buttons1 |= Down1 << i;
    
    
    clockhigh;
    wait;
    clocklow;
    wait;
  }
}

void loop() {
  // If not using plentiful DigiJoystick.delay() calls, make sure to
  //DigiJoystick.update(); // call this at least every 50ms
  // calling more often than that is fine
  // this will actually only send the data every once in a while unless the data is different
  
  DigiJoystick.setX( (char)0 );
  DigiJoystick.setY( (char)0 );
  DigiJoystick.setXROT( (char)0 );
  DigiJoystick.setYROT( (char)0 );
  DigiJoystick.setZROT( (char)0 );
  DigiJoystick.setSLIDER( (char)0 );

  uint8_t PadButtons0 = 0;
  uint8_t PadButtons1 = 0;
  GetPadButtons( PadButtons0, PadButtons1 );
  DigiJoystick.setButtons( PadButtons0, PadButtons1 );
  
  // it's best to use DigiJoystick.delay() because it knows how to talk to
  // the connected computer - otherwise the USB link can crash with the 
  // regular arduino delay() function
  DigiJoystick.delay(50); // wait 50 milliseconds
  
 }

