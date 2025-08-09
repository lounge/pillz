namespace pillz.server.Tables;

[SpacetimeDB.Type]
public partial struct JetpackInput
{
    public float Fuel;
    public bool Enabled;
    public bool Throttling;
    
    public JetpackInput(uint fuel, bool enabled, bool throttling)
    {
        Fuel = fuel;
        Enabled = enabled;
        Throttling = throttling;
    }
}