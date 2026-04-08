namespace GerarCustoAbateDesossa.Infrastructure.Configuration;

internal sealed class IniFileReader
{
    private readonly Dictionary<string, Dictionary<string, string>> _data =
        new(StringComparer.OrdinalIgnoreCase);

    public static IniFileReader Load(string filePath)
    {
        var reader = new IniFileReader();
        reader.Parse(File.ReadAllLines(filePath));
        return reader;
    }

    public string GetValue(string section, string key, string defaultValue)
    {
        if (_data.TryGetValue(section, out var sectionData) &&
            sectionData.TryGetValue(key, out var value))
        {
            return value;
        }

        return defaultValue;
    }

    private void Parse(IEnumerable<string> lines)
    {
        var currentSection = string.Empty;

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith(';') || line.StartsWith('#'))
            {
                continue;
            }

            if (line.StartsWith('[') && line.EndsWith(']'))
            {
                currentSection = line[1..^1].Trim();
                if (!_data.ContainsKey(currentSection))
                {
                    _data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
                }

                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim();
            var value = line[(separatorIndex + 1)..].Trim();

            if (!_data.ContainsKey(currentSection))
            {
                _data[currentSection] = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            }

            _data[currentSection][key] = value;
        }
    }
}
