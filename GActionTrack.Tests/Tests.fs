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

        let users = Queries.GetUsers()
        Assert.Equal(2, List.length users)

        storage.Clear()
    finally
        System.IO.File.Delete(dbPath)