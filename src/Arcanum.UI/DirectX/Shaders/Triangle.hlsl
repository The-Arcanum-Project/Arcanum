cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
};

// The new structured buffer for our color lookup table
StructuredBuffer<float4> ColorLookup : register(t0);

struct VSInput
{
    float2 Position : POSITION;
    float2 CenterPosition: CENTER;
    uint PolygonId : POLYGON_ID; // Changed from Color
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color: COLOR; // Pass the ID to the pixel shader
};

PSInput VSMain(VSInput input)
{
    PSInput result;
    float2 localPos = input.Position - input.CenterPosition;
    float4 color = ColorLookup[input.PolygonId];
    
    float sin_a, cos_a;
    sincos(color.a, sin_a, cos_a);

    float2 rotated;
    rotated.x = localPos.x * cos_a - localPos.y * sin_a;
    rotated.y = localPos.x * sin_a + localPos.y * cos_a;
    color.a = 1.0f;
    float4 pos = float4(rotated + input.CenterPosition, 0.0f, 1.0f);
    result.Position = mul(pos, WorldViewProjection);
    result.Color = color;
    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    // Look up the color from the buffer using the PolygonId
    return input.Color;
}
