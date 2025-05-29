module Tracking

open Storage

let mutable private storage: ITrackingStorage = InMemoryTrackingStorage()

let SetStorage (newStorage: ITrackingStorage) =
    storage <- newStorage

let Track(userId: string, sessionId: string, procedureId: string, action: string, metadata: obj) =
    storage.Save({
        Timestamp = System.DateTimeOffset.UtcNow
        UserId = userId
        SessionId = sessionId
        ProcedureId = procedureId
        Action = action
        Parameters = metadata
    })
