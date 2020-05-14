using Unity.Mathematics;

public static class MathHelper
{
    public static bool IsPointInSegment(float2 a, float2 b, float2 p)
    {
        var epsilon = 0.1f;

        var ba = b - a;
        var pa = p - a;

        if (math.length(math.cross(new float3(ba, 0), new float3(pa, 0))) <= epsilon)
        {
            var dot = math.dot(ba, pa);
            if (dot > 0)
            {
                var dist = math.distance(a, b);
                if (dot < dist * dist)
                {
                    return true;
                }
            }
        }

        return false;
    }

    public static float2 GetClosestPoint(float2 a, float2 b, float2 p)
    {
        var segment = b - a;
        var lenght = math.length(segment);
        segment = math.normalize(segment);

        var pa = p - a;
        var d = math.dot(pa, segment);
        d = math.clamp(d, 0f, lenght);

        return a + segment * d;
    }
}