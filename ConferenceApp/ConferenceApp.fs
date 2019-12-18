// Copyright 2018-2019 Fabulous contributors. See LICENSE.md for license.
namespace ConferenceApp

open Fabulous
open Fabulous.XamarinForms
open Fabulous.XamarinForms.LiveUpdate
open Xamarin.Forms
open System.IO
open System.Reflection
open ConferenceApp.Services.XamExpertDay

module App = 
    type Model = 
      { SelectedTrack: Track option
        Speakers: Speaker List
        Tracks: Track List }

    type Msg = 
        | TrackSelected of int option

    let initModel speakers (tracks:Track List) = { SelectedTrack = None; Speakers = speakers; Tracks = tracks }

    let loadFile filename =
        let assembly = IntrospectionExtensions.GetTypeInfo(typedefof<Model>).Assembly;
        let stream = assembly.GetManifestResourceStream(filename);
        use streamReader = new StreamReader(stream)
        streamReader.ReadToEnd()


    let init () =
        let html = loadFile("ConferenceApp.ExpertXamarin.html")
        //let xamExpertHtml = (loadFile "ConferenceApp.ExpertXamarin.html")
        let speakers = (getSpeakers html) |> Seq.toList
        let tracks = (getTracks html) |> Seq.toList
        // test code for live update desiging
        //let speakers = [{Id = "1"; Name = "Harvey Specter"; Photo = "https://external-content.duckduckgo.com/iu/?u=https%3A%2F%2Fqph.fs.quoracdn.net%2Fmain-qimg-84da307035ee6477d0943dfd9fe2c7dc-c&f=1&nofb=1"; Tagline = "Senior Partner"}]
        //let tracks = [{Room = "Room 1"; Time = "08:00 - 09:00"; Title = "Intro"; SpeakerId = None}; {Room = "Room 1"; Time = "09:00 - 10:00"; Title = "Being Awesome"; SpeakerId = Some "1"}]
        (initModel speakers tracks), Cmd.none

    let update msg model =
        match msg with
        | TrackSelected trackIndex -> match trackIndex with
                                        | Some indx -> { model with SelectedTrack = (Some model.Tracks.[indx]) }, Cmd.none
                                        | None -> { model with SelectedTrack = None }, Cmd.none
    let showTrackInfo track (model:Model) dispatch =
        let speaker = match track.SpeakerId with
                      | Some speakerId -> model.Speakers |> Seq.tryPick(fun s -> if s.Id = speakerId then Some s else None)
                      | None -> None

        let addSpeakerInfo (speaker:Speaker) =
            View.StackLayout(margin = Thickness(0.,32.,0.,0.), children = [
                    View.Label (text = "Speaker", fontSize = FontSize 22. )
                    View.Image (source = (Image.Path speaker.Photo))
                    View.Label (text = "Presenter: " + speaker.Name)
                    View.Label (text = "Tagline: " + speaker.Tagline)
                ])
            
        let speakerViewElements = match (speaker |> Option.map addSpeakerInfo) with
                                  | Some speakerInfo -> speakerInfo
                                  | None -> View.Label(text = "Brought to you by the Organizers");

        View.Grid (margin = Thickness(8.,8.,8.,16.),
                    rowdefs = [Star; Auto],
                    children = [
                        View.StackLayout(children = [
                            View.Label (text = track.Title, fontSize = FontSize 22.)
                            View.Label (text = "In: " + track.Room, fontSize = FontSize 14.)
                            View.Label (text = "At: " + track.Time, fontSize = FontSize 14., margin = Thickness(0.,-4.,0.,0.))
                            speakerViewElements
                            ])
                        (View.Button (text = "Back", command = (fun () -> dispatch (TrackSelected None)))).Row(1)
                    ])

    let showTrackCell track =
        View.ViewCell( view =
            View.StackLayout(children = [
                View.Label (text = track.Title, 
                            fontSize = FontSize 22.)
                View.Label (text = track.Time + " in " + track.Room, 
                            fontSize = FontSize 14.,
                            fontAttributes = FontAttributes.Italic)
                ]))

    let view (model: Model) dispatch =

        View.ContentPage(
            content = match model.SelectedTrack with 
                        | Some track -> showTrackInfo track model dispatch
                        | None -> View.ListView(
                                        rowHeight = 80,
                                        hasUnevenRows = true,
                                        margin = Thickness(8.,0.,0.,0.),
                                        items = (model.Tracks |> List.map showTrackCell),
                                        selectionMode = ListViewSelectionMode.Single,
                                        itemSelected = (fun args -> dispatch (TrackSelected args))
                                        )
            )

    // Note, this declaration is needed if you enable LiveUpdate
    let program = Program.mkProgram init update view

type App () as app = 
    inherit Application ()

    let runner = 
        App.program
#if DEBUG
        |> Program.withConsoleTrace
#endif
        |> XamarinFormsProgram.run app

#if DEBUG
    // Uncomment this line to enable live update in debug mode. 
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/tools.html#live-update for further  instructions.
    //
    //do runner.EnableLiveUpdate()
#endif    

    // Uncomment this code to save the application state to app.Properties using Newtonsoft.Json
    // See https://fsprojects.github.io/Fabulous/Fabulous.XamarinForms/models.html#saving-application-state for further  instructions.
#if APPSAVE
    let modelId = "model"
    override __.OnSleep() = 

        let json = Newtonsoft.Json.JsonConvert.SerializeObject(runner.CurrentModel)
        Console.WriteLine("OnSleep: saving model into app.Properties, json = {0}", json)

        app.Properties.[modelId] <- json

    override __.OnResume() = 
        Console.WriteLine "OnResume: checking for model in app.Properties"
        try 
            match app.Properties.TryGetValue modelId with
            | true, (:? string as json) -> 

                Console.WriteLine("OnResume: restoring model from app.Properties, json = {0}", json)
                let model = Newtonsoft.Json.JsonConvert.DeserializeObject<App.Model>(json)

                Console.WriteLine("OnResume: restoring model from app.Properties, model = {0}", (sprintf "%0A" model))
                runner.SetCurrentModel (model, Cmd.none)

            | _ -> ()
        with ex -> 
            App.program.onError("Error while restoring model found in app.Properties", ex)

    override this.OnStart() = 
        Console.WriteLine "OnStart: using same logic as OnResume()"
        this.OnResume()
#endif


