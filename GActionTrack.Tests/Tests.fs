module Tests

open Xunit
open System.Collections.Generic

let dictOf kvs = Dictionary<string, string>(kvs |> Seq.map (fun (k,v) -> KeyValuePair(k,v)))


[<Fact>]
let ``Procedure going over midnight`` () =

    let dbPath = "tracking.db"
    try
        use storage = new Storage.LiteDbTrackingStorage(dbPath)

        Tracking.SetStorage storage

        Tracking.TrackWithTimestamp (System.DateTimeOffset(2024, 6, 1, 23, 50, 0, System.TimeSpan.Zero), "user1", "session1", "procedure1", "action1", null)
        Tracking.TrackWithTimestamp (System.DateTimeOffset(2024, 6, 2, 0, 10, 0, System.TimeSpan.Zero), "user1", "session1", "procedure1", "action2", null)

        Queries.SetStorage storage
        let days = Queries.GetProcedureDailyStatistics(Queries.NoFilter) |> List.ofSeq
        Assert.Equal(1, days.Length) // Count start date only

        let day1 = List.head days
        Assert.Equal(1, day1.ProcedureCount)
        Assert.Equal(20, (int)day1.AverageProcedureDuration.TotalMinutes)

        storage.Clear()
    finally
        System.IO.File.Delete(dbPath)

[<Fact>]
let ``Procedure summary works`` () =

    let dbPath = "tracking.db"
    try
        use storage = new Storage.LiteDbTrackingStorage(dbPath)

        Tracking.SetStorage storage

        Tracking.TrackWithTimestamp (System.DateTimeOffset(2024, 6, 1, 10, 0, 0, System.TimeSpan.Zero), "user1", "session1", "procedure1", "action1", null)
        Tracking.TrackWithTimestamp (System.DateTimeOffset(2024, 6, 1, 10, 5, 0, System.TimeSpan.Zero), "user1", "session1", "procedure1", "action2", null)
        Tracking.TrackWithTimestamp (System.DateTimeOffset(2024, 6, 2, 10, 10, 0, System.TimeSpan.Zero), "user1", "session1", "procedure2", "action1", null)
        Tracking.TrackWithTimestamp (System.DateTimeOffset(2024, 6, 2, 10, 15, 0, System.TimeSpan.Zero), "user1", "session1", "procedure2", "action2", null)
        Tracking.TrackWithTimestamp (System.DateTimeOffset(2024, 6, 2, 11, 10, 0, System.TimeSpan.Zero), "user1", "session1", "procedure3", "action1", null)
        Tracking.TrackWithTimestamp (System.DateTimeOffset(2024, 6, 2, 11, 25, 0, System.TimeSpan.Zero), "user1", "session1", "procedure3", "action2", null)

        Queries.SetStorage storage
        let days = Queries.GetProcedureDailyStatistics(Queries.NoFilter) |> List.ofSeq
        Assert.Equal(2, days.Length)

        let day1 = List.head days
        Assert.Equal(5, (int)day1.AverageProcedureDuration.TotalMinutes)

        let day2 = List.last days
        Assert.Equal(10, (int)day2.AverageProcedureDuration.TotalMinutes)

        storage.Clear()
    finally
        System.IO.File.Delete(dbPath)

[<Fact>]
let ``Basic counting works`` () =

    let dbPath = "tracking.db"
    try
        use storage = new Storage.LiteDbTrackingStorage(dbPath)

        Tracking.SetStorage storage

        Tracking.Track ("user1", "session1", "procedure1", "action1", (dictOf [ ("test", "ttt"); ("aaa", "bbb") ]))
        Tracking.Track ("user1", "session1", "procedure1", "action1", (dictOf [ ("test", "ttt"); ("aaa", "bbb") ]))
        Tracking.Track ("user1", "session2", "procedure1", "action1", null)
        Tracking.Track ("user2", "session1", "procedure1", "action1", null)

        Queries.SetStorage storage
        let numberOfSessions = Queries.GetNumberOfSessions()
        Assert.Equal(3, numberOfSessions)

        let users = Queries.GetUsers() |> List.ofSeq
        Assert.Equal(2, users.Length)

        let user1 = List.find (fun (u: Queries.UserSummary) -> u.UserId = "user1") users
        Assert.Equal(2, user1.SessionCount)
        Assert.Equal(2, user1.ProcedureCount)

        let user1Sessions = Queries.GetSessions (Queries.UserFilter("user1"))
        Assert.Equal(2, Seq.length user1Sessions)
        let session1 = Seq.find (fun (s: Queries.SessionSummary) -> s.SessionId = "session1") user1Sessions
        Assert.Equal(1, session1.ProcedureCount)


        let allSessions = Queries.GetSessions(Queries.NoFilter)
        Assert.Equal(2, Seq.length allSessions)

        storage.Clear()
    finally
        System.IO.File.Delete(dbPath)