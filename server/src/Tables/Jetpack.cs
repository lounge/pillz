using SpacetimeDB;

namespace pillz.server.Tables;

[Type]
public partial struct Jetpack
{
    public float Fuel;
    public bool Enabled;
    public bool Throttling;
}
