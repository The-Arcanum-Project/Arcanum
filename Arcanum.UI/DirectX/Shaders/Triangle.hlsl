cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
};

// The new structured buffer for our color lookup table
StructuredBuffer<float4> ColorLookup : register(t0);

struct VSInput
{
    float2 Position   : POSITION;
    uint   PolygonId  : POLYGON_ID; // Changed from Color
};

struct PSInput
{
    float4 Position   : SV_POSITION;
    uint   PolygonId  : POLYGON_ID; // Pass the ID to the pixel shader
};

PSInput VSMain(VSInput input)
{
    PSInput result;
    float4 pos = float4(input.Position.x, input.Position.y, 0.0f, 1.0f);
    result.Position = mul(pos, WorldViewProjection);
    result.PolygonId = input.PolygonId;
    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    // Look up the color from the buffer using the PolygonId
    return ColorLookup[input.PolygonId];
}