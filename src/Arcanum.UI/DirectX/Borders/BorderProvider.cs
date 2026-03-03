using Arcanum.Core.Utils;
using Vortice.Direct3D11;
using Vortice.DXGI;

namespace Arcanum.UI.DirectX.Borders;

public static class BorderProvider
{
    public static unsafe ID3D11ShaderResourceView GenerateBorderAtlas(ID3D11Device device)
    {
        var desc = new Texture2DDescription
        {
            Width = BorderConfig.BORDER_ATLAS_WIDTH,
            Height = BorderConfig.SLICE_SIZE,
            ArraySize = BorderConfig.NUMBER_OF_BORDER_TYPES,
            Format = Format.R8_UNorm, // 1-byte Grayscale (Red channel)
            SampleDescription = new(1, 0),
            Usage = ResourceUsage.Default,
            BindFlags = BindFlags.ShaderResource,
            CPUAccessFlags = CpuAccessFlags.None,
            MiscFlags = ResourceOptionFlags.None
        };
        
        const int sliceLength = BorderConfig.BORDER_ATLAS_WIDTH * BorderConfig.SLICE_SIZE;
        
        
        SubresourceData[] subresources = new SubresourceData[BorderConfig.NUMBER_OF_BORDER_TYPES];

        // 1. Load the entire atlas pixel data into a single byte array
        if (!ArcResources.GetResourceBytes(BorderConfig.BORDER_ATLAS_PATH, typeof(BorderProvider).Assembly,
                out var bytes))
            throw new ("Failed to load border atlas");
        
        // 2. Validate the loaded data size matches our expected total size
        if (bytes.Length != sliceLength * BorderConfig.NUMBER_OF_BORDER_TYPES)
            throw new ("Border atlas data size mismatch. Expected: " + (sliceLength * BorderConfig.NUMBER_OF_BORDER_TYPES) + " bytes, but got: " + bytes.Length + " bytes.");
        
        // 3. Pin the single large array ONCE using a fixed block
        fixed (byte* pPixels = bytes)
        {
            for (var i = 0; i < BorderConfig.NUMBER_OF_BORDER_TYPES; i++)
            {
                // 4. Calculate the pointer offset for this specific slice
                subresources[i] = new()
                {
                    DataPointer = (IntPtr)(pPixels + (i * sliceLength)),
                    RowPitch = BorderConfig.BORDER_ATLAS_WIDTH,           // 16 bytes per row
                    SlicePitch = sliceLength         // Total bytes in this slice
                };
            }

            // 5. Create the Texture2D Array on the GPU while the memory is pinned
            // (Using 'using' block so the base texture disposes automatically at the end of the method)
            using var textureArray = device.CreateTexture2D(desc, subresources);
            
            // 6. Create the Shader Resource View
            return device.CreateShaderResourceView(textureArray);
        } // <-- The GC automatically unpins the array here when the fixed block ends!
    }
}