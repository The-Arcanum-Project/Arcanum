RWStructuredBuffer<uint> Histogram : register(u0);

[numthreads(256, 1, 1)]
void main(uint3 dispatch_thread_id : SV_DispatchThreadID)
{
    uint index = dispatch_thread_id.x;
    // (Add a bounds check here if you dispatch more threads than histogram size)
    Histogram[index] = 0;
}
