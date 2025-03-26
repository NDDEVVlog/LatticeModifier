# Lattice System for Unity

This Unity script provides a **lattice-based control grid** that can be used for deformable objects, procedural mesh modifications, or structured transformations. The system allows users to **edit, save, and restore** a 3D control grid using an interactive Unity Editor tool.

---

## Features
✅ Customizable **3D control grid** with adjustable resolution  
✅ Supports **editing** the grid in Unity's Scene View  
✅ Provides **world-space and local-space** control grid data  
✅ Saves and restores control grid states  
✅ **Visualization** of control points and default grid  
✅ Supports **Gizmo-based selection and transformations**  

---

## How to Use

### 1. Adding Lattice to a GameObject
1. Attach the `Lattice` component to a GameObject in the Unity scene.  
2. The grid will be generated around the object's position.  

### 2. Editing the Control Grid
1. Select the GameObject with the `Lattice` component.  
2. In the **Inspector**, click **"Enter Edit Mode"** to start modifying the grid.  
3. Use the **Scene View** to:
   - **Click vertices** to select them.  
   - **Hold Shift** to select multiple vertices.  
   - **Move, Rotate, or Scale** selected vertices using Unity's Handles (`W, E, R` keys).  
4. Click **"Exit Edit Mode"** when done.  

### 3. Adjusting Grid Resolution
1. Change the **Resolution (X, Y, Z)** values in the Inspector.  
2. Click **"Apply Resolution"** to regenerate the control grid.  

### 4. Saving & Restoring Control Grid
- The grid **automatically saves** when modified.  
- Click **"Restore Control Grid"** to reset to the last saved state.  

### 5. Visualizing the Grid
- **Green Gizmos** show control points.  
- **Blue lines** connect control points.  
- **Yellow spheres** (if enabled) show default vertices.  

---

## Code Overview

### Main Classes

📌 **`Lattice` (MonoBehaviour)**  
- Manages the **control grid**, resolution, and editing state.  
- Provides methods to **save, restore, and modify** grid data.  
- Uses `OnDrawGizmos()` to visualize grid structure.  

📌 **`LatticeEditor` (Custom Editor)**  
- Provides an **Inspector UI** for modifying the control grid.  
- Enables **Scene View interaction** (vertex selection & transformation).  
- Supports **keyboard shortcuts** for quick mode switching.  

---

## Shortcuts

| Key            | Action                     |
|---------------|----------------------------|
| **W**        | Move selected vertices      |
| **E**        | Rotate selected vertices    |
| **R**        | Scale selected vertices     |
| **Shift + Click** | Multi-select vertices |

---

## Future Improvements
🚀 Support for **mesh deformation**  
🚀 Exporting control grid as **external data**  
🚀 Dynamic **interpolation between control points**  

---

## Author & License
📌 Developed for **Unity 2021+**  
📌 Feel free to modify and use in your projects! 🚀
