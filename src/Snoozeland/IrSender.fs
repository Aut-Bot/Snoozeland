namespace Snoozeland

module IrSender = 

    open Gpio
    open System
    open System.Collections
    open System.IO
    open Config
    

    let intToBinStr (num:int) = Convert.ToString(num, 2)
    let inputWire = PinId.Pin18

    let setUpPi = 
        printfn "%i" Gpio.setupWiringPi
        Gpio.serialOpen (Gpio.SerialPort) (Gpio.BaudRate)

    let readCommandFile (filePath:string) =
        seq {
            use sr = new StreamReader (filePath)
            while not sr.EndOfStream do
                yield sr.ReadLine ()
        } 

    let fromHex (s:string) = 
        s 
        |> Seq.chunkBySize 2
        |> Seq.map (fun x -> 
            let str = String(x)
            Int32.Parse(str , System.Globalization.NumberStyles.HexNumber)
        )

    let sendData fileDescriptor data = 
        data
        |> Seq.iter (fun x -> Gpio.serialPutchar fileDescriptor (char x))

    let runCommand startCommand command = 
        let fileDescriptor = setUpPi
        startCommand + command |> fromHex |> sendData fileDescriptor

    let start (config:Config) = 
        let fileDescriptor = setUpPi
        let startSequence = 
            config.configuration.emitterStartSequence

        let powerOn = 
            let device = 
                config.configuration.devices
                |> Seq.find (fun x -> x.name = "deviceId")
            startSequence + (device.commands |> Seq.find (fun x -> x.name = "powerOn")).commandString
            
        let powerOff = 
            let device = 
                config.configuration.devices
                |> Seq.find (fun x -> x.name = "deviceId")
            startSequence + (device.commands |> Seq.find (fun x -> x.name = "powerOff")).commandString
            
        let commands = 
            let device = 
                config.configuration.devices
                |> Seq.find (fun x -> x.name = "deviceId")
            device.commands
            |> Seq.filter (fun x -> 
                x.name <> "powerOn" && 
                x.name <> "powerOff")

        while true do
            
            powerOn |> fromHex |> sendData fileDescriptor
            Threading.Thread.Sleep(1000)
            commands
            |> Seq.iter (fun x -> 
                
                let cmd = startSequence + x.commandString
                printfn "%A%A" x.name cmd 
                cmd 
                |> fromHex 
                |> sendData fileDescriptor
                Threading.Thread.Sleep(3000)
            )
            Threading.Thread.Sleep(3000)
            powerOff |> fromHex |> sendData fileDescriptor
            Threading.Thread.Sleep(3000)