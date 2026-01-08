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

type TimelinePoint = {
    Date: System.DateTimeOffset // Use only the date part for grouping
    ProcedureCount: int
}

type DailyProcedureStatistics = {
    Date: System.DateTimeOffset
    ProcedureCount: int
    AverageProcedureDuration: TimeSpan
}

type ProcedureInfo = {
    Id: string
    Start: System.DateTimeOffset
    End: System.DateTimeOffset
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

let DateFilter (startDate: DateTimeOffset option) (endDate: DateTimeOffset option) =
    { Filter = fun entry ->
        let afterStart =
            match startDate with
            | Some sd -> entry.Timestamp >= sd
            | None -> true
        let beforeEnd =
            match endDate with
            | Some ed -> entry.Timestamp <= ed
            | None -> true
        afterStart && beforeEnd
    }

let ProcedureFilter (procedureId: string) =
    { Filter = fun entry -> not (isNull entry.ProcedureId) && entry.ProcedureId = procedureId }

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


let GetProceduresInner(filter: TrackingEntry -> bool) =
    storage.GetAll()
    |> Seq.filter filter
    |> Seq.filter (fun entry -> not (isNull entry.ProcedureId))
    |> Seq.groupBy (fun entry -> entry.ProcedureId)
    |> Seq.map (fun (procedureId, entries) ->
        let count = entries |> Seq.length
    
        let startTime = entries |> Seq.map (fun e -> e.Timestamp) |> Seq.min
        let endTime = entries |> Seq.map (fun e -> e.Timestamp) |> Seq.max

        {
            Id = procedureId
            Start = startTime
            End = endTime
        }
        )
    |> fun seq -> List<_> seq

let GetProcedures(filter: EntryFilter) =
    GetProceduresInner (fun entry -> filter.Filter(entry))

let GetProcedureDailyStatistics(filter: EntryFilter) =
    GetProcedures(filter)
    |> Seq.groupBy (fun entry -> entry.Start.Date)
    |> Seq.map (fun (date, entries) ->
        let count = entries |> Seq.length
        
        let procedureDurations: TimeSpan[] =
            entries
            |> Seq.map (fun e -> e.End - e.Start)
            |> Seq.toArray

        let averageDuration =
            if procedureDurations.Length > 0 then
                TimeSpan.FromTicks(
                    procedureDurations
                    |> Seq.map (fun d -> float d.Ticks)
                    |> Seq.average
                    |> int64
                )
            else
                TimeSpan.Zero

        { 
            Date = System.DateTimeOffset(date);
            ProcedureCount = count;
            AverageProcedureDuration = averageDuration;
        })
    |> Seq.sortBy (fun tp -> tp.Date)
    |> fun seq -> List<_> seq

/// Aggregates the number of procedures per day (timeline)
let GetProcedureTimeline() =
    storage.GetAll()
    |> Seq.filter (fun entry -> not (isNull entry.ProcedureId))
    |> Seq.groupBy (fun entry -> entry.Timestamp.Date)
    |> Seq.map (fun (date, entries) ->
        let count = entries |> Seq.map (fun e -> e.ProcedureId) |> Seq.distinct |> Seq.length
        { Date = System.DateTimeOffset(date); ProcedureCount = count })
    |> Seq.sortBy (fun tp -> tp.Date)
    |> fun seq -> List<_> seq


let GetEntries(filter: EntryFilter) =
    storage.GetAll()
    |> Seq.filter (fun entry -> filter.Filter(entry))
    |> fun seq -> List<_> seq



let inline Test (argument: ^T) : string
    when ^T : (member DoStuff : unit -> string) =
    
    argument.DoStuff()

type TestType(name: string) =
    member this.DoStuff() =
        sprintf "Hello, %s!" name


Test (TestType("World")) |> ignore