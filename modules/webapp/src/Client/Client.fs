module Client

open Elmish
open Elmish.React

open Fable.Helpers.React
open Fable.Helpers.React.Props
open Fable.PowerPack.Fetch
open Fable.Import
open Fable.Core
open Fable.Core.JsInterop

open Thoth.Json

open Shared
open System.Drawing

open Fulma
open Fable.Import.React


// The model holds data that you want to keep track of while the application is running
// in this case, we are keeping track of a counter
// we mark it as optional, because initially it will not be available from the client
// the initial value will be requested from server
type Model = { ComponentModel: ComponentModel option }

// The Msg type defines what events/actions can occur while the application is running
// the state of the application changes *only* in reaction to these events
type Msg =
| NextPlaylist
| PreviousPlaylist
| AddPlaylist
| CreatePlaylist
| InitialPlaylist of Result<ComponentModel, exn>

let initialComponentModel = fetchAs<ComponentModel> "/api/init" (Decode.Auto.generateDecoder())

// defines the initial state and initial command (= side-effect) of the application
let init () : Model * Cmd<Msg> =
    let initialModel = { ComponentModel = None }
    let loadCountCmd =
        Cmd.ofPromise
            initialComponentModel
            []
            (Ok >> InitialPlaylist)
            (Error >> InitialPlaylist)
    initialModel, loadCountCmd



// The update function computes the next state of the application based on the current state and the incoming events/messages
// It can also run side-effects (encoded as commands) like calling the server via Http.
// these commands in turn, can dispatch messages to which the update function will react.
let update (msg : Msg) (currentModel : Model) : Model * Cmd<Msg> =
    match currentModel.ComponentModel, msg with
    (* 
    | Some counter, Increment ->
        let nextModel = { currentModel with ComponentModel = Some { Value = counter.Value + 1 } }
        nextModel, Cmd.none
    | Some counter, Decrement ->
        let nextModel = { currentModel with ComponentModel = Some { Value = counter.Value - 1 } }
        nextModel, Cmd.none
        *)
    | _, InitialPlaylist (Ok initialModel)->
        let nextModel = { ComponentModel = Some initialModel }
        nextModel, Cmd.none

    | _ -> currentModel, Cmd.none

let inline react_responsive_music_player (playlist : ComponentModel seq) : ReactElement = 
    ofImport "defualt" "react-responsive-music-player" (keyValueList CaseRules.LowerFirst playlist) []

let safeComponents =
    (* 
    let components =
        span [ ]
           [
             a [ Href "https://saturnframework.github.io" ] [ str "Saturn" ]
             str ", "
             a [ Href "http://fable.io" ] [ str "Fable" ]
             str ", "
             a [ Href "https://elmish.github.io/elmish/" ] [ str "Elmish" ]
             str ", "
             a [ Href "https://mangelmaxime.github.io/Fulma" ] [ str "Fulma" ]
           ]
    
    p [ ]
        [ strong [] [ str "SAFE Template" ]
          str " powered by: "
          components ]
    *)
    let playlist = 
        {
            Autoplay = true
            ProgressColor = "#000000"
            ButtonColor = "#000000"
            PlayList = List.Empty
        }

    div [ ]
        [ react_responsive_music_player [ playlist ] ]

let show = function
| { ComponentModel = Some componentModel } -> string componentModel
| { ComponentModel = None   } -> "Loading..."

let button txt onClick =
    Button.button
        [ Button.IsFullWidth
          Button.Color IsPrimary
          Button.OnClick onClick ]
        [ str txt ]

let view (model : Model) (dispatch : Msg -> unit) =
    div []
        [ Navbar.navbar [ Navbar.Color IsPrimary ]
            [ Navbar.Item.div [ ]
                [ Heading.h2 [ ]
                    [ str "SAFE Template" ] ] ]

          Container.container []
              [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ Heading.h3 [] [ str ("Press buttons to manipulate counter: " + show model) ] ]
                Columns.columns []
                    [ Column.column [] [ button "-" (fun _ -> dispatch PreviousPlaylist) ]
                      Column.column [] [ button "+" (fun _ -> dispatch NextPlaylist) ] ] ]

          Footer.footer [ ]
                [ Content.content [ Content.Modifiers [ Modifier.TextAlignment (Screen.All, TextAlignment.Centered) ] ]
                    [ safeComponents ] ] ]

#if DEBUG
open Elmish.Debug
open Elmish.HMR
#endif

Program.mkProgram init update view
#if DEBUG
|> Program.withConsoleTrace
|> Program.withHMR
#endif
|> Program.withReact "elmish-app"
#if DEBUG
|> Program.withDebugger
#endif
|> Program.run
