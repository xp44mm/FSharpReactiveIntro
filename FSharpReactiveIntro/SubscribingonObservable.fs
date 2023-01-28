module SubscribingonObservable
open System
open FSharp.Control.Reactive
open System.Windows.Forms

let buttonForm () =
    let buttonForm = new Form ()
    let btn = new Button()
    let txb = new TextBox()

    let subscription =
        txb.TextChanged
        |> Observable.startWith [EventArgs.Empty]
        |> Observable.subscribe (fun _ -> btn.Enabled <- not (String.IsNullOrEmpty txb.Text))

    btn.Text <- "OK"
    btn.Top <- 20

    buttonForm.Controls.Add txb
    buttonForm.Controls.Add btn
    buttonForm.Show ()
    Application.Run(buttonForm)
