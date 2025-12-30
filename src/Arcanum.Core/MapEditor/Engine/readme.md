#### 1. **Foundation** (Shared utilities & Interfaces)

* **`Diagnostics/`**: `ILogger`, `PerformanceTimer`, `DebugOverlay`.
* **`Math/`**: Extensions for `System.Numerics`.
    * `Ray.cs`: Ray struct for picking.
    * `BoundingBox.cs`: AABB logic.
    * `MathUtils.cs`: Coordinate conversion (WorldToGrid, ScreenToRay).
* **`Input/`**:
    * `InputSnapshot.cs`: The struct sent by UI.
    * `IInputProvider.cs`: Interface the UI implements.
    * `KeyCodes.cs`: Abstracted keys (avoiding WPF Key enums).
* **`History/`**: The Undo/Redo System.
    * `ICommand.cs`: The base interface for an action.
    * `UndoStack.cs`: Manages the queue of commands.
    * `Transaction.cs`: Allows grouping multiple actions (e.g., a brush stroke) into one undo step.

#### 2. **Rendering** (The Graphics Engine)

* **`API/`**: Direct Silk.NET/D3D11 implementations.
    * `RenderContext.cs`: Manages Device/SwapChain.
    * `ShaderCompiler.cs`: Hot-reloads HLSL.
    * `StateManager.cs`: BlendStates, DepthStencilStates.
* **`Resources/`**: Wrappers around GPU memory.
    * `Buffers/`: `VertexBuffer`, `IndexBuffer`, `StructuredBuffer` (for Instancing).
    * `Textures/`: `Texture2D`, `RenderTexture`, `TextureArray`.
    * `Shaders/`: `ShaderProgram` (VS+PS wrapper).
* **`Scene/`**: High-level render objects.
    * `Camera.cs`: View/Projection logic.
    * `Mesh.cs`: Geometry definitions (Cube, Quad, Sphere).
    * `Material.cs`: Shader + Parameters.
* **`Passes/`**: Modular rendering logic.
    * `IRenderPass.cs`.
    * `GridPass.cs`: Renders the floor grid.
    * `GizmoPass.cs`: Renders selection handles.

#### 3. **Workspace** (Session Management)

* `Project.cs`: The root object holding the state of the loaded map.
* `Settings.cs`: Editor preferences (Camera speed, autosave).
* `IFileSystem.cs`: Abstracted file access (for VFS).

---

#### 4. **Modes** (The 3 Editors)

**`Modes/Shared/`**

* `IEditorMode.cs`: The interface (`Update`, `Render`, `Activate`).
* `SelectionManager.cs`: logic for handling what is currently selected.

**`Modes/Draft/`** (Procedural)

* **`Graph/`**: The node/spline data structure.
    * `DraftNode.cs`, `DraftConnection.cs`.
* **`Generators/`**: The math that runs on the graph.
    * `SplineGenerator.cs`, `VoronoiGenerator.cs`.
* **`Models/`**:
    * `SplineObject.cs`, `BiomeRegion.cs`.
* **`Commands/`**: `AddSplineNodeCommand.cs`, `MoveNodeCommand.cs`.

**`Modes/Sculpt/`** (Raster)

* **`Brushes/`**: Logic for painting.
    * `BrushSettings.cs`: Size, Flow, Jitter.
    * `BrushEngine.cs`: Calculates where to apply the stamp.
    * `Erosion/`: `HydraulicErosion.cs` (Compute Shader wrappers).
* **`Layers/`**: Managing the big textures.
    * `LayerStack.cs`.
    * `TileManager.cs`: Handling 512x512 chunks.
* **`Commands/`**: `PaintStrokeCommand.cs` (Be careful with memory here).

**`Modes/Nudge/`** (Metadata - **Start Here**)

* **`Entities/`**: The in-memory representation of Jomini data.
    * `NudgeObject.cs`: Base class.
    * `CityObject.cs`, `PortObject.cs`, `LocatorObject.cs`.
* **`Spatial/`**: Optimization for picking.
    * `QuadTree.cs` or `BVH.cs`: For finding objects quickly.
* **`Tools/`**: Interaction logic.
    * `SelectTool.cs`: Raycast logic.
    * `TranslateTool.cs`: Gizmo dragging logic.
    * `RotateTool.cs`, `ScaleTool.cs`.
    * `MagnetTool.cs`: Snapping logic.
* **`Rendering/`**: Nudge-specific renderers.
    * `InstanceRenderer.cs`: Manages the `StructuredBuffer` for cities.
    * `LabelRenderer.cs`: SDF Text rendering.
* **`Commands/`**:
    * `TransformObjectCommand.cs`: Handles Move/Rotate/Scale for Undo.
    * `DeleteObjectCommand.cs`.
    * `CreateObjectCommand.cs`.
