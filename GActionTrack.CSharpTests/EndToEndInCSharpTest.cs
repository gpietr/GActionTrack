namespace GActionTrack.CSharpTests;

public class EndToEndInCSharpTest
{
    private record TestTracingData(string Detail1, string Detail2);


    [Fact]
    public void TestEndToEnd()
    {
        Storage.ITrackingStorage storage = new Storage.LiteDbTrackingStorage("TestDatabase.db");
        Tracking.SetStorage(storage);
        Queries.SetStorage(storage);

        Tracking.Track("User 1", "Session 1", "Procedure 1", "Action 1", new Dictionary<string, string>
        {
            { "Detail1", "Detail A" },
            { "Detail2", "Detail B" }
        });

        Assert.Equal(1, Queries.GetNumberOfSessions());

        List<Queries.UserSummary> userStatistics = Queries.GetUsers();
        Assert.Single(userStatistics);
        Assert.Equal(1, userStatistics[0].SessionCount);


        var allSessions = Queries.GetSessions(Queries.NoFilter);
        Assert.Single(allSessions);

        var user1Sessions = Queries.GetSessions(Queries.UserFilter("User 1"));
        Assert.Single(user1Sessions);
    }
}
