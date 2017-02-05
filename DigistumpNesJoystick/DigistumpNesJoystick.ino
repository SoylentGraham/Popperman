//  https://github.com/MHeironimus/ArduinoJoystickLibrary/
#include <Joystick.h>

//  digistump board manager
//  http://digistump.com/package_digistump_index.json
//#include "DigiJoystick.h"

//  gr: order of Data,Data,LatchClock rendered data2 not working... this order is okay
//    maybe something funny about digispark pins...
int LatchPin = 20;
int ClockPin = 21;
#define PAD_COUNT 8
int DataPin[PAD_COUNT] = { 2, 3, 4, 5, 6, 7, 8, 9 };

#define NES_BUTTON_COUNT 8
#define JOYSTICK_BUTTON_COUNT 4

Joystick_ Joystick[PAD_COUNT] = {
  Joystick_(0x03, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, true, false, false, false, false, false, false, false, false, false),
  Joystick_(0x04, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, true, false, false, false, false, false, false, false, false, false),
  Joystick_(0x05, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, true, false, false, false, false, false, false, false, false, false),
  Joystick_(0x06, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, true, false, false, false, false, false, false, false, false, false),
  Joystick_(0x07, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, true, false, false, false, false, false, false, false, false, false),
  Joystick_(0x08, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, true, false, false, false, false, false, false, false, false, false),
  Joystick_(0x09, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, true, false, false, false, false, false, false, false, false, false),
  Joystick_(0x0a, JOYSTICK_TYPE_GAMEPAD, JOYSTICK_BUTTON_COUNT, 0, true, true, false, false, false, false, false, false, false, false, false),
};

#define AXIS_MIN  -100
#define AXIS_MID  0
#define AXIS_MAX  100

void setup() 
{
  for ( int p=0;  p<PAD_COUNT;  p++)
  {
    auto& Pad = Joystick[p];
    Pad.begin(false);
    Pad.setXAxisRange( AXIS_MIN, AXIS_MAX );
    Pad.setYAxisRange( AXIS_MIN, AXIS_MAX );
  }
    
  pinMode( LatchPin, OUTPUT );
  pinMode( ClockPin, OUTPUT );
  for ( int i=0;  i<PAD_COUNT;  i++ )
    pinMode( DataPin[i], INPUT );
 }

void GetPadButtons(uint8_t Buttons[PAD_COUNT])
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
  
  for ( int p=0;  p<PAD_COUNT;  p++ )
  {
    Buttons[p] = 0;;
  }

  //  8 nes buttons
  for (int i = 0; i <NES_BUTTON_COUNT; i++)
  {
    //  data is low when grounded (button pressed), so reversed
    for ( int p=0;  p<PAD_COUNT;  p++ )
    {
      uint8_t Down = dataread(DataPin[p]) ? 0 : 1;
      Buttons[p] |= Down << i;
    }
    
    clockhigh;
    wait;
    clocklow;
    wait;
  }

  //  if they're ALL on, the pad isn't connected (none grounded)
  uint8_t AllButtons = 0xff;
  for ( int p=0;  p<PAD_COUNT;  p++ )
  {
    if ( Buttons[p] != AllButtons )
      continue;
    Buttons[p] = 0;
  }
  
}


#define NES_BUTTON_UP(btns) ( ((btns) & (1<<4)) != 0 )
#define NES_BUTTON_DOWN(btns) ( ((btns) & (1<<5)) != 0 )
#define NES_BUTTON_LEFT(btns) ( ((btns) & (1<<6)) != 0 )
#define NES_BUTTON_RIGHT(btns) ( ((btns) & (1<<7)) != 0 )

void loop() {
  // If not using plentiful DigiJoystick.delay() calls, make sure to
  //DigiJoystick.update(); // call this at least every 50ms
  // calling more often than that is fine
  // this will actually only send the data every once in a while unless the data is different
  /*
  DigiJoystick.setX( (char)0 );
  DigiJoystick.setY( (char)0 );
  DigiJoystick.setXROT( (char)0 );
  DigiJoystick.setYROT( (char)0 );
  DigiJoystick.setZROT( (char)0 );
  DigiJoystick.setSLIDER( (char)0 );
*/

  uint8_t PadButtons[PAD_COUNT];
  GetPadButtons( PadButtons );
/*
  uint8_t Buttons07 = 0;
  uint8_t Buttons816 = 0;

  for ( int p=0;  p<PAD_COUNT;  p++)
    Buttons07 |= PadButtons[p];
  
  DigiJoystick.setButtons( Buttons07, Buttons816 );
  
  // it's best to use DigiJoystick.delay() because it knows how to talk to
  // the connected computer - otherwise the USB link can crash with the 
  // regular arduino delay() function
  DigiJoystick.delay(50); // wait 50 milliseconds
  */
  for ( int p=0;  p<PAD_COUNT;  p++)
  {
    auto& Pad = Joystick[p];
    auto& NesButtons = PadButtons[p];

    int16_t x = AXIS_MID;
    int16_t y = AXIS_MID;
    if ( NES_BUTTON_LEFT(NesButtons) )
      x = AXIS_MIN;
    if ( NES_BUTTON_RIGHT(NesButtons) )
      x = AXIS_MAX;
    if ( NES_BUTTON_UP(NesButtons) )
      y = AXIS_MIN;
    if ( NES_BUTTON_DOWN(NesButtons) )
      y = AXIS_MAX;
      
    Pad.setXAxis( x );
    Pad.setYAxis( y );
    for ( int b=0;  b<JOYSTICK_BUTTON_COUNT; b++ )
    {
      uint8_t Down = PadButtons[p] & (1<<b);
      Pad.setButton( b, Down );
    }
    Pad.sendState();
  }


    delay(100);
 }

