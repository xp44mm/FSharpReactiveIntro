module GettingStarted

open FSharp.Control.Reactive

let observableModule () =
    let obs1 = Observable.single 1
    let obs2 = Observable.single "A"

    Observable.zip obs1 obs2
    |> Observable.subscribe (printfn "%A")
    |> ignore

open FSharp.Control.Reactive.Builders

let computationExpressions () =
    let rec generate x =
        observe {
            yield x
            if x < 10 then
                yield! generate (x + 1) }
    generate 5
    |> Observable.subscribeWithCallbacks
        (printfn "next:%A") (printfn "error:%A") (printfn "complete:%A")
    |> ignore
