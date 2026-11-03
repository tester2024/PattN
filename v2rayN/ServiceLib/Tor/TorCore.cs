using System.Globalization;

namespace ServiceLib.Tor;

public sealed record TorSettings(
    int ListenPort = TorPorts.DefaultListenPort,
    bool Snowflake = true,
    bool Lyrebird = true,
    bool Conjure = true,
    bool AutoFetch = true,
    string CustomBridges = "")
{
    public static TorSettings From(TorItem? item) => new(
        ListenPort: TorPorts.ListenPortOf(item?.ListenPort),
        Snowflake: item?.Snowflake ?? true,
        Lyrebird: item?.Lyrebird ?? true,
        Conjure: item?.Conjure ?? true,
        AutoFetch: item?.AutoFetch ?? true,
        CustomBridges: item?.CustomBridges ?? string.Empty);
}

public sealed record TorCore(
    int Port = TorPorts.DefaultListenPort,
    bool Snowflake = true,
    bool Lyrebird = true,
    bool Conjure = true,
    bool AutoFetch = true,
    IReadOnlyList<string>? CustomBridges = null,
    int? UpstreamPort = null)
{
    public const string CommandName = "tor-core";

    public IReadOnlyList<string> Bridges => CustomBridges ?? [];

    public IReadOnlyList<string> Arguments
    {
        get
        {
            var args = new List<string>
            {
                "--bind",
                string.Create(CultureInfo.InvariantCulture, $"{Global.Loopback}:{Port}"),
                $"--snowflake={(Snowflake ? "true" : "false")}",
                $"--lyrebird={(Lyrebird ? "true" : "false")}",
                $"--conjure={(Conjure ? "true" : "false")}",
                $"--auto-fetch={(AutoFetch ? "true" : "false")}",
            };

            foreach (var bridge in Bridges)
            {
                args.AddRange(["--bridge", bridge]);
            }

            if (UpstreamPort is { } upstream)
            {
                args.AddRange([
                    "--upstream",
                    string.Create(CultureInfo.InvariantCulture, $"socks5://{Global.Loopback}:{upstream}"),
                ]);
            }

            return args;
        }
    }

    public string Command => string.Join(' ', new[] { CommandName }.Concat(Arguments).Select(Quoted));

    public IReadOnlyList<string> ActiveTransports
    {
        get
        {
            var list = new List<string>();
            if (Snowflake) list.Add("Snowflake");
            if (Lyrebird) list.Add("Lyrebird");
            if (Conjure) list.Add("Conjure");
            return list;
        }
    }

    public TorCore Through(int port) => this with { UpstreamPort = port };

    public static TorCore Of(ProfileItem profile, int listenPort = TorPorts.DefaultListenPort, TorSettings? defaults = null)
    {
        var fallback = defaults ?? new TorSettings(ListenPort: listenPort);
        var extra = profile.GetProtocolExtra();
        var rawBridges = KotlinText.NullIfBlank(extra.TorCustomBridges) ?? fallback.CustomBridges;

        return new TorCore(
            Port: listenPort,
            Snowflake: extra.TorSnowflake ?? fallback.Snowflake,
            Lyrebird: extra.TorLyrebird ?? fallback.Lyrebird,
            Conjure: extra.TorConjure ?? fallback.Conjure,
            AutoFetch: extra.TorAutoFetch ?? fallback.AutoFetch,
            CustomBridges: BridgeLines(rawBridges));
    }

    public static List<string> BridgeLines(string? text)
    {
        return AetherFmt.BridgeLines((text ?? string.Empty).Replace(';', '\n'))
            .Where(line => !line.StartsWith('-'))
            .Distinct(StringComparer.Ordinal)
            .ToList();
    }

    private static string Quoted(string word)
    {
        return word.Length == 0 || word.Any(KotlinText.IsWhitespace) ? $"\"{word}\"" : word;
    }
}
