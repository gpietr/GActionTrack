module Queries
open Storage

let mutable private storage: ITrackingStorage = Unchecked.defaultof<ITrackingStorage>

let SetStorage (newStorage: ITrackingStorage) =
    storage <- newStorage

let GetNumberOfSessions() =
    storage.GetAll()
    |> Seq.map (fun entry -> [ entry.UserId; entry.SessionId ])
    |> Seq.distinct
    |> Seq.length


let GetUsers() =
    storage.GetAll()
    |> Seq.map (fun entry -> entry.UserId)
    |> Seq.distinct
    |> Seq.toList