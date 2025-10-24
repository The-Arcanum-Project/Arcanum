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

struct BorderVSInput
{
    float2 Position   : POSITION;
    uint   BorderId   : BORDER_ID;
    float2 TexCoord   : TEXCOORD0; // The semantic name TEXCOORD0 must match the C# InputLayout
};

struct BorderPSInput
{
    float4 Position  : SV_POSITION;
    uint   BorderId  : BORDER_ID;
    float2 TexCoord  : TEXCOORD0;
};

struct BorderProperties
{
    float4 Color;
    uint   StyleIndex;
    // The padding fields from C# are not needed here, HLSL handles alignment.
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

BorderPSInput BorderVSMain(BorderVSInput input) // <-- Changed input type
{
    BorderPSInput output;
    float4 position = float4(input.Position, 0.0f, 1.0f);
    output.Position = mul(position, WorldViewProjection);
    output.BorderId = input.BorderId;
    output.TexCoord = input.TexCoord; // <-- Pass the TexCoord through
    return output;
}

StructuredBuffer<BorderProperties> BorderData : register(t0);

float4 BorderPSMain(BorderPSInput input) : SV_Target
{
    // Use the BorderId to look up the full properties struct for this border.
    //BorderProperties props = BorderData[input.BorderId];

    // For now, we just return the color from the properties.
    // In the future, you could use 'props.StyleIndex' to create different
    // border styles (e.g., dashes, dots).
    return float4(input.TexCoord.x, input.TexCoord.y, 0, 1);
}