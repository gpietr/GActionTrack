namespace GActionTrack.Web.Controllers

open System
open System.Collections.Generic
open System.Linq
open System.Threading.Tasks
open System.Diagnostics

open Microsoft.AspNetCore.Mvc
open Microsoft.Extensions.Logging

open GActionTrack

open GActionTrack.Web.Models

type IndexViewModel = {
    Sessions: List<Queries.SessionSummary>
    Timeline: List<Queries.TimelinePoint>
}

type ProceduresViewModel = {
    Procedures: List<Queries.ProcedureInfo>
    Description: string
}

type ProcedureDetailsViewModel = {
    Entries: List<Storage.TrackingEntry>
    Description: string
}

[<AllowNullLiteral>]

type HomeController (logger : ILogger<HomeController>) =
    inherit Controller()

    let Init() =
        // For now, use the default in-memory storage and no filter
        use storage = new Storage.LiteDbTrackingStorage("C:\\temp\\test-logs.db")
        Queries.SetStorage(storage)
        storage

    member this.Index () =
        use storage = Init()
        let sessions = Queries.GetSessions Queries.NoFilter
        let timeline = Queries.GetProcedureTimeline()
        let vm = {
            Sessions = List<_> sessions
            Timeline = List<_> timeline
        }
        this.View(vm)

    member this.Procedures () =
        use storage = Init()

        let procedures = Queries.GetProcedureDailyStatistics Queries.NoFilter
        this.View(procedures)

    member this.ProceduresByDate(date: DateTimeOffset) =
        use storage = Init()

        let startOfDay = date.Date
        let endOfDay = date.AddDays(1.0).AddTicks(-1L)

        let dateFilter = Queries.DateFilter (Some startOfDay) (Some endOfDay)
        let procedures = Queries.GetProcedures dateFilter
        this.View("ProcedureList", { Procedures = List<_> procedures; Description = sprintf "Procedures for %s" (date.ToString("dd-MM-yy")) })


    member this.ProcedureDetails(procedureId: string) =
        use storage = Init()

        let procedureFilter = Queries.ProcedureFilter procedureId

        let entries = Queries.GetEntries procedureFilter
        this.View({ Entries = List<_> entries; Description = sprintf "Details for Procedure ID: %s" procedureId })

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error () =
        let reqId = 
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })
