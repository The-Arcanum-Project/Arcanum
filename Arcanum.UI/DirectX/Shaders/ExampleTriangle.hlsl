struct VS_INPUT
{
    float3 position : POSITION;
    float4 color : COLOR;
};

struct PS_INPUT
{
    float4 position : SV_POSITION;
    float4 color : COLOR;
};

PS_INPUT VSMain(VS_INPUT input)
{
    PS_INPUT output;
    // Pass the position directly to the rasterizer.
    // The 'w' component of 1.0f is important.
    output.position = float4(input.position, 1.0f);
    
    // Pass the color to the pixel shader.
    output.color = input.color;
    
    return output;
}

float4 PSMain(PS_INPUT input) : SV_TARGET
{
    // Return the interpolated color from the vertex shader.
    return input.color;
}