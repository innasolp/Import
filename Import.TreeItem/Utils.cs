namespace Import.TreeItem;

public static class Utils
{
    private static readonly string urlDelimiter = "-";
    private static readonly string[] specialUrlSymbols = ["\\", "/"];

    public static bool TryGetIdFromUrl(string url, out int id)
    {
        var str = url;
        foreach (string symbol in specialUrlSymbols)
        {
            str = str.Replace(symbol, string.Empty);
        }

        var split = str.Split(urlDelimiter);
        return int.TryParse(split.Last(), out id);
    }

    public static int GetIdFromUrl(string url)
    {
        if (TryGetIdFromUrl(url, out int id))
            return id;

        throw new FormatException($"Invalid item url {url}.");
    }

    public static bool TryGetNameFromUrl(string url, out string name)
    {
        var formattedUrl = url;
        name = url;
        string[]? split = null;
        foreach (string symbol in specialUrlSymbols)
        {
            if (!formattedUrl.Contains(symbol)) continue;

            split = formattedUrl.Split(symbol);
            if (split.Length > 0)
                break;
        }

        formattedUrl = split?.LastOrDefault(s => s != string.Empty);
        if (string.IsNullOrEmpty(formattedUrl))
            return false;

        if (TryGetIdFromUrl(formattedUrl, out int id))
        {
            name = formattedUrl.Replace($"{urlDelimiter}{id}", string.Empty);
        }
        else
            name = formattedUrl;

        return true;
    }
}