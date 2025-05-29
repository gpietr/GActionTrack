namespace GActionTrack.CSharpTests;

using GActionTrack;

public class EndToEndInCSharpTest
{
    [Fact]
    public void TestEndToEnd()
    {
        Tracking.Track("User 1", "Session 1", "Procedure 1", "Action 1", new
        { 
            Detail1 = "Value 1",
            Detail2 = "Value 2"
        } );
    }
}
