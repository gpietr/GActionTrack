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

type HomeController (logger : ILogger<HomeController>) =
    inherit Controller()

    member this.Index () =
        // For now, use the default in-memory storage and no filter
        use storage = new Storage.LiteDbTrackingStorage("C:\\temp\\test-logs.db")
        Queries.SetStorage(storage)
        let sessions = Queries.GetSessions Queries.NoFilter
        this.View(sessions)

    member this.Privacy () =
        this.View()

    [<ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)>]
    member this.Error () =
        let reqId = 
            if isNull Activity.Current then
                this.HttpContext.TraceIdentifier
            else
                Activity.Current.Id

        this.View({ RequestId = reqId })
