RWStructuredBuffer<uint> GlobalHistogram : register(u0);

[numthreads(256, 1, 1)]
void main(uint3 dispatch_thread_id : SV_DispatchThreadID)
{
    uint total_size, _;
    GlobalHistogram.GetDimensions(total_size, _);
    if (dispatch_thread_id.x < total_size)
    {
        GlobalHistogram[dispatch_thread_id.x] = 0;
    }
}
