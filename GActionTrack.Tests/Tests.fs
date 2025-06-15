module Tests

open Xunit
open System.Collections.Generic

let dictOf kvs = Dictionary<string, string>(kvs |> Seq.map (fun (k,v) -> KeyValuePair(k,v)))

[<Fact>]
let ``Basic counting works`` () =

    let dbPath = "tracking.db"
    try
        use storage = new Storage.LiteDbTrackingStorage(dbPath)

        Tracking.SetStorage storage

        Tracking.Track ("user1", "session1", "procedure1", "action1", Some (dictOf [ ("test", "ttt"); ("aaa", "bbb") ]))
        Tracking.Track ("user1", "session1", "procedure1", "action1", Some (dictOf [ ("test", "ttt"); ("aaa", "bbb") ]))
        Tracking.Track ("user1", "session2", "procedure1", "action1", None)
        Tracking.Track ("user2", "session1", "procedure1", "action1", None)

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