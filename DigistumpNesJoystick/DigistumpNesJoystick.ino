//  https://github.com/MHeironimus/ArduinoJoystickLibrary/
#include <Joystick.h>
#include "pitches.h" 


//  digistump board manager
//  http://digistump.com/package_digistump_index.json
//#include "DigiJoystick.h"

//  gr: order of Data,Data,LatchClock rendered data2 not working... this order is okay
//    maybe something funny about digispark pins...
#define LatchPin  20
#define ClockPin  21
#define LedPin    13  //  nano led =13
#define SpeakerPin   10

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

#define NES_BUTTON_UP(btns) ( ((btns) & (1<<4)) != 0 )
#define NES_BUTTON_DOWN(btns) ( ((btns) & (1<<5)) != 0 )
#define NES_BUTTON_LEFT(btns) ( ((btns) & (1<<6)) != 0 )
#define NES_BUTTON_RIGHT(btns) ( ((btns) & (1<<7)) != 0 )
#define NES_BUTTON_SELECT(btns) ( ((btns) & (1<<2)) != 0 )
#define NES_BUTTON_START(btns) ( ((btns) & (1<<3)) != 0 )
uint8_t LastNesButtons[PAD_COUNT] = {0,0,0,0,0,0,0,0};
uint8_t LastPadConnected[PAD_COUNT] = {false,false,false,false,false,false,false,false};


void EnableLed(bool Enable)
{
 digitalWrite( LedPin, Enable ? HIGH : LOW );
}

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


  pinMode( SpeakerPin, OUTPUT );
  //EnableLed(false);

  PlayMarioUnderworldIntroSound(SpeakerPin);
  delay(400);
 }

void PlayMarioUnderworldIntroSound(int Pin)
{
  int Tempo = 130;
  tone(Pin,NOTE_C4,Tempo);
  delay(Tempo);
  tone(Pin,NOTE_C5,Tempo);
  delay(Tempo);
  tone(Pin,NOTE_A3,Tempo);
  delay(Tempo);
  tone(Pin,NOTE_A4,Tempo);
  delay(Tempo);
  tone(Pin,NOTE_AS3,Tempo);
  delay(Tempo);
  tone(Pin,NOTE_AS4,Tempo);
  delay(Tempo);
  
  noTone(Pin);
}

void PlayCoinSound(int Pin)
{
   // Play coin sound   
  tone(Pin,NOTE_B5,100);
  delay(100);
  tone(Pin,NOTE_E6,850);
  delay(850);
  noTone(Pin);
}

void Play1UpSound(int Pin)
{
  // Play 1-up sound
  tone(Pin,NOTE_E6,125);
  delay(130);
  tone(Pin,NOTE_G6,125);
  delay(130);
  tone(Pin,NOTE_E7,125);
  delay(130);
  tone(Pin,NOTE_C7,125);
  delay(130);
  tone(Pin,NOTE_D7,125);
  delay(130);
  tone(Pin,NOTE_G7,125);
  delay(125);
  noTone(Pin);
}

void PlayFireballSound(int Pin)
{
  // Play Fireball sound
  tone(Pin,NOTE_G4,35);
  delay(35);
  tone(Pin,NOTE_G5,35);
  delay(35);
  tone(Pin,NOTE_G6,35);
  delay(35);
  noTone(Pin);
}
 
void GetPadButtons(uint8_t Buttons[PAD_COUNT],bool Connected[PAD_COUNT])
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
    Connected[p] = ( Buttons[p] != AllButtons );
    if ( Connected[p] )
      continue;
    Buttons[p] = 0;
  }
  
}




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
  bool PadConnected[PAD_COUNT];
  GetPadButtons( PadButtons, PadConnected );
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
  bool PlayFireball = false;
  bool Play1Up = false;

  for ( int p=0;  p<PAD_COUNT;  p++)
  {
    auto& Pad = Joystick[p];
    auto& NesButtons = PadButtons[p];
    auto NesButtonsPressed = NesButtons & ( NesButtons ^ LastNesButtons[p] );
    LastNesButtons[p] = NesButtons;

    if ( !LastPadConnected[p] && PadConnected[p] )
      Play1Up = true;
    LastPadConnected[p] = PadConnected[p];
    
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

    PlayFireball |= NES_BUTTON_SELECT( NesButtonsPressed );
    //Play1Up |= NES_BUTTON_START( NesButtons );
    
    Pad.setXAxis( x );
    Pad.setYAxis( y );
    for ( int b=0;  b<JOYSTICK_BUTTON_COUNT; b++ )
    {
      uint8_t Down = PadButtons[p] & (1<<b);
      Pad.setButton( b, Down );
    }
    Pad.sendState();
  }


    if ( PlayFireball )
    {
      PlayFireballSound(SpeakerPin);
    }
    else if ( Play1Up )
    {
      Play1UpSound(SpeakerPin);
      //PlayCoinSound(SpeakerPin);
    }
    else
    {
      delay(100);
    }
 
      
 }

  

