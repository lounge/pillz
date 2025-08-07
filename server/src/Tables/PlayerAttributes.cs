namespace pillz.server.Tables;

[SpacetimeDB.Type]
public partial struct PlayerAttributes
{
    public float Fuel;
    public bool JetpackEnabled;
    public bool IsThrottling;
    
    public PlayerAttributes(uint fuel, bool jetpackEnabled, bool isThrottling)
    {
        Fuel = fuel;
        JetpackEnabled = jetpackEnabled;
        IsThrottling = isThrottling;
    }
}