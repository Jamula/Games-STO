using System.Text;

namespace STO.Core.Services;

/// <summary>
/// Lightweight CSV parser and writer that handles quoted fields.
/// </summary>
public static class CsvHelper
{
    /// <summary>
    /// Parses a CSV stream into rows of string arrays. The first row is treated as a header.
    /// Returns (headers, dataRows).
    /// </summary>
    public static (string[] Headers, List<string[]> Rows) Parse(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, leaveOpen: true);
        var lines = new List<string[]>();

        string? line;
        while ((line = reader.ReadLine()) is not null)
        {
            if (string.IsNullOrWhiteSpace(line))
                continue;

            lines.Add(ParseLine(line));
        }

        if (lines.Count == 0)
            return ([], []);

        var headers = lines[0].Select(h => h.Trim()).ToArray();
        var rows = lines.Skip(1).ToList();
        return (headers, rows);
    }

    /// <summary>
    /// Parses a single CSV line, respecting quoted fields that may contain commas and escaped quotes.
    /// </summary>
    public static string[] ParseLine(string line)
    {
        var fields = new List<string>();
        var current = new StringBuilder();
        bool inQuotes = false;

        for (int i = 0; i < line.Length; i++)
        {
            char c = line[i];

            if (inQuotes)
            {
                if (c == '"')
                {
                    // Escaped quote ("") or end of quoted field
                    if (i + 1 < line.Length && line[i + 1] == '"')
                    {
                        current.Append('"');
                        i++; // skip next quote
                    }
                    else
                    {
                        inQuotes = false;
                    }
                }
                else
                {
                    current.Append(c);
                }
            }
            else
            {
                if (c == '"')
                {
                    inQuotes = true;
                }
                else if (c == ',')
                {
                    fields.Add(current.ToString().Trim());
                    current.Clear();
                }
                else
                {
                    current.Append(c);
                }
            }
        }

        fields.Add(current.ToString().Trim());
        return [.. fields];
    }

    /// <summary>
    /// Writes CSV rows to a stream, quoting fields that contain commas, quotes, or newlines.
    /// </summary>
    public static MemoryStream Write(string[] headers, IEnumerable<string[]> rows)
    {
        var ms = new MemoryStream();
        using (var writer = new StreamWriter(ms, Encoding.UTF8, leaveOpen: true))
        {
            writer.WriteLine(string.Join(",", headers.Select(Escape)));
            foreach (var row in rows)
            {
                writer.WriteLine(string.Join(",", row.Select(Escape)));
            }

            writer.Flush();
        }

        ms.Position = 0;
        return ms;
    }

    private static string Escape(string field)
    {
        if (field.Contains(',') || field.Contains('"') || field.Contains('\n'))
            return $"\"{field.Replace("\"", "\"\"")}\"";

        return field;
    }
}
