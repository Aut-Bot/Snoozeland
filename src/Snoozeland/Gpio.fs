namespace Snoozeland

open System
open System.Runtime.InteropServices

module Gpio = 


  module private imp =
    [<DllImport( "libwiringPi.so", EntryPoint="pinMode", CallingConvention = CallingConvention.Cdecl, SetLastError=true )>]
    extern void pinMode( int pin, int mode );

    [<DllImport( "libwiringPi.so", EntryPoint="digitalWrite", CallingConvention = CallingConvention.Cdecl, SetLastError=true )>]
    extern void digitalWrite( int pin, int state );

    [<DllImport( "libwiringPi.so", EntryPoint="digitalRead", CallingConvention = CallingConvention.Cdecl, SetLastError=true )>]
    extern int digitalRead( int pin );

    [<DllImport( "libwiringPi.so", EntryPoint="wiringPiSetup", CallingConvention = CallingConvention.Cdecl, SetLastError=true )>]
    extern int wiringPiSetup();

    [<DllImport( "libwiringSerial.so", EntryPoint="serialOpen", CallingConvention = CallingConvention.Cdecl, SetLastError=true )>]
    extern int serialOpen(string port, int baudRate);

    [<DllImport( "libwiringSerial.so", EntryPoint="serialPutchar", CallingConvention = CallingConvention.Cdecl, SetLastError=true )>]
    extern void serialPutchar(int fileDescriptor, char data);

    [<DllImport( "libwiringSerial.so", EntryPoint="serialDataAvail", CallingConvention = CallingConvention.Cdecl, SetLastError=true )>]
    extern int serialDataAvail(int fileDescriptor);

    [<DllImport( "libwiringSerial.so", EntryPoint="serialGetchar", CallingConvention = CallingConvention.Cdecl, SetLastError=true )>]
    extern int serialGetchar(int fileDescriptor)
     
     

  type PinMode =
    | In
    | Out

  type PinState =
    | High
    | Low

  type PinId =
    | Gpio14 = 15
    | Wipi15 = 15
    | Pin8 = 15
    | Gpio23 = 4
    | Wipi4 = 4
    | Pin16 = 4
    | Gpio24 = 5
    | Wipi5 = 5
    | Pin18 = 5
    | Gpio18 = 1
    | Wipi1 = 1
    | Pin12 = 1
    | Gpio8 = 10
    | Wipi10 = 10
    | Pin24 = 10

  [<Literal>]
  let SerialPort = "/dev/ttyS0"

  [<Literal>]
  let BaudRate = 9600

  let setupWiringPi = 
    imp.wiringPiSetup()

  let serialOpen SerialPort BaudRate = 
    imp.serialOpen(SerialPort, BaudRate)

  let serialPutchar fileDescriptor data = 
    imp.serialPutchar (fileDescriptor, data)

  let serialGetChar fileDescriptor = 
    imp.serialGetchar(fileDescriptor)

  let serialDataAvail fileDescriptor = 
    imp.serialDataAvail(fileDescriptor)

  let pinMode (pin:PinId) mode =
    match mode with
    | In -> imp.pinMode( int pin, 0 )
    | Out -> imp.pinMode( int pin, 1 )

  let digitalWrite (pin:PinId) state =
    match state with
    | High -> imp.digitalWrite( int pin, 1 )
    | Low -> imp.digitalWrite( int pin, 0 )
    
  let digitalRead (pin: PinId) = 
    imp.digitalRead ( int pin )