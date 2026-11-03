namespace ServiceLib.Tor;

public enum TorListenPortProblem
{
    NotAPort,
    LocalProxy,
    AetherPort,
}

public static class TorPorts
{
    public const int DefaultListenPort = 10829;
    public const int FirstListenPort = 1024;
    public const int LastListenPort = 65535;

    public static int ListenPortOf(string? text)
    {
        return ListenPortOf(AetherDigits.ToIntOrNull(text?.Trim()));
    }

    public static int ListenPortOf(int? port)
    {
        return port is >= FirstListenPort and <= LastListenPort ? port.Value : DefaultListenPort;
    }

    public static TorListenPortProblem? ListenPortProblem(
        string text,
        IEnumerable<int> localPorts,
        int aetherPort = AetherPorts.DefaultListenPort)
    {
        var port = AetherDigits.ToIntOrNull(text.Trim());
        if (port is not (>= FirstListenPort and <= LastListenPort))
        {
            return TorListenPortProblem.NotAPort;
        }
        if (localPorts.Contains(port.Value))
        {
            return TorListenPortProblem.LocalProxy;
        }
        if (AetherPorts.IsReserved(port.Value, aetherPort))
        {
            return TorListenPortProblem.AetherPort;
        }
        return null;
    }
}
