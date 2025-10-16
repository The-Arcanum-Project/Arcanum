// This struct defines the data we'll send for EACH rectangle we want to draw.
// We will use "instancing" to draw all rectangles in a single command.
struct InstanceData
{
    float2 offset; // Top-left corner of the rectangle (in pixels)
    float2 size; // Width and height of the rectangle (in pixels)
    float4 color; // The average color for this rectangle
};

// This struct defines the output of the Vertex Shader, which becomes
// the input for the Pixel Shader.
struct VS_OUTPUT
{
    float4 position : SV_Position; // The final clip-space position of the vertex
    float4 color : COLOR; // The color for this instance, passed to the pixel shader
};

// We will pass the total render target dimensions to the shader
// to correctly calculate the final vertex positions.
cbuffer Constants : register(b0)
{
    float2 renderTargetSize;
};


VS_OUTPUT VS(
    // Per-Vertex input: A simple position for one of the 4 corners of our "template" quad.
    // We will draw a simple (0,0) to (1,1) quad and scale/translate it for each instance.
    float2 vertexPos : POSITION,

    // Per-Instance input: The specific data for the rectangle we are currently drawing.
    float2 instanceOffset : INSTANCE_OFFSET,
    float2 instanceSize : INSTANCE_SIZE,
    float4 instanceColor : INSTANCE_COLOR,

    // The instance ID, provided by the system.
    uint instanceId : SV_InstanceID)
{
    VS_OUTPUT output;

    // 1. Scale and translate the template vertex to create the rectangle in pixel space.
    float2 pixelPos = (vertexPos * instanceSize) + instanceOffset;

    // 2. Convert pixel coordinates to Normalized Device Coordinates (NDC) [-1, 1].
    // This is the coordinate system the GPU rasterizer understands.
    float2 ndcPos = (pixelPos / renderTargetSize) * 2.0f - 1.0f;

    // 3. Flip the Y coordinate because screen space (0,0 at top-left) is the
    // inverse of NDC space (0,0 at center, +Y is up).
    ndcPos.y = -ndcPos.y;

    // 4. Set the final position and pass the color through.
    output.position = float4(ndcPos, 0.0f, 1.0f);
    output.color = instanceColor;

    return output;
}

// The Pixel Shader is very simple. It just receives the color from the
// Vertex Shader and returns it to be written to the render target.
float4 PS(VS_OUTPUT input) : SV_Target
{
    return input.color;
}
