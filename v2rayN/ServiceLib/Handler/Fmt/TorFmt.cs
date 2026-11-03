namespace ServiceLib.Handler.Fmt;

public enum TorProblem
{
    ListenPortTaken,
    BridgesMissing,
}

public class TorFmt : BaseFmt
{
    private const string DefaultRemarks = "Tor";

    public static ProfileItem? Resolve(string str, out string msg)
    {
        msg = ResUI.ConfigurationFormatIncorrect;

        var url = Utils.TryUri(str);
        if (url == null)
        {
            return null;
        }

        var query = Utils.ParseQueryString(url.Query);
        var remarks = url.GetComponents(UriComponents.Fragment, UriFormat.Unescaped);
        var bridges = query["bridges"] is { } raw
            ? string.Join("\n", TorCore.BridgeLines(raw))
            : null;

        ProfileItem item = new()
        {
            ConfigType = EConfigType.Tor,
            CoreType = ECoreType.Xray,
            Remarks = remarks.Length > 0 ? remarks : DefaultRemarks,
        };

        item.SetProtocolExtra(item.GetProtocolExtra() with
        {
            TorSnowflake = ParseBool(query["snowflake"], true),
            TorLyrebird = ParseBool(query["lyrebird"], true),
            TorConjure = ParseBool(query["conjure"], true),
            TorAutoFetch = ParseBool(query["auto_fetch"], true),
            TorCustomBridges = string.IsNullOrEmpty(bridges) ? null : bridges,
        });

        return item;
    }

    public static string? ToUri(ProfileItem? item)
    {
        if (item == null)
        {
            return null;
        }

        var core = TorCore.Of(item);
        var query = new List<(string Key, string Value)>
        {
            ("snowflake", core.Snowflake ? "1" : "0"),
            ("lyrebird", core.Lyrebird ? "1" : "0"),
            ("conjure", core.Conjure ? "1" : "0"),
            ("auto_fetch", core.AutoFetch ? "1" : "0"),
        };

        if (core.Bridges.Count > 0)
        {
            query.Add(("bridges", string.Join(";", core.Bridges)));
        }

        var queryText = string.Join("&", query.Select(it => $"{it.Key}={Utils.UrlEncode(it.Value)}"));
        return $"{Global.ProtocolShares[EConfigType.Tor]}?{queryText}#{Utils.UrlEncode(item.Remarks ?? string.Empty)}";
    }

    public static TorProblem? Normalize(ProfileItem config, IEnumerable<int>? takenPorts, int listenPort)
    {
        var extra = config.GetProtocolExtra();
        var bridges = TorCore.BridgeLines(extra.TorCustomBridges);
        config.SetProtocolExtra(extra with
        {
            TorCustomBridges = bridges.Count > 0 ? string.Join("\n", bridges) : null,
        });

        var autoFetch = extra.TorAutoFetch != false;
        var anyTransport = extra.TorSnowflake != false || extra.TorLyrebird != false || extra.TorConjure != false;
        if (!autoFetch && bridges.Count == 0 && !anyTransport)
        {
            return TorProblem.BridgesMissing;
        }

        if (takenPorts?.Contains(listenPort) == true)
        {
            return TorProblem.ListenPortTaken;
        }

        return null;
    }

    private static bool ParseBool(string? raw, bool fallback) => raw?.Trim().ToLowerInvariant() switch
    {
        "1" or "true" or "yes" or "on" => true,
        "0" or "false" or "no" or "off" => false,
        _ => fallback,
    };
}
