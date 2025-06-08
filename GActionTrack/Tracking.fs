module Tracking

open Storage

let mutable private storage: ITrackingStorage = new InMemoryTrackingStorage()

let SetStorage (newStorage: ITrackingStorage) =
    storage <- newStorage

let Track(userId: string, sessionId: string, procedureId: string, action: string, metadata: Option<System.Collections.Generic.Dictionary<string, string>>) =
    storage.Save({
        Timestamp = System.DateTimeOffset.UtcNow
        UserId = userId
        SessionId = sessionId
        ProcedureId = procedureId
        Action = action
        Parameters = metadata
    })
