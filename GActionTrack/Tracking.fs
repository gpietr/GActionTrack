module Tracking

open Storage

let mutable private storage: ITrackingStorage = new InMemoryTrackingStorage()

let SetStorage (newStorage: ITrackingStorage) =
    storage <- newStorage

let Track(userId: string | null, sessionId: string, procedureId: string | null, action: string, metadata: System.Collections.Generic.Dictionary<string, string> | null) =
    storage.Save({
        Timestamp = System.DateTimeOffset.UtcNow
        UserId = userId
        SessionId = sessionId
        ProcedureId = procedureId
        Action = action
        Parameters = metadata
    })

let TrackWithTimestamp(timestamp: System.DateTimeOffset, userId: string | null, sessionId: string, procedureId: string | null, action: string, metadata: System.Collections.Generic.Dictionary<string, string> | null) =
    storage.Save({
        Timestamp = timestamp
        UserId = userId
        SessionId = sessionId
        ProcedureId = procedureId
        Action = action
        Parameters = metadata
    })