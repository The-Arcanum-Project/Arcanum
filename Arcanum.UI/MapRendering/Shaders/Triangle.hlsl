cbuffer Constants : register(b0)
{
    float4x4 WorldViewProjection;
};

struct VSInput
{
    float4 Position : POSITION;
    float4 Color : COLOR;
};

struct PSInput
{
    float4 Position : SV_POSITION;
    float4 Color : COLOR;
};

PSInput VSMain(VSInput input)
{
    PSInput result;
    result.Position = input.Position;
    result.Position = mul(input.Position, WorldViewProjection);
    result.Color = input.Color;
    return result;
}

float4 PSMain(PSInput input) : SV_TARGET
{
    return input.Color;
}