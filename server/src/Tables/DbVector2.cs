namespace masks.server.Tables;

[SpacetimeDB.Type]
public partial struct DbVector2
{
    public float X;
    public float Y;

    public DbVector2(float x, float y)
    {
        X = x;
        Y = y;
    }

    public float SqrMagnitude => X * X + Y * Y;
    public float Magnitude => MathF.Sqrt(SqrMagnitude);
    public DbVector2 Normalized
    {
        get
        {
            var mag = Magnitude;
            if (mag > float.Epsilon)
                return this / mag;
            return new DbVector2(0, 0);
        }
    }

    public static DbVector2 operator +(DbVector2 a, DbVector2 b) => new(a.X + b.X, a.Y + b.Y);
    public static DbVector2 operator -(DbVector2 a, DbVector2 b) => new(a.X - b.X, a.Y - b.Y);
    public static DbVector2 operator *(DbVector2 a, float b) => new(a.X * b, a.Y * b);
    public static DbVector2 operator /(DbVector2 a, float b) => new(a.X / b, a.Y / b);
}