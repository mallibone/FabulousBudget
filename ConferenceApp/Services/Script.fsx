#r @"/Users/mallibone/Work/Playground/ConferenceApp/packages/FSharp.Data.3.3.2/lib/net45/FSharp.Data.dll"
open FSharp.Data

let expertDay = HtmlDocument.Load("/Users/mallibone/Downloads/ExpertXamarin.html")

type speaker = {Id:string; Name:string; Photo:string; Tagline:string}
type track = {Room:string; Time:string; Title:string; SpeakerId:string option}

let GetName (htmlNode:HtmlNode) =
    htmlNode.CssSelect("h3.sz-speaker__name > a") |> Seq.map (fun h -> h.DirectInnerText()) |> Seq.head

let GetPhoto (htmlNode:HtmlNode) =
    htmlNode.Descendants["img"] |> Seq.map (fun h -> h.AttributeValue("src")) |> Seq.head

let GetTagline (htmlNode:HtmlNode) =
    htmlNode.CssSelect("h4.sz-speaker__tagline") |> Seq.map (fun h -> h.DirectInnerText()) |> Seq.head

let GetId (htmlNode:HtmlNode) =
    htmlNode.Attribute("data-speakerid").Value()

let speakers = expertDay.CssSelect("li.sz-speaker")
                                |> Seq.map (fun s -> {Id = (GetId s); Name = (GetName s); Photo = (GetPhoto s); Tagline = (GetTagline s)})


    
let GetRoom (htmlNode:HtmlNode) =
    htmlNode.CssSelect("div.sz-session__room") 
        |> Seq.map (fun h -> h.DirectInnerText()) 
        |> Seq.head

let GetTime (htmlNode:HtmlNode) =
    htmlNode.CssSelect("div.sz-session__time") 
        |> Seq.map (fun h -> h.DirectInnerText())
        |> Seq.head

let GetTitle (htmlNode:HtmlNode) =
    htmlNode.CssSelect("h3.sz-session__title > a") 
        |> Seq.map (fun h -> h.DirectInnerText()) 
        |> Seq.head

let GetValue (htmlAttribute:HtmlAttribute) =
    Some (htmlAttribute.Value())

let GetSpeakerId (htmlNode:HtmlNode) =
    htmlNode.CssSelect("ul.sz-session__speakers > li") 
        |> Seq.map (fun h -> h.TryGetAttribute "data-speakerid") 
        |> Seq.map (Option.bind GetValue)
        |> Seq.tryHead
        |> Option.flatten

let tracks = expertDay.CssSelect("div.sz-session__card")
                    |> Seq.map(fun s -> {Room = (GetRoom s); Time = (GetTime s); Title = (GetTitle s); SpeakerId = (GetSpeakerId s) })
                    