struct VS_INPUT
{
    // The input position is expected to be in Normalized Device Coordinates [-1, 1]
    float2 pos : POSITION;
};

struct PS_INPUT
{
    float4 pos : SV_POSITION;
};

PS_INPUT VSMain(VS_INPUT input)
{
    PS_INPUT output;
    // Pass the position straight through. No transformation!
    // Z must be between 0 and 1. W must be 1.
    output.pos = float4(input.pos, 0.5f, 1.0f);
    return output;
}

float4 PSMain(PS_INPUT input) : SV_TARGET
{
    // Return a constant color for the outline (e.g., semi-transparent yellow)
    return float4(1.0f, 0.0f, 0.0f, 1.0f);
}