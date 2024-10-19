using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using System;
using System.IO;


var connection = "";

var blobServiceClient = new BlobServiceClient(connection);
var containerClient = blobServiceClient.GetBlobContainerClient("cool");

var filePath = "C:\\Users\\OWNER\\dippa\\uploadData\\dummy300M.txt";
var uploadTimes = 4;


var tiers = new List<string>() {"hot", "cool", "cold"};
var tiersEnum = new List<AccessTier>() {AccessTier.Hot, AccessTier.Cool, AccessTier.Cold };
var results = new List<List<double>>();

for(var i = 0; i < tiers.Count; i++)
{
    var options = new BlobUploadOptions
    {
        AccessTier = tiersEnum[i]
    };

    var tierResults = new List<double>();

    for(var j = 0; j < uploadTimes ; j++)
    {
        var dto = new DateTimeOffset(DateTime.UtcNow);
        var unixTime = dto.ToUnixTimeSeconds().ToString();
        var blobName = tiers[i] + unixTime  + "-" + j.ToString() + ".txt";
        var startTime = DateTime.Now;

        var blobClient = containerClient.GetBlobClient(blobName);
        blobClient.Upload(filePath, options);

        var endTime = DateTime.Now;
        var writeTime = (endTime - startTime).TotalMilliseconds;

        tierResults.Add(writeTime);
    }

    results.Add(tierResults);
}

for (var i = 0; i < tiers.Count; i++)
{
    var median = CalcMedian(results[i], uploadTimes);
    var deviation = CalcDeviation(results[i], uploadTimes, median);
    Console.WriteLine(tiers[i] + " median: " + median.ToString());
    Console.WriteLine(tiers[i]+ " deviation: " + deviation.ToString());

    var dto = new DateTimeOffset(DateTime.UtcNow);
    var unixTime = dto.ToUnixTimeSeconds().ToString();

    var resultsRow = "median: " + median.ToString() + ", deviation: " + deviation.ToString() + "\n";
    var resultsString = results[i].Aggregate(resultsRow, (acc, x) => acc + x.ToString() + "\n");
    var resultPath = "C:\\Users\\OWNER\\dippa\\testing\\blobResults\\" + tiers[i] + unixTime + ".txt";
    File.WriteAllText(resultPath, resultsString);
}


static double CalcMedian(List<double> values, int storeTimes)
{
    return values.Aggregate(0.0, (acc, x) => acc + x) / storeTimes;
}

static double CalcDeviation(List<double> values, int storeTimes, double median)
{
    var varianceCool = values.Aggregate(0.0, (acc, x) => acc + Math.Pow((x - median), 2)) / storeTimes;
    return Math.Sqrt(varianceCool);
}
