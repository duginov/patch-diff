using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace Patch;

// to be used for any patch loaders (e.g. csv, Excel-based etc)
public interface IPatchLoader
{
    Patch Load(string filePath);
}

public class CsvPatchLoader : IPatchLoader
{
    // well-known fields
    private const string BeginDate = "BeginDate";
    private const string EndDate = "EndDate";

    /// <summary>
    /// Loads a patch from csv file into Patch object
    /// </summary>
    /// <param name="filePath"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public Patch Load(string filePath)
    {
        var patch = new Patch { FileName = filePath };
        using var reader = new StreamReader(filePath);
        var csvConfig = new CsvConfiguration(CultureInfo.InvariantCulture) { Delimiter = ",", TrimOptions = TrimOptions.Trim };
        using var csvParser = new CsvParser(reader, csvConfig);
        
        var headers = ReadHeaders(csvParser, filePath);

        static bool IsDateField(string h) => h is BeginDate or EndDate;

        var key = headers.First(h=>!IsDateField(h));
        var keyIndex = headers.IndexOf(key);
        patch.KeyName = key;
        patch.ValueFields = headers
            .Where(h => h != key && !IsDateField(h))
            .ToList();
        var beginDateIndex = headers.IndexOf(BeginDate);
        var endDateIndex = headers.IndexOf(EndDate);

        ThrowForMissingDateFields(beginDateIndex, endDateIndex, filePath);

        int[] nonValueIndexes = [keyIndex, beginDateIndex, endDateIndex]; // contains the key and date field indices

        // read all the lines
        while (csvParser.Read())
        {
            var line = new PatchLine();
            var row = csvParser.Record!.ToList();
            line.Key = new MasterKey(row[keyIndex], row[beginDateIndex], row[endDateIndex]);

            // read and process every field, skipping non-value fields (e.g. key, from/to dates)
            for (var columnIndex = 0; columnIndex < row.Count; columnIndex++)
            {
                if (nonValueIndexes.Contains(columnIndex))
                {
                    continue;
                }

                var lineWasAdded = line.Values.TryAdd(headers[columnIndex],
                                                        string.IsNullOrEmpty(row[columnIndex]) ? null : row[columnIndex]); // nulls are easy to compare later
                if (!lineWasAdded)
                {
                    throw new Exception(
                        $"Duplicate column name {headers[columnIndex]} detected in {filePath}");
                }
            }

            patch.Lines.Add(line);
        }
        return patch; 
    }

    private static void ThrowForMissingDateFields(int beginDateIndex, int endDateIndex, string filePath)
    {
        var sb = new StringBuilder();
        if (beginDateIndex < 0)
        {
            sb.AppendLine($"Column BeginDate is missing in {filePath}");
        }
        if (endDateIndex < 0)
        {
            sb.AppendLine($"Column EndDate is missing in {filePath}");
        }

        if (sb.Length > 0)
        {
            throw new KeyNotFoundException(sb.ToString());
        }
    }

    private static List<string> ReadHeaders(CsvParser parser, string filePath)
    {
        var hasMoreLines = parser.Read();
        if (parser.Record == null)
        {
            throw new Exception($"Could not read the header in {filePath}");
        }

        if (!hasMoreLines)
        {
            throw new Exception($"No data in {filePath}");
        }

        var headers = parser.Record!.ToList();

        if (headers.Count < 4)
        {
            throw new Exception($"File {filePath} contains less then four columns (key, BeginDate, EndDate and at least one value column) ");
        }

        return headers;
    }
}