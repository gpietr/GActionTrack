module Storage

type TrackingEntry =
    {
        Timestamp: System.DateTimeOffset
        UserId: string
        SessionId: string
        ProcedureId: string
        Action: string
        Parameters: obj
    }

type ITrackingStorage =
    abstract member Save : TrackingEntry -> unit
    abstract member GetAll : unit -> seq<TrackingEntry>

type InMemoryTrackingStorage() =
    let store = ResizeArray<TrackingEntry>()
    
    interface ITrackingStorage with
        member _.Save entry =
            store.Add entry
        
        member _.GetAll() =
            store :> seq<_>