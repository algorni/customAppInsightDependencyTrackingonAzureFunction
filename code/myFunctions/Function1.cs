using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;

namespace myFunctions
{
    public class Function1
    {
        private readonly TelemetryClient telemetryClient;

        public Function1(TelemetryConfiguration configuration)
        {
            this.telemetryClient = new TelemetryClient(configuration);
        }


        [FunctionName("Function1")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,            
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");
            
            string name = req.Query["name"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            name = name ?? data?.name;


            //now let's assume you need to perform a call to an external dependency which is not automatically tracked by Application Insights

            log.LogInformation("Starting the call of an external dependency");

            var startTime = DateTime.UtcNow;
            var timer = System.Diagnostics.Stopwatch.StartNew();

            //wait randomly up to 5 seconds
            await Task.Delay(Random.Shared.Next(5000));

            timer.Stop();

            log.LogInformation("External dependency call complated");

            //track the dependency to be shown in the dependency map            
            telemetryClient.TrackDependency("myDependencyType", "myDependencyCall", "myDependencyData", startTime, timer.Elapsed, true);


            string responseMessage = string.IsNullOrEmpty(name)
                ? "This HTTP triggered function executed successfully. Pass a name in the query string or in the request body for a personalized response. External Depdendency called."
                : $"Hello, {name}. This HTTP triggered function executed successfully. External Depdendency called.";
            
            return new OkObjectResult(responseMessage);
        }
    }
}
