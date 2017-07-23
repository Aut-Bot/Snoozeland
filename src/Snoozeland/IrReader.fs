namespace Snoozeland

module IrReader = 

    open Gpio
    open System.IO
    open System
    open System.Threading
    open System.Collections.Generic
    open Config


    [<Literal>]
    let GetStartString = "----------Start----------"
    [<Literal>]
    let GetStopString = "-----------End-----------\n"

    let ticksToMicroseconds ticks = 
        ticks / (TimeSpan.TicksPerMillisecond / 1000L);

    let setUpPi = 
        printfn "%i" Gpio.setupWiringPi
        Gpio.serialOpen (Gpio.SerialPort) (Gpio.BaudRate)

    let deviceExists (config:Config) deviceName = 
        config.configuration.devices
        |> Seq.exists (fun x -> x.name = deviceName)

    let commandExists (config:Config) (deviceCommand:DeviceCommand) =
        config.configuration.devices
        |> Seq.filter (fun x -> x.name = deviceCommand.deviceName)
        |> Seq.collect (fun x -> x.commands)
        |> Seq.exists (fun x -> x.name = deviceCommand.command.name)

    let saveCommand config (deviceCommand:DeviceCommand)  =
        if commandExists config deviceCommand then 
            printfn "adding command"
            let mutable newCommand = Config.configuration_Type.devices_Item_Type.commands_Item_Type()
            newCommand.name <- deviceCommand.command.name
            newCommand.commandString <- deviceCommand.command.payload
            let device = 
                config.configuration.devices
                |> Seq.find (fun x -> x.name = deviceCommand.deviceName)  
            device.commands.Add(newCommand) 
        else  
            printfn "updating command"
            let device = 
                config.configuration.devices
                |> Seq.find (fun x -> x.name = deviceCommand.deviceName)
            device.commands 
            |> Seq.filter (fun x -> x.name = deviceCommand.command.name)
            |> Seq.iter (fun x -> x.commandString <- deviceCommand.command.payload)

        config.Save()
        config

    let irRecord fileDescriptor deviceName commandName = 
        let command = 
            [1..3]
            |> List.map (fun x -> 
                while (Gpio.serialDataAvail(fileDescriptor)) <= 0 do
                    Thread.Sleep(200)
                String.Format("{0:X2}", Gpio.serialGetChar(fileDescriptor)) 
            )
            |> List.fold (+) ""
        
        {deviceName = deviceName; 
         command = {name = commandName; 
                    payload = command}}    
    let recordDeviceCommands config deviceName =
        let fileDescriptor = setUpPi 
        let record = irRecord fileDescriptor deviceName
        
        // device loop
        let rec userRecord config = 
            //command loop
            printfn "Enter a new command name, or `exit` to exit"
            let commandName = Console.ReadLine()
            match commandName.ToLowerInvariant() with
            | "exit" -> ()
            | _ -> 
                let cfg = record commandName |> saveCommand config
                userRecord cfg 
        
        userRecord config 


