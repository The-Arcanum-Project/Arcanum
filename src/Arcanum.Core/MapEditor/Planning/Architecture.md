# Architecture

We have **3** exclusive modes the user can work in:

* **Draft**-Mode (Procedural Generation / Planning)
* **Sculpt**-Mode (Raster Editing)
* **Nudge**-Mode (Metadata Editing)

## Global Editor Features

- **The Linter (Error List):** A collapsible bottom panel present in all modes that lists Warnings/Errors (e.g., "City
  ID 405 is underwater", "River flows uphill"). Clicking an error snaps the camera to that object.
- Resize/Expand Map: A utility to add pixels to any side of the map, automatically applying coordinate offsets to all
  Nudge objects and Vector shapes to keep them geographically locked.
- Dockable panel to manage imported reference images.
- Support for Opacity and Blend Modes (Multiply, Difference, Add) to overlay references on top of the terrain

---

## 1. Draft Mode

The user creates arbitrary objects (vectors/data) which define how a map should look via procedural generation.

### History

- Full undo / redo support.
- Non-destructive changes (Project File based).
- Changes can be "Baked" destructively to the map (transition to Sculpt Mode), which clears history.

### Moving / Rotating of Objects

- Show gizmos to move objects in 2D space.
- Show bounds to resize the object.
- Show all control points for Splines/Polygons.
- Allow for rotation of objects in 2 dimensions.
- **Smart Masking:** Allow objects to act as masks for others (e.g., drag a Spline into a Noise Mask slot to confine
  noise to that area).

### Loading

- Customized loading steps to load data from files.
- Parsing of custom `.pgp` files to load procedural data.
- **Palette System:** Load abstract definitions (e.g., "Wilderness") rather than raw colors, allowing global updates by
  changing the palette definition.

### Saving

- **Baking:** Required to export the final result (converts math to pixels).
- Parsing of custom `.pgp` files to save procedural data.

### Editor Design

- **Right Column:** **Hierarchy / Outliner** (Layer view of splines, gradients, masks).
- **Center Column:** High-performance Interactive Map (DirectX 11).
- **Left Column:** **Properties Panel** (Dynamic UI based on selected object).
- **Visuals:**
    - **Ghost Layers:** Option to display the underlying "Sculpt" layer (faded) as a reference to align procedural
      elements with existing terrain.
    - **Height-Based Texturing Preview:** Real-time shader preview of biomes based on slope/height rules.
- **Context Menus:** Standard Create/Edit menus on RMB.

#### Automated Placement

- Calculate best suggestions for object positions.
- Optimize objects (e.g., even out splines, smooth gradients).
- Automatic coastal water generation.
- Automatic ocean crossing generation.
- Automatic subdivision of Locations.

#### Filter Options

- Types: Spline, Gradient, Image, Mask, Biome Region.

---

## 2. Sculpt Mode

The user directly edits the map pixels (Heightmap, Provinces, Terrain) in 2D space.

### History

- **Tile-Based Undo:** Limited undo/redo support (snapshots affected chunks only).
- Destructive changes.

### Tools & Brushes

- **Hydraulic Erosion:** A brush that simulates water flow to carve realistic river valleys.
- **Physical Lake Generator**: Tool to flood-fill terrain depressions to a specific water level.
- **River Profiles:** Logic to automatically widen rivers as they approach the ocean based on flow data.
- **Smart Masking (Protection):** Lock specific values (e.g., "Lock Sea Level", "Protect Mountains") to prevent
  accidental overwrites.
- **Walkability Baker:** Automatic generation of Impassable/Wasteland definitions based on slope angles/height.

### Moving / Rotating of Objects

- Only other images can be moved as layers.

### Loading

- Customized loading steps to load data from raw texture files.

### Saving

- Customized saving steps to save data to texture files.

### Editor Design

- One big interactive map.
- **Left Dock:** Brush Settings (Size, Hardness, Flow, Jitter) & Toolbelt.
- **Right Dock:** Mini-Map & Layer Visibility.
- **Visuals:**
    - **Slope Heatmap:** Toggle to color-code terrain by steepness.

#### Filter Options

- View Modes: Height, Terrain, Province, River, Normal Map.

#### Selection Sub-System:

- Implementation of standard raster selection tools: Lasso, Rectangle, Magic Wand (Flood Fill).
- Selection logic: Add (Shift), Subtract (Ctrl), Intersect.
- Selections act as a hard mask for all Brushes.

#### Demographic Painting:

- Ability to switch the "Canvas" from Visuals to Data Layers.
- Paint Culture, Religion, and Trade Goods using color-coded brushes.

---

## 3. Nudge Mode

The user modifies 3D objects (Cities, Armies, Locators) placed on the map.

### History

- Full undo / redo support.
- Non-destructive changes (Metadata/Text editing).

### Moving / Rotating of Objects

- **Selection:** Raycast-based selection (ID Buffer) for pixel-perfect picking.
- **Placement:** **Ctrl + Right Click** moves the current object to the cursor position (prevents accidental moves).
- **Gizmos:** 3D handles for Move, Rotate, Scale.
- **Batch Operations:** Multi-select objects to Align, Distribute, or Rotate as a group.
- **Magnet Tool:**
    - Snap to Ground.
    - **Surface Snapping:** Snap to nearest Coastal Water vertex (for Ports).
- Quick options to set orientation to face a specific direction / normal.

### Loading & Saving

- Uses Jomini/Paradox default file structures.

### Editor Design

- One big interactive map.
- **Visuals:**
    - **Dynamic LOD:** Instanced rendering to show thousands of objects efficiently; hides minor objects when zoomed
      out.
    - **SDF Labels:** Crisp text labels for all objects.
- **Left Dock:** Filter Tree (Show/Hide Object Types).
- **Right Dock:** Object Properties & Coordinates.

#### Automatic Estimation & Validation

Automatically check if a location:

- Is missing data.
- Has data outside of its bounds.
- Has data too close to its bounds.
- Has data overlaying a river or conflicting objects.
- **Pole of Inaccessibility:** Algorithm to automatically find the best visual center for city labels.

#### Filter Options

- Types: City / Combat / Unit / Volcano / VFX / Port.
- LocationCollections (e.g., only show areas X, Y, Z).
- Status: Invalid / Warning / Valid / Locked.