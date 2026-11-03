namespace ServiceLib.Models.Configs;

[Serializable]
public class TorItem
{
    public int? ListenPort { get; set; }
    public bool? Snowflake { get; set; } = true;
    public bool? Lyrebird { get; set; } = true;
    public bool? Conjure { get; set; } = true;
    public bool? AutoFetch { get; set; } = true;
    public string? CustomBridges { get; set; }
}
