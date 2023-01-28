module RxInActionFSharp.DoorOpened

open System
open System.IO
open System.Text
open System.Collections.Generic
open System.Threading
open System.Threading.Tasks

open FSharp.Control.Reactive
open FSharp.Idioms.PointFree
open FSharp.Literals.Literal

open System.Reactive
open System.Reactive.Disposables
open System.Reactive.Observable.Aliases
open System.Reactive.Threading.Tasks
open System.Reactive.Linq
open System.Reactive.Subjects

type Gender =
| Male
| Female

type OpenDirection =
| Entering
| Leaving

type DoorOpened = 
    {
     Name: string
     Direction: OpenDirection
     Gender: Gender
    }

    static member create (name,gender,direction) = {
        Name = name
        Direction = direction
        Gender = gender
        }

let test () =
    let doorOpenedSubject = new Subject<DoorOpened>()
    let doorOpened:IObservable<DoorOpened> = doorOpenedSubject.AsObservable()

    let entrances = doorOpened.Filter(fun o -> o.Direction = OpenDirection.Entering)
    let maleEntering = entrances.Filter(fun x -> x.Gender = Gender.Male)
    let femaleEntering = entrances.Filter(fun x -> x.Gender = Gender.Female)

    let exits = doorOpened.Filter(fun o -> o.Direction = OpenDirection.Leaving)
    let maleExiting = exits.Filter(fun x -> x.Gender = Gender.Male)
    let femaleExiting = exits.Filter(fun x -> x.Gender = Gender.Female)

    maleEntering
        .Join(femaleEntering,
            (fun male -> maleExiting.Filter(fun exit -> exit.Name = male.Name)),
            (fun female -> femaleExiting.Filter(fun exit -> female.Name = exit.Name)),
            (fun m f -> m,f))
        .Subscribe(ConsoleObserver "Together At Room")
        |> ignore

    doorOpenedSubject.OnNext(
        DoorOpened.create("Bob", Gender.Male, OpenDirection.Entering))
    doorOpenedSubject.OnNext(
        DoorOpened.create("Sara", Gender.Female, OpenDirection.Entering))
    doorOpenedSubject.OnNext(
        DoorOpened.create("John", Gender.Male, OpenDirection.Entering))
    doorOpenedSubject.OnNext(
        DoorOpened.create("Sara", Gender.Female, OpenDirection.Leaving))
    doorOpenedSubject.OnNext(
        DoorOpened.create("Fibi", Gender.Female, OpenDirection.Entering))
    doorOpenedSubject.OnNext(
        DoorOpened.create("Bob", Gender.Male, OpenDirection.Leaving))
    doorOpenedSubject.OnNext(
        DoorOpened.create("Dan", Gender.Male, OpenDirection.Entering))
    doorOpenedSubject.OnNext(
        DoorOpened.create("Fibi", Gender.Female, OpenDirection.Leaving))
    doorOpenedSubject.OnNext(
        DoorOpened.create("John", Gender.Male, OpenDirection.Leaving))
    doorOpenedSubject.OnNext(
        DoorOpened.create("Dan", Gender.Male, OpenDirection.Leaving))

    // Rest of code that simulates participants leaving the room

let test2 () =
    let doorOpenedSubject = new Subject<DoorOpened>()
    let doorOpened = doorOpenedSubject.AsObservable()

    let enterences = doorOpened.Filter(fun o -> o.Direction = OpenDirection.Entering)
    let maleEntering = enterences.Filter(fun x -> x.Gender = Gender.Male)
    let femaleEntering = enterences.Filter(fun x -> x.Gender = Gender.Female)
    let exits = doorOpened.Filter(fun o -> o.Direction = OpenDirection.Leaving)
    let maleExiting = exits.Filter(fun x -> x.Gender = Gender.Male)
    let femaleExiting = exits.Filter(fun x -> x.Gender = Gender.Female)
    let malesAcquaintances =
        maleEntering
            .GroupJoin(femaleEntering,
                (fun male -> maleExiting.Filter(fun exit -> exit.Name = male.Name)),
                (fun female -> femaleExiting.Filter(fun exit -> female.Name = exit.Name)),
                (fun m females -> m,females))

    let amountPerUser =
        malesAcquaintances
            .FlatMap(fun (m,females) ->
                    females
                        .Scan(0,fun acc curr -> acc + 1)
                        .Map(fun cnt -> m,females,cnt)
            )


    amountPerUser.Subscribe(ConsoleObserver "Amount of meetings per User")
    |> ignore
    //This is the sequence you see in Figure 9.8
    doorOpenedSubject.OnNext(DoorOpened.create("Bob", Gender.Male, OpenDirection.Entering))
    doorOpenedSubject.OnNext(DoorOpened.create("Sara", Gender.Female, OpenDirection.Entering))
    doorOpenedSubject.OnNext(DoorOpened.create("John", Gender.Male, OpenDirection.Entering))
    doorOpenedSubject.OnNext(DoorOpened.create("Sara", Gender.Female, OpenDirection.Leaving))
    doorOpenedSubject.OnNext(DoorOpened.create("Fibi", Gender.Female, OpenDirection.Entering))
    doorOpenedSubject.OnNext(DoorOpened.create("Bob", Gender.Male, OpenDirection.Leaving))
    doorOpenedSubject.OnNext(DoorOpened.create("Dan", Gender.Male, OpenDirection.Entering))
    doorOpenedSubject.OnNext(DoorOpened.create("Fibi", Gender.Female, OpenDirection.Leaving))
    doorOpenedSubject.OnNext(DoorOpened.create("John", Gender.Male, OpenDirection.Leaving))
    doorOpenedSubject.OnNext(DoorOpened.create("Dan", Gender.Male, OpenDirection.Leaving))
