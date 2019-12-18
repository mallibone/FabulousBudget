namespace ConferenceApp.Services
//#r @"../../packages/FSharp.Data.3.3.2/lib/net45/FSharp.Data.dll"
open FSharp.Data

module XamExpertDay =
    type expertDay = HtmlProvider<"ExpertXamarin.html">

    type speaker = {Id:string; Name:string; Photo:string; Tagline:string}
    type track = {Room:string; Time:string; Title:string; SpeakerId:string option}

    let getName (htmlNode:HtmlNode) =
        htmlNode.CssSelect("h3.sz-speaker__name > a") |> Seq.map (fun h -> h.DirectInnerText()) |> Seq.head

    let getPhoto (htmlNode:HtmlNode) =
        htmlNode.Descendants["img"] |> Seq.map (fun h -> h.AttributeValue("src")) |> Seq.head

    let getTagline (htmlNode:HtmlNode) =
        htmlNode.CssSelect("h4.sz-speaker__tagline") |> Seq.map (fun h -> h.DirectInnerText()) |> Seq.head

    let getId (htmlNode:HtmlNode) =
        htmlNode.Attribute("data-speakerid").Value()

    let getSpeakers (html:string) =
        HtmlDocument.Parse(html)
            .CssSelect("li.sz-speaker")
            |> Seq.map (fun s -> {Id = (getId s); Name = (getName s); Photo = (getPhoto s); Tagline = (getTagline s)})

    let private getRoom (htmlNode:HtmlNode) =
        htmlNode.CssSelect("div.sz-session__room") 
            |> Seq.map (fun h -> h.DirectInnerText()) 
            |> Seq.head

    let private getTime (htmlNode:HtmlNode) =
        htmlNode.CssSelect("div.sz-session__time") 
            |> Seq.map (fun h -> h.DirectInnerText())
            |> Seq.head

    let private getTitle (htmlNode:HtmlNode) =
        htmlNode.CssSelect("h3.sz-session__title > a") 
            |> Seq.map (fun h -> h.DirectInnerText()) 
            |> Seq.head

    let private getValue (htmlAttribute:HtmlAttribute) =
        Some (htmlAttribute.Value())

    let private getSpeakerId (htmlNode:HtmlNode) =
        htmlNode.CssSelect("ul.sz-session__speakers > li") 
            |> Seq.map (fun h -> h.TryGetAttribute "data-speakerid") 
            |> Seq.map (Option.bind getValue)
            |> Seq.tryHead
            |> Option.flatten

    let getTracks (html:string) =
        HtmlDocument.Parse(html)
            .CssSelect("div.sz-session__card")
            |> Seq.map(fun s -> {Room = (getRoom s); Time = (getTime s); Title = (getTitle s); SpeakerId = (getSpeakerId s) })

