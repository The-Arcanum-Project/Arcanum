# Arcanum Map Editor - Feature Requests & Ideas

## 1. General Workflow & UI

* **Reference Image Support:**
    * Ability to import multiple reference images.
    * Toggle visibility and transparency for individual images.
    * **Blending Modes:** Support for GIMP-like layer modes (Difference, Multiply, Grain Extract) to exaggerate height
      data.
    * **Native Heightmap Reference:** Option to use the game’s actual heightmap as a reference layer (handling
      resolution differences automatically).
* **Canvas Expansion:** A tool to expand the map dimensions (add pixels to top/bottom/sides) that automatically adjusts
  and tiles all related files (heightmap, rivers, locations, etc.).
* **Overlays:** Option to display Location/Province names directly on the map to avoid needing to hover for
  identification.
* **UX/Controls:** Smart mouse controls (e.g., Right-click to pick height/value, Left-click to paint) to reduce UI
  clicking.

## 2. Selection & Painting Tools

* **GIMP-Style Selection Tools:**
    * Select by Color (contiguous/bucket fill logic).
    * Select by Color (Global/entire image).
    * Tolerance settings for color selection.
    * Rectangle Select.
    * Lasso Select.
* **Selection Logic:**
    * Invert Selection.
    * Add to Selection (Shift-click behavior).
    * Remove from Selection (Ctrl-click behavior).
* **Pixel Editing:** Simple brush to change pixel colors (painting locations/provinces).

## 3. Heightmap & Terrain Editing

* **Visualization:**
    * **Contextual View:** Display edges of neighboring tiles/decals to ensure mountains/terrain line up correctly
      across chunks.
    * **Color-Coded Heights:** Option to view heightmaps with a color gradient (heat map) instead of simple greyscale
      for better readability.
* **Adjustments:**
    * **Height Curves:** A visual graph/curve editor to adjust height values (non-linear contrast/brightness) with a
      toggle to view height in meters vs percentages.
* **Brushes:**
    * **Specific Terrain Brushes:** River channels, Canyons, Plateaus, Finnish Lakes.
    * **Orogeny Styles:** Brushes based on the 4 different mountain generation styles.
    * **Constraints:** Configurable height bases and caps (to prevent overshooting logical heights).
* **Procedural/Spline Generation (Non-Destructive):**
    * **Spline Tools:** Draw mountains or river valleys using lines/splines.
    * **Parametric Settings:** Adjust roughness, size, shape, and "Age" (erosion level) of mountain ranges.
    * **Modifiers:** A workflow similar to Blender modifiers—keep the data editable/vector-based and "bake" it to raster
      images later.
    * **Anchor Points:** Ability to branch sub-ranges off main splines.
* **Lakes:** Automatic generation of elevated lakes based on surrounding height data (auto-calculating water level and
  size).

## 4. Location & Province Generation

* **Image-to-Location:** Tool to generate location definitions from a provided image map.
* **Vanilla Protection:** Logic to detect and ignore vanilla hex codes so existing locations are not overwritten during
  generation.
* **Closest Neighbor/Voronoi:** Algorithmic generation of water boundaries based on distance to the nearest land
  location (handling the cutoffs between sea zones).
* **Data Painting:** Configurable percentage-based brushes for painting demographics (Culture, Religion, etc.).

## 5. Object Placement & Gizmos ("Nudge" Tools)

* **3D Placement:** precise positioning of 3D map objects (Ports, Cities, Army Positions).
* **Pixel-Perfect Clicking:** Click a specific pixel on the location map to assign the port/city to that exact
  coordinate.
* **Label Optimization:**
    * **Pole of Inaccessibility:** Implement the "Pole of Inaccessibility" algorithm (rather than simple Centroids) for
      optimal placement of City Labels and Army positions inside irregular shapes.
    * **Text Formatting:** Controls for name orientation and TrueType size (similar to EU4 Nudge).

## 6. Sea & Water Features

* **Sea Lanes:**
    * Spline-based generation for sea lanes.
    * Automatic patterning (e.g., standard 200px wide lanes with 100px segments).
    * Scale settings to adapt lane size for smaller/larger maps.
* **Coastal Waters:** Automatic generation of coastal water zones based on land proximity.
* **Ports:** A generator for port locations (referenced external tool by Nlapin).