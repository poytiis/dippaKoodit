// See https://aka.ms/new-console-template for more information

var configPath = "./config.csv";
var configLines = File.ReadAllLines(configPath);
var testFile = configLines[0];
var coolPath = configLines[1];
var premiumPath = configLines[2];
var storeTimes = Int32.Parse(configLines[3]);
var resultsPath = configLines[4];
var hotPath = configLines[5];
var transactionPath = configLines[6];

var fileBytes = File.ReadAllBytes(testFile);
var coolResults = RunStoreTest(storeTimes, fileBytes, coolPath);
var premiumResults = RunStoreTest(storeTimes, fileBytes, premiumPath);
var hotResults = RunStoreTest(storeTimes, fileBytes, hotPath);
var transactionResults = RunStoreTest(storeTimes, fileBytes, transactionPath);


var medianCool = CalcMedian(coolResults, storeTimes);
var medianPremium = CalcMedian(premiumResults, storeTimes);
var medianHot = CalcMedian(hotResults, storeTimes);
var medianTransaction = CalcMedian(transactionResults, storeTimes);

var devitationCool = CalcDeviation(coolResults, storeTimes, medianCool);
var devitationPremium = CalcDeviation(premiumResults, storeTimes, medianPremium);
var devitationHot = CalcDeviation(hotResults, storeTimes, medianHot);
var devitationTransaction = CalcDeviation(transactionResults, storeTimes, medianTransaction);


Console.WriteLine("Median cool: " + medianCool);
Console.WriteLine("Deviation cool: " + devitationCool);

Console.WriteLine("Median premium: " + medianPremium);
Console.WriteLine("Deviation premium: " + devitationPremium);

Console.WriteLine("Median hot: " + medianHot);
Console.WriteLine("Deviation hot: " + devitationHot);

Console.WriteLine("Median transaction: " + medianTransaction);
Console.WriteLine("Deviation transaction: " + devitationTransaction);

DateTimeOffset dto = new DateTimeOffset(DateTime.UtcNow);
string unixTime = dto.ToUnixTimeSeconds().ToString();


var coolResultFileName = "coolResults-" + unixTime + ".txt";
var premiumResultFileName = "premiumResults-" + unixTime + ".txt";
var hotResultFileName = "hotResults-" + unixTime + ".txt";
var transactionResultFileName = "transactionResults-" + unixTime + ".txt";

var coolResultsRow = "median: " + medianCool.ToString() + ", deviation: " + devitationCool.ToString() + "\n";
var premiumResultsRow = "median: " + medianPremium.ToString() + ", deviation: " + devitationPremium.ToString() + "\n";
var hotResultsRow = "median: " + medianHot.ToString() + ", deviation: " + devitationHot.ToString() + "\n";
var transactionResultsRow = "median: " + medianTransaction.ToString() + ", deviation: " + devitationTransaction.ToString() + "\n";

StoreResults(coolResults, resultsPath + coolResultFileName, coolResultsRow);
StoreResults(premiumResults, resultsPath + premiumResultFileName, premiumResultsRow);
StoreResults(hotResults, resultsPath + hotResultFileName, hotResultsRow);
StoreResults(transactionResults, resultsPath + transactionResultFileName, transactionResultsRow);

static void StoreResults( List<double> results, string path, string calcResults)
{
    var resultsString = results.Aggregate(calcResults, (acc, x) => acc + x.ToString() + "\n");
    File.WriteAllText(path, resultsString);
}

static List<double> RunStoreTest(int storeTimes, byte[] fileBytes, string basePath)
{
    var results = new List<double>();
    for (int i = 0; i < storeTimes + 1; i++)
    {
        var fileName = "file" + i.ToString() + ".txt";
        var path = Path.Combine(basePath, fileName);
        var startTime = DateTime.Now;
        File.WriteAllBytes(path, fileBytes);
        var endTime = DateTime.Now;
        var writeTime = (endTime - startTime).TotalMilliseconds;
        if (i != 0)
        {
            results.Add(writeTime);
        }

    }

    DirectoryInfo di = new(basePath);

    foreach (FileInfo file in di.GetFiles())
    {
        file.Delete();
    }

    return results;
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