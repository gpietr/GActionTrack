module Storage

open LiteDB

type TrackingEntry =
    {
        Timestamp: System.DateTimeOffset
        UserId: string | null
        SessionId: string
        ProcedureId: string | null
        Action: string
        Parameters: System.Collections.Generic.Dictionary<string, string> | null
    }

type ITrackingStorage =
    abstract member Save : TrackingEntry -> unit
    abstract member GetAll : unit -> seq<TrackingEntry>
    abstract member Clear : unit -> unit


type LiteDbTrackingStorage(dbPath: string) =
    let database = new LiteDatabase(dbPath)
    let collection = database.GetCollection<TrackingEntry>("tracking")

    member _.Save (entry: TrackingEntry) : unit =
        collection.Insert(entry) |> ignore
        
    member _.GetAll() =
        collection.FindAll() |> Seq.toList :> seq<_>
        
    member _.Clear(): unit = 
        collection.DeleteAll() |> ignore

    // Interface delegates to public members
    interface ITrackingStorage with
        member this.Save entry = this.Save entry
        member this.GetAll() = this.GetAll()
        member this.Clear() = this.Clear()

    interface System.IDisposable with
        member _.Dispose() =
            database.Dispose()

type InMemoryTrackingStorage() =
    let mutable entries: TrackingEntry list = []

    interface ITrackingStorage with
        member _.Save entry =
            entries <- entry :: entries
        member _.GetAll() =
            entries :> seq<_>
        member _.Clear() =
            entries <- []

