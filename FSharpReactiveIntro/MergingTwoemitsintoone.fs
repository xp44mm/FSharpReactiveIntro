module MergingTwoemitsintoone

open System
open FSharp.Control.Reactive
open System.Windows.Forms

let colorForm () =
    let redBtn = new Button()
    let greenBtn = new Button()
    let result = new Label()

    let red = redBtn.Click |> Observable.mapTo "Red"
    let green = greenBtn.Click |> Observable.mapTo "Green"

    let _ =
        Observable.merge red green
        |> Observable.subscribe (fun x -> result.Text <- x)

    redBtn.Text <- "Red"
    greenBtn.Text <- "Green"
    greenBtn.Top <- 20
    result.Left <- 100

    let colorForm = new Form ()
    colorForm.Controls.Add redBtn
    colorForm.Controls.Add greenBtn
    colorForm.Controls.Add result
    colorForm.Show ()

    Application.Run(colorForm)
