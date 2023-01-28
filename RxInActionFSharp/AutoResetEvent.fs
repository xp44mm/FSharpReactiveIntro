module RxInActionFSharp.AutoResetEvent

open System
open System.Threading

let myResetEvent = new AutoResetEvent(false)

let sqrt() =
    let _ = myResetEvent.WaitOne() // WaitOne方法阻塞
    Console.WriteLine(DateTime.Now.ToShortTimeString())
    Thread.Sleep(500)

let test() =
    let td = new Thread(new ThreadStart(sqrt))
    td.Name <- "线程一"
    td.Start()
    let _ = myResetEvent.Set()//Set()方法执行后则继续执行
    Console.ReadKey()





