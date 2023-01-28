module RxInActionFSharp.ReactingToErrors

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
open System.Reactive.Concurrency
open System.Runtime

type WeatherSimulation = WeatherSimulation

let BasicOnError()=
    // This the most basic way you would work with OnError.
    // But its not ideal, consider using the 'Catch' operator
    let weatherSimulationResults =
        Observable.Throw<WeatherSimulation>(new OutOfMemoryException())

    weatherSimulationResults
        .Subscribe(
            (fun _ -> ()),
            (fun (e:exn) ->
                if e.GetType() = typeof<OutOfMemoryException> then
                    //a last attampt to free some memory
                    GCSettings.LargeObjectHeapCompactionMode <-
                        GCLargeObjectHeapCompactionMode.CompactOnce
                    GC.Collect()
                    GC.WaitForPendingFinalizers()
                    Console.WriteLine("GC Done")
            )
            )

let test2 () =
    let weatherSimulationResults =
        Observable.Throw<WeatherSimulation>(new OutOfMemoryException())

    weatherSimulationResults
        .Catch(fun (ex:OutOfMemoryException) ->
            Console.WriteLine("handling OOM exception")
            Observable.Empty<WeatherSimulation>()
        )
        .Subscribe(ConsoleObserver "Catch (source throws)")
let test3 () =
    let weatherSimulationResults =
        Observable.Throw<WeatherSimulation>(new OutOfMemoryException())

    //Catch is not limited to a single exception type, it can be general to ALL exceptions 
    weatherSimulationResults
        .Catch(Observable.Empty<WeatherSimulation>())
        .Subscribe(ConsoleObserver "Catch (handling all exception types)")

type WeatherReport = { Temperature:float; Station:string }

let test4 () =
    let weatherStationA =
        Observable.Throw<WeatherReport>(new OutOfMemoryException())

    let weatherStationB =
        Observable.Return<_>({ Station = "B"; Temperature = 20.0 })

    weatherStationA
        .OnErrorResumeNext(weatherStationB)
        .Subscribe(ConsoleObserver "OnErrorResumeNext(source throws)")
        |> ignore
    weatherStationB
        .OnErrorResumeNext(weatherStationB)
        .Subscribe(ConsoleObserver "OnErrorResumeNext(source completed)")
        |> ignore

let test5 () =
    let weatherStationA =
        Observable.Throw<WeatherReport>(new OutOfMemoryException())

    weatherStationA
        .Do(ConsoleObserver "Log")
        .Retry(3)
        .Subscribe(ConsoleObserver "Retry")


type DisposableType () =
    interface IDisposable with
        member this.Dispose() = ()

let TraditionalUsingStatement() =
    use disposable = new DisposableType()

    ()

open System.Reflection
type SensorData = {Data:uint64}
let test6 () =
    let logFilePath = Path.Combine(
        Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
        "example.log")
    let sensorData =
        Observable.Range(1, 3)
            .Map(fun x -> { Data = x |> uint64 })

    let sensorDataWithLogging =
        Observable.Using((fun () -> new StreamWriter(logFilePath)),
            (fun (writer:StreamWriter) -> sensorData.Do(fun x -> x.Data |> writer.WriteLine))
            )

    sensorDataWithLogging.Subscribe(ConsoleObserver "sensor")


let test7 () =
    let initial () =
        let subject = new Subject<int>()
        let observable =
            Observable.Using(
                (fun () -> Disposable.Create(fun ()-> Console.WriteLine("DISPOSED"))),
                (fun _ -> subject:>IObservable<_>))
        subject,observable

    let subject,observable = initial()
    Console.WriteLine("Disposed when completed")
    observable.Subscribe(ConsoleObserver "")
        |> ignore
    subject.OnCompleted()

    let subject,observable = initial()
    Console.WriteLine("Disposed when error occurs")
    observable.Subscribe(ConsoleObserver "")
        |> ignore
    subject.OnError(new Exception("error"))

    let subject,observable = initial()
    Console.WriteLine("Disposed when subscription disposed")
    let subscription = observable.Subscribe(ConsoleObserver "")
    subscription.Dispose()

let test8 () =
    let progress = Observable.Range(1, 3)
            
    progress
        .Finally(fun () -> ())
        .Subscribe(fun x -> ())

let test9 () =
    Console.WriteLine("Successful complete")
    Observable.Empty<int>()
        .Finally(fun () -> Console.WriteLine("Finally Code"))
        .Subscribe(ConsoleObserver "")
        |> ignore

    Console.WriteLine("Error termination");
    Observable.Throw<Exception>(new Exception("error"))
        .Finally(fun () -> Console.WriteLine("Finally Code"))
        .Subscribe(ConsoleObserver "")
        |> ignore

    Console.WriteLine("Unsubscribing")
    let subject=new Subject<int>()
    let subscription =
        subject.AsObservable()
            .Finally(fun () -> Console.WriteLine("Finally Code"))
            .Subscribe(ConsoleObserver "")
    subscription.Dispose()

let test10 () =
    let observable = Observable.Return 0
    let subscription =
        observable.Subscribe(fun x -> ())
    ()
let test11 () =
    let mutable obj = new obj()
    let weak = new WeakReference(obj)
    GC.Collect()
    Console.WriteLine("IsAlive: {0} obj!=null is {1}", weak.IsAlive, obj<>null)

    obj <- null
    GC.Collect()
    Console.WriteLine("IsAlive: {0}", weak.IsAlive)

let test12 () =
    let fast = Observable.Interval(TimeSpan.FromSeconds(1))
    let slow = Observable.Interval(TimeSpan.FromSeconds(2))
    let zipped = slow.Zip(fast, fun x y -> x + y)
    let subscription =
        zipped
            .Select(fun x -> sprintf "%d elements are in memory" x)
            .Subscribe(ConsoleObserver "Backpressure")

    Console.ReadLine() |> ignore
    subscription.Dispose()

let test13 () =
    let heartRatesValues = [ 70; 75; 80; 90; 80 ]
    let speedValues = [ 50;51;53;52;55 ]

    let heartRates = 
        Observable.Interval(TimeSpan.FromSeconds(1))
            .Map(fun x -> heartRatesValues.[int x % heartRatesValues.Length])
    let speeds = 
        Observable.Interval(TimeSpan.FromSeconds(3))
            .Select(fun x -> speedValues.[int x % speedValues.Length])

    heartRates.CombineLatest(speeds, fun h s -> sprintf "Heart:%d Speed:%d" h s)
        .Take(5)
        .Subscribe(ConsoleObserver "CombineLatest")

