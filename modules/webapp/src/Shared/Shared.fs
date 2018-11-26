namespace Shared
open Fable.Import.Browser
open Fulma
open System.Drawing

type Artist = { Name : string }

type ArtistArray = seq<Artist>

type Song = {
    URL : URL
    Cover : URL
    Title : string
    Artist : ArtistArray
}

type ComponentModel = {
    Autoplay : bool
    ProgressColor : string
    ButtonColor : string
    PlayList : seq<Song> 
    //Style : object of html inline styling for component 
}

(*
autoplay bool false
progressColor string #66cccc the color of the progress
btnColor string #4a4a4a the color of the buttons
playlist array [] the playlist
style object {}

const playlist = [
  {
    url: 'path/to/mp3',
    cover: 'path/to/jpg',
    title: 'Despacito',
    artist: [
      'Luis Fonsi',
      'Daddy Yankee'
    ]
  },
  {
    url: 'path/to/mp3',
    cover: 'path/to/jpg',
    title: 'Bedtime Stories',
    artist: [
      'Jay Chou'
    ]
  }
]
*)