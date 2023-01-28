module RxInActionFSharp.BankAccount
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

type BankAccount() =
    let _inner = new Subject<int>()

    member this.MoneyTransactions 
        with get() = _inner :> IObservable<int>

let test () =
    let acct = new BankAccount()
    acct.MoneyTransactions
        .Subscribe(ConsoleObserver "Transferring")
    |> ignore

    let hackedSubject = 
        acct.MoneyTransactions :?> Subject<int>

    hackedSubject.OnNext(-9999)

let test2 () =
    let sbj = new Subject<int>()
    let proxy = sbj.AsObservable()

    try
        let _ = proxy :?> Subject<int>
        ()
    with e -> 
        Console.WriteLine(e.Message)

    try
        let _ = proxy :?> IObserver<int>
        ()
    with e -> 
        Console.WriteLine(e.Message)



