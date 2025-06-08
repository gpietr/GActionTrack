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

        // Using anonymous types doesn't currently work 
        //Tracking.Track("User 1", "Session 1", "Procedure 1", "Action 2", new { Detail1 = "Detail C", Detail2 = "Detail D" });

        Assert.Equal(1, Queries.GetNumberOfSessions());
    }
}
