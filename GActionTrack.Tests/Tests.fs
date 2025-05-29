module Tests

open Xunit

[<Fact>]
let ``Basic counting works`` () =

    let storage: Storage.ITrackingStorage = Storage.InMemoryTrackingStorage()
    Tracking.SetStorage storage

    Tracking.Track ("user1", "session1", "procedure1", "action1", {| test = "ttt"; aaa = "bbb" |})
    Tracking.Track ("user1", "session1", "procedure1", "action1", {| test = "ttt"; aaa = "bbb" |})
    Tracking.Track ("user1", "session2", "procedure1", "action1", {| test = "ttt"; aaa = "bbb" |})
    Tracking.Track ("user2", "session1", "procedure1", "action1", {| test = "ttt"; aaa = "bbb" |})

    Statistics.SetStorage storage
    let numberOfSessions = Statistics.GetNumberOfSessions()
    Assert.Equal(3, numberOfSessions)