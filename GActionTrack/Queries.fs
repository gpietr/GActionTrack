module Queries
open Storage
open System.Collections.Generic
open System


type UserSummary =
    {
        UserId: string
        SessionCount: int
        ProcedureCount: int
        FirstActivity: System.DateTimeOffset
        LastActivity: System.DateTimeOffset
    }

type SessionSummary =
    {
        UserId: string
        SessionId: string
        ProcedureCount: int
        FirstActivity: System.DateTimeOffset
        LastActivity: System.DateTimeOffset
    }

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
    |> Seq.groupBy (fun entry -> entry.UserId)
    |> Seq.map (fun (userId, entries) ->
        let sessionCount = entries |> Seq.map (fun e -> e.SessionId) |> Seq.distinct |> Seq.length
        let procedureCount = entries |> Seq.map (fun e -> e.SessionId + e.ProcedureId) |> Seq.distinct |> Seq.length
        let firstActivity = entries |> Seq.map (fun e -> e.Timestamp) |> Seq.min
        let lastActivity = entries |> Seq.map (fun e -> e.Timestamp) |> Seq.max
        
        { UserId = userId
          SessionCount = sessionCount
          ProcedureCount = procedureCount
          FirstActivity = firstActivity
          LastActivity = lastActivity })
    |> fun seq -> List<_> seq


type EntryFilter = {
    Filter: TrackingEntry -> bool
}

let NoFilter = { Filter = fun entry -> true}

let UserFilter (userId: string) = 
    { Filter = fun entry -> entry.UserId = userId }

let GetSessionsInner(filter: TrackingEntry -> bool) =
    storage.GetAll()
    |> Seq.filter filter
    |> Seq.groupBy (fun entry -> entry.SessionId)
    |> Seq.map (fun (sessionId, entries) ->
        let procedureCount = entries |> Seq.map (fun e -> e.ProcedureId) |> Seq.distinct |> Seq.length
        let firstActivity = entries |> Seq.map (fun e -> e.Timestamp) |> Seq.min
        let lastActivity = entries |> Seq.map (fun e -> e.Timestamp) |> Seq.max
        
        { UserId = entries |> Seq.head |> fun e -> e.UserId
          SessionId = sessionId
          ProcedureCount = procedureCount
          FirstActivity = firstActivity
          LastActivity = lastActivity })
    |> fun seq -> List<_> seq


let GetSessions(filter: EntryFilter) =
    GetSessionsInner (fun entry -> filter.Filter(entry))


let GetSessionsForUser(userId: string) =
    GetSessionsInner (UserFilter userId).Filter
