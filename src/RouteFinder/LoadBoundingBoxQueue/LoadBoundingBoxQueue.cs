using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using OpenStreetMapEtl.Utils;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.WindowsAzure.Storage.Queue;

namespace LoadBoundingBoxQueue
{
    public static class LoadBoundingBoxesIntoRegion
    {
        public static double kmSize = 16;

        [FunctionName("LoadBoundingBoxesIntoRegion")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            [Queue("bounding-box-downloadrequests")] CloudQueue messages,
            ILogger log
        )
        {
            log.LogInformation($"{nameof(LoadBoundingBoxesIntoRegion)} processed a {req.Method} request {req.QueryString}");

            var query = req.GetQueryParameterDictionary();
            BoundingBox originalBoundingBox;
            try
            {
                originalBoundingBox = GetOriginalBoundingBox(query);
            } catch (ArgumentException ex)
            {
                return new BadRequestObjectResult($"Bad request: {ex.Message}");
            }

            var bboxes = new BuildBoundingBoxes(kmSize).GetBoundingBoxes(originalBoundingBox).ToList();
            foreach (var bbox in bboxes)
            {
                await messages.AddMessageAsync(new CloudQueueMessage(JsonConvert.SerializeObject(bbox)));
            }

            return (ActionResult)new OkObjectResult($"Added {bboxes.Count} to the queue.");
        }

        private static BoundingBox GetOriginalBoundingBox(IDictionary<string, string> query)
        {
            if (!query.TryGetValue("q", out var bbox)) {
                throw new ArgumentException("Missing argument q");
            }
            var boxCoords = bbox.Split(',');
            if (boxCoords.Length != 4)
            {
                throw new ArgumentException($"Bad bbox definition {bbox}");
            }
            var parsed = double.TryParse(boxCoords[0], out var southLat);
            parsed &= double.TryParse(boxCoords[1], out var westLng);
            parsed &= double.TryParse(boxCoords[2], out var northLat);
            parsed &= double.TryParse(boxCoords[3], out var eastLng);
            if (!parsed)
            {
                throw new ArgumentException($"Bad bbox definition {bbox}");
            }

            var bboxObj = new BoundingBox
            {
                EastLongitude = eastLng,
                WestLongitude = westLng,
                NorthLatitude = northLat,
                SouthLatitude = southLat
            };
            return bboxObj;
        }

    }
}
