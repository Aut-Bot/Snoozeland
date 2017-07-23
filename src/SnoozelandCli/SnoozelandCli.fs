namespace SnoozelandCli

open Snoozeland.IrReader
open Snoozeland.IrSender
open Snoozeland.Config
open System
open System.Reflection
open Argu
open FSharp.Configuration

module Snoozeland = 

    type ShowDeviceCommandsArgs = 
        | DeviceName of deviceName:string
    with 
        interface IArgParserTemplate with 
            member x.Usage = 
                match x with 
                | DeviceName _ -> "name of device to list exist commands for"

    type AddDevice = 
        | DeviceName of deviceName:string
    with 
        interface IArgParserTemplate with 
            member x.Usage = 
                match x with 
                | DeviceName _ -> "name of device to add"
    
    type Record = 
        | DeviceName of deviceName:string
    with 
        interface IArgParserTemplate with 
            member x.Usage = 
                match x with
                | DeviceName _ -> "name of device to record commands for"

    type Transmit =
        | DeviceName of deviceName:string
        | CommandName of commandName:string
    with 
        interface IArgParserTemplate with 
            member x.Usage = 
                match x with 
                | DeviceName _ -> "name of device to transmit to"
                | CommandName _ -> "name of command to transmit"

    and CLIArguments = 
        | [<Mandatory; Inherit>]Config of configFilePath:string
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-s")>] ShowDeviceCommands of ParseResults<ShowDeviceCommandsArgs>
        | [<AltCommandLine("-g")>]GetIRStartCmd
        | SetIRStartCmd of irStartCmd:string
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-a")>] AddDevice of ParseResults<AddDevice>
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-r")>] Record of ParseResults<Record>
        | [<CliPrefix(CliPrefix.None); AltCommandLine("-t")>] Transmit of ParseResults<Transmit>
        | Version 
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with
                | Config _ -> "specify the configuration file."
                | ShowDeviceCommands _ -> "show all recorded commands for a specific device."
                | GetIRStartCmd _ -> "show start command for IR Transmitter."
                | SetIRStartCmd _ -> "set start command for IR Transmitter."
                | AddDevice _ -> "add a new device to the device registry."
                | Record _ -> "set program in record mode."
                | Transmit _ -> "set program in tranmit mode."
                | Version _ -> "show exe version information."

    let errorHandler = ProcessExiter(colorizer = function ErrorCode.HelpText -> None | _ -> Some ConsoleColor.White)
    let parser = ArgumentParser.Create<CLIArguments>(programName = "Snoozeland.exe", errorHandler = errorHandler)
    let transmitSubParser = parser.GetSubCommandParser <@ Transmit @>
    let recordSubParser = parser.GetSubCommandParser <@ Record @>
    let addDeviceSubParser = parser.GetSubCommandParser <@ AddDevice @>
    let showDeviceCommandsSubParser = parser.GetSubCommandParser <@ ShowDeviceCommands @>

    let showDeviceCommands (results:ParseResults<ShowDeviceCommandsArgs>) (config:Config) =
        let deviceName = results.GetResult <@ ShowDeviceCommandsArgs.DeviceName @>
        match config.configuration.devices |> Seq.isEmpty with 
        | true -> 
            sprintf "Device %s is not configured." deviceName 
        | false ->
            // list device commands
            let deviceCmds = 
                config.configuration.devices 
                |> Seq.filter (fun x -> x.name = deviceName)
                |> Seq.collect(fun x -> x.commands)
                |> Seq.map (fun x -> sprintf "%s:%s" x.name x.commandString)
                |> Seq.fold (+) (Environment.NewLine)
            sprintf "%s" deviceCmds
        |> ignore

    let setIRStartCmd (cmd:string) (config:Config) = 
        config.configuration.emitterStartSequence <- cmd
        config.Save()

    let getIrStartCmd (config:Config) =
        printfn "%s" <| config.configuration.emitterStartSequence

    let addDevice (results:ParseResults<AddDevice>) (config:Config) = 
        let deviceName = results.GetResult <@ AddDevice.DeviceName @>
        let mutable device = Config.configuration_Type.devices_Item_Type()
        device.name <- deviceName
        config.configuration.devices.Add(device)

    let recordDevice (results:ParseResults<Record>) (config:Config) = 
        let deviceName = results.GetResult <@ Record.DeviceName @>
        Snoozeland.IrReader.recordDeviceCommands config deviceName

    let transmitDevice (results:ParseResults<Transmit>) (config:Config) =
        let deviceName = results.GetResult <@ Transmit.DeviceName @>
        let commandName = results.GetResult <@ Transmit.CommandName @>

        let command = 
            config.configuration.devices
            |> Seq.filter (fun x -> x.name = deviceName)
            |> Seq.collect (fun x -> x.commands)
            |> Seq.find (fun x -> x.name = commandName)
        let startCommand = config.configuration.emitterStartSequence
        Snoozeland.IrSender.runCommand startCommand <| command.commandString

    [<EntryPoint>]
    let main args = 
        
        let results = parser.ParseCommandLine(raiseOnUsage=true)

        let version = results.Contains <@ Version @> 
        if not version then
         
            let config = results.GetResult <@ Config @> |> loadConfig

            match results.GetSubCommand() with 
            | ShowDeviceCommands x ->  showDeviceCommands x config 
            | GetIRStartCmd -> getIrStartCmd config
            | SetIRStartCmd x -> setIRStartCmd x config
            | AddDevice x -> addDevice x config
            | Record x -> recordDevice x config
            | Transmit x -> transmitDevice x config
            // globals
            | Version
            | Config _ -> failwith "This code should never be reached"

            |> ignore

        printfn "%s" <| Assembly.GetExecutingAssembly().GetName().Version.ToString()

        0