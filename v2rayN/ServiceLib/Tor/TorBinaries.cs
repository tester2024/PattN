namespace ServiceLib.Tor;

public static class TorBinaries
{
    public const string CoreName = "tor-core";
    public const string ConjureName = "conjure-client";
    public const string LyrebirdName = "lyrebird";

    public static IReadOnlyList<string> Names { get; } = [CoreName, ConjureName, LyrebirdName];

    public static string Folder => Utils.GetBinPath("", "tor");

    public static string CoreFile => AetherBinaries.FileIn(Folder, CoreName);

    public static string ConjureFile => AetherBinaries.FileIn(Folder, ConjureName);

    public static string LyrebirdFile => AetherBinaries.FileIn(Folder, LyrebirdName);

    public static bool IsCoreAvailable() => AetherBinaries.IsRunnable(CoreFile);

    public static bool IsConjureAvailable() =>
        AetherBinaries.IsRunnable(ConjureFile) || AetherBinaries.IsRunnable(AetherBinaries.FileIn(AetherBinaries.Folder, ConjureName));

    public static bool IsLyrebirdAvailable() =>
        AetherBinaries.IsRunnable(LyrebirdFile) || AetherBinaries.IsLyrebirdAvailable();
}
