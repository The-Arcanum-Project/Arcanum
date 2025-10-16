// This struct defines the data we'll send for EACH rectangle.
// It now includes two colors.
struct InstanceData
{
    float2 offset; // Top-left corner of the rectangle
    float2 size; // Width and height
    float4 color1; // Top-left color
    float4 color2; // Bottom-right color
};

// The output of the Vertex Shader, which becomes the input for the Pixel Shader.
// We now pass the vertex's local position (from 0 to 1) to the pixel shader.
struct VS_OUTPUT
{
    float4 position : SV_Position; // Final clip-space position
    float4 color1 : COLOR0; // First color (passed through)
    float4 color2 : COLOR1; // Second color (passed through)
    float2 local_pos : TEXCOORD0; // The original vertex position (0,0), (1,0), etc.
};

cbuffer Constants : register(b0)
{
    float2 renderTargetSize;
};


VS_OUTPUT VS(
    // Per-Vertex input
    float2 vertexPos : POSITION,

    // Per-Instance input
    float2 instanceOffset : INSTANCE_OFFSET,
    float2 instanceSize : INSTANCE_SIZE,
    float4 instanceColor1 : INSTANCE_COLOR0, // Updated semantic
    float4 instanceColor2 : INSTANCE_COLOR1, // New color

    uint instanceId : SV_InstanceID)
{
    VS_OUTPUT output;

    // 1. Scale and translate the vertex (same as before)
    float2 pixelPos = (vertexPos * instanceSize) + instanceOffset;
    float2 ndcPos = (pixelPos / renderTargetSize) * 2.0f - 1.0f;
    ndcPos.y = -ndcPos.y;

    // 2. Set the final position and pass through the data.
    output.position = float4(ndcPos, 0.0f, 1.0f);
    output.color1 = instanceColor1;
    output.color2 = instanceColor2;

    // 3. Pass the original vertex position to the pixel shader for interpolation.
    // This 'vertexPos' will be (0,0) for the top-left vertex, (1,1) for the
    // bottom-right, and smoothly interpolated for every pixel in between.
    output.local_pos = vertexPos;

    return output;
}


float4 PS(VS_OUTPUT input) : SV_Target
{
    // The 'input.local_pos' value is the magic here. The GPU's rasterizer
    // has automatically interpolated it for this specific pixel.
    // For a pixel at the center of the rectangle, local_pos will be (0.5, 0.5).

    // We can use this to create the gradient.
    // A simple linear interpolation (lerp) is not a perfect diagonal gradient,
    // but it's very close and extremely fast.
    // We can average the X and Y components to get a diagonal weight.
    float weight = (input.local_pos.x + input.local_pos.y) / 2.0f;

    // Linearly interpolate between the two colors based on the weight.
    float4 final_color = lerp(input.color1, input.color2, weight);

    return final_color;
}
