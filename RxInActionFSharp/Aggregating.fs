module RxInActionFSharp.Aggregating

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
    Observable
        .Range(1, 5)
        .Sum()
        .Subscribe(ConsoleObserver "Sum")

let test1 () =
    Observable
        .Range(1, 5)
        .Count()
        .Subscribe(ConsoleObserver "Count")

let test2 () =
    Observable
        .Range(1, 5)
        .Count(fun x -> x % 2 = 0)
        .Subscribe(ConsoleObserver "Count of even numbers")

let test3 () =
    Observable
        .Range(1, 5)
        .Average()
        .Subscribe(ConsoleObserver "Average")

let test4 () =
    Observable
        .Range(1, 5)
        .Max()
        .Subscribe(ConsoleObserver "Max")

let test5 () =
    Observable
        .Range(1, 5)
        .Min()
        .Subscribe(ConsoleObserver "Min")

type StudentGrade = {Id:string;Name:string;Grade:float}
let test6 () =
    let grades = new Subject<StudentGrade>()
    grades.Max(fun g -> g.Grade)
        .Subscribe(ConsoleObserver "Maximal grade")
        |> ignore

    grades.OnNext({Id = "1";Name = "A";Grade = 85.0})
    grades.OnNext({Id = "2";Name = "B";Grade = 90.0})
    grades.OnNext({Id = "3";Name = "C";Grade = 80.0})
    grades.OnCompleted()

let test7 () =
    let grades = new Subject<StudentGrade>()
    grades
        .MaxBy(fun s -> s.Grade)
        .FlatMap(fun max -> max.ToObservable())
        .Subscribe(ConsoleObserver "Maximal object by grade")
        |> ignore

    grades.OnNext({Id = "1";Name = "A";Grade = 85.0})
    grades.OnNext({Id = "2";Name = "B";Grade = 90.0})
    grades.OnNext({Id = "3";Name = "C";Grade = 80.0})
    grades.OnCompleted()

let test8 () =
    Observable
        .Range(1, 5)
        .Aggregate(1,fun accumulate currItem -> accumulate * currItem)
        .Subscribe(ConsoleObserver "Aggregate")
        |> ignore

    Observable
        .Range(1, 5)
        .Scan(1,fun accumulate currItem -> accumulate * currItem)
        .Subscribe(ConsoleObserver "Scan")
        |> ignore

let test9 () =
    let numbers = new Subject<int>()
    numbers.Aggregate(
        new SortedSet<int>(),
        (fun largest item ->
            largest.Add(item) |> ignore
            if (largest.Count > 2) then
                largest.Remove(largest.Min) |> ignore
            largest
        ),
        (fun largest -> largest.Min))
        .Subscribe(ConsoleObserver "Aggregate")
        |> ignore

    numbers.OnNext(3)
    numbers.OnNext(1)
    numbers.OnNext(4)
    numbers.OnNext(2)
    numbers.OnCompleted()
