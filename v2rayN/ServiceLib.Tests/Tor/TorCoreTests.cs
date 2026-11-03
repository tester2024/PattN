using TUnit.Assertions.Enums;

namespace ServiceLib.Tests.Tor;

public class TorCoreTests
{
    [Test]
    public async Task BuildsArgumentsAndActiveTransportsFromProfile()
    {
        var profile = new ProfileItem
        {
            ConfigType = EConfigType.Tor,
            Remarks = "Tor Node",
        };
        profile.SetProtocolExtra(new ProtocolExtraItem
        {
            TorSnowflake = true,
            TorLyrebird = false,
            TorConjure = true,
            TorAutoFetch = false,
            TorCustomBridges = """
                # comment
                Bridge conjure 23.128.36.60:80 A13CE1DE88F96022C0CE6CCF6436FC6F5401D35B url=https://example.org
                snowflake 192.0.2.3:80 2B280B23E1107BB62ABFC40DDCC8824814F80A72
                """,
        });

        var core = TorCore.Of(profile, 19050, new TorSettings()).Through(19053);

        await core.ActiveTransports.Should().BeEquivalentTo(["Snowflake", "Conjure"], CollectionOrdering.Matching);
        await core.Arguments.Should().BeEquivalentTo(
            [
                "--bind", "127.0.0.1:19050",
                "--snowflake=true",
                "--lyrebird=false",
                "--conjure=true",
                "--auto-fetch=false",
                "--bridge", "conjure 23.128.36.60:80 A13CE1DE88F96022C0CE6CCF6436FC6F5401D35B url=https://example.org",
                "--bridge", "snowflake 192.0.2.3:80 2B280B23E1107BB62ABFC40DDCC8824814F80A72",
                "--upstream", "socks5://127.0.0.1:19053",
            ],
            CollectionOrdering.Matching);
    }

    [Test]
    public async Task ValidatesTorListenPortAgainstLocalProxyAndAether()
    {
        await TorPorts.ListenPortOf((string?)null).Should().BeEqualTo(TorPorts.DefaultListenPort);
        await TorPorts.ListenPortOf("80").Should().BeEqualTo(TorPorts.DefaultListenPort);
        await TorPorts.ListenPortOf("19050").Should().BeEqualTo(19050);

        await TorPorts.ListenPortProblem("19050", [10808, 10809], 10819).Should().BeNull();
        await TorPorts.ListenPortProblem("70000", [10808], 10819).Should().BeEqualTo(TorListenPortProblem.NotAPort);
        await TorPorts.ListenPortProblem("10808", [10808, 10809], 10819).Should().BeEqualTo(TorListenPortProblem.LocalProxy);
        await TorPorts.ListenPortProblem("10820", [10808], 10819).Should().BeEqualTo(TorListenPortProblem.AetherPort);
    }

    [Test]
    public async Task ParsesAndSerializesTorUri()
    {
        var original = new ProfileItem
        {
            ConfigType = EConfigType.Tor,
            Remarks = "My Tor",
        };
        original.SetProtocolExtra(new ProtocolExtraItem
        {
            TorSnowflake = false,
            TorLyrebird = true,
            TorConjure = true,
            TorAutoFetch = true,
            TorCustomBridges = "obfs4 192.0.2.1:443 AAAA cert=xyz iat-mode=0",
        });

        var uri = TorFmt.ToUri(original);
        await uri.Should().NotBeNull();

        var parsed = TorFmt.Resolve(uri!, out _);
        await parsed.Should().NotBeNull();
        await parsed!.Remarks.Should().BeEqualTo("My Tor");
        var extra = parsed.GetProtocolExtra();
        await extra.TorSnowflake.Should().BeEqualTo(false);
        await extra.TorLyrebird.Should().BeEqualTo(true);
        await extra.TorConjure.Should().BeEqualTo(true);
        await extra.TorAutoFetch.Should().BeEqualTo(true);
        await extra.TorCustomBridges.Should().BeEqualTo("obfs4 192.0.2.1:443 AAAA cert=xyz iat-mode=0");
    }
}
