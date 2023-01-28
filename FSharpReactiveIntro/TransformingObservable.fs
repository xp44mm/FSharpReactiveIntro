module TransformingObservable

open System
open FSharp.Control.Reactive
open System.Windows.Forms

module String = 
    let rev (s : string) = new String (s.ToCharArray () |> Array.rev)

let reverseForm () =
    let input = new TextBox()
    let reversed = new Label()

    let subscription =
        input.TextChanged
        |> Observable.map (fun _ -> input.Text |> String.rev)
        |> Observable.subscribe (fun x -> reversed.Text <- x)

    reversed.Top <- 20
    let reverseForm = new Form ()
    reverseForm.Controls.Add input
    reverseForm.Controls.Add reversed
    reverseForm.Show ()
    Application.Run(reverseForm)
