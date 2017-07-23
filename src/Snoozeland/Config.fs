namespace Snoozeland

module Config =

    open System.IO
    open FSharp.Configuration

    // Let the type provider do it's work
    type Config = YamlConfig<"commands.default.yaml">

    type SerialData = string

    type Command = {
        name : string
        payload : SerialData
    }

    type DeviceCommand = {
        deviceName : string
        command : Command
    }

    let loadConfig fileName = 
        let config = Config()
        let file = Path.Combine(Directory.GetCurrentDirectory(), fileName)
        if not (File.Exists(file)) then 
            printfn "Creating File: %s" file
            File.Create(file) |> ignore

        printfn "Loading File: %A" file
        config.Load(file)
        config