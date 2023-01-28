module RxInActionFSharp.Operators
open System
open System.IO
open System.Text
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

open FSharp.Idioms.PointFree
open FSharp.Literals.Literal
open FSharp.Control.Reactive

open System.Reactive
open System.Reactive.Disposables
open System.Reactive.Observable.Aliases
open System.Reactive.Threading.Tasks
open System.Reactive.Linq
open System.Reactive.Subjects


let test () =
    let messages = Observable.Interval(TimeSpan.FromSeconds(1))
    let messagesViewModels =
        messages
            .Map(fun i -> {|
                MessageContent = i
                User = i*11L
            |})
    ()

let test1 () =
    let news = 
        Observable
            .Interval(TimeSpan.FromSeconds(1))
            .Map(fun i -> 
                {|Images=Observable.Return({|IsChildFriendly=true|})
                |})

    news
        .FlatMap(fun n -> n.Images)
        .Filter(fun img -> img.IsChildFriendly)
        .Subscribe(fun img -> 
            let AddToHeadlines = ignore
            AddToHeadlines(img))
type Image = {ImageName:string; IsChildFriendly:bool}
type NewsItem = {Title:string;Url:string;Images: Image list}
let test2 () =
    let theNews = [
        {
            Title = "NewsItem1"
            Url = "https://news.com/NewsItem1"
            Images = [
                {ImageName = "Item1Image1"; IsChildFriendly = true}
                {ImageName = "Item1Image2"; IsChildFriendly = false}
            ]
        }
        {
            Title = "NewsItem2"
            Url = "https://news.com/NewsItem2"
            Images = [
                {ImageName = "Item2Image1"; IsChildFriendly = true}
            ]
        }
    ]

    ()
type NewImageViewModel = {ItemUrl:string;NewsImage:Image}
let test3 () =
    let news:IObservable<NewsItem> = Observable.empty<NewsItem>

    news.FlatMap(fun newsItem -> 
        let imgs = newsItem.Images.ToObservable()
        imgs
            .Map(fun img -> newsItem.Url,img)
            )
        .Filter(fun (_,img) -> img.IsChildFriendly)
        .Subscribe(ConsoleObserver "AddToHeadlines")

type ChatMessage = {Content:string;Sender:string}
type ChatRoom = {Id:string;Messages:IObservable<ChatMessage>;}
type ChatMessageViewModel(m) = class end
let test4()=
    let rooms = Observable.empty<ChatRoom>

    rooms
        .Do(ConsoleObserver "Rooms")
        .SelectMany(fun r -> r.Messages) // mergeMap ?
        .Select(fun m -> ChatMessageViewModel(m))
        .Subscribe(ConsoleObserver "AddToDashboard")

let test5() =
    let roomsSubject = new Subject<ChatRoom>()
    let room1 = new Subject<ChatMessage>()
    let room2 = new Subject<ChatMessage>()

    roomsSubject.OnNext({Id = "Room1"; Messages = room1})

    room1.OnNext({Content = "First Message"; Sender = "1"});
    room1.OnNext({Content = "Second Message"; Sender = "1"});
    roomsSubject.OnNext({Id = "Room2"; Messages = room2});
    room2.OnNext({Content = "Hello World"; Sender = "2" });
    room1.OnNext({Content = "Another Message"; Sender = "1" });

    let rooms: IObservable<ChatRoom> = roomsSubject.AsObservable()
    ()

let test6() =
    let roomsSubject = new Subject<ChatRoom>()

    let rooms: IObservable<ChatRoom> = roomsSubject.AsObservable()

    rooms
        .Do(ConsoleObserver "Rooms")
        .SelectMany(fun room ->
            let msgs = room.Messages
            msgs
                .Map(fun msg -> room.Id, msg)
            )
        .Subscribe(ConsoleObserver "AddToDashboard")

let test7 () =
    let strings = ["aa"; "Abc"; "Ba"; "Ac"].ToObservable()

    strings
        .Filter(fun s -> s.[0] = 'A')
        .Subscribe(ConsoleObserver "")

let test8 () =
    let subject = new Subject<{|Title:string|}>()
    subject
        .Do(ConsoleObserver "")
        .Distinct(fun n->n.Title)
        .Subscribe(ConsoleObserver "Distinct")
        |> ignore
    subject.OnNext({|Title = "Title1"|})
    subject.OnNext({|Title = "Title2"|})
    subject.OnNext({|Title = "Title1"|})
    subject.OnNext({|Title = "Title3"|})
