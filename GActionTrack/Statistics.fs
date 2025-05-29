module Statistics
open Storage

let mutable private storage: ITrackingStorage = InMemoryTrackingStorage()

let SetStorage (newStorage: ITrackingStorage) =
    storage <- newStorage

let GetNumberOfSessions() =
    storage.GetAll()
    |> Seq.map (fun entry -> [ entry.UserId; entry.SessionId ])
    |> Seq.distinct
    |> Seq.length