using UnityEngine;
using System.Collections.Generic;
using Settworks.Hexagons;

[AddComponentMenu("")]
public class HexKitExampleSceneControl : MonoBehaviour {

	// In the scene, these are set to the HexSolid and HexOpen prefabs.
	public GameObject solid;
	public GameObject open;

	// Cursor position, cell highlighting and click behavior.
	HexCoord cursorHex;
	Transform cursor;
	bool clear;

	// Start point of pathfinding, or field-of-view center.
	HexCoord origin;
	// Destination point of pathfinding.
	HexCoord target;
	// The list of cells which block a path, or block line of sight.
	HashSet<HexCoord> blockers;

	// HexCoord coordinates just outside the lower left and upper right of the screen.
	HexCoord[] bounds;

	// Indexed containers for the objects that make up the display grid.
	Dictionary<HexCoord, Renderer> borders;
	Dictionary<HexCoord, Renderer> cells;

	// Current mode: if true, field-of-view; if false, pathfinding (default).
	bool FOV;


	void Start() {
		// Lower left screen corner in world space.
		Vector2 screenCorner = GetComponent<Camera>().ScreenToWorldPoint(Vector2.zero);
		// Find the HexCoord corners of a rectangle that contains all visible cells, including partial cells.
		// Origin is at center in this scene, so just negate for opposite corner.
		bounds = HexCoord.CartesianRectangleBounds(screenCorner, -screenCorner);

		// Construct the grid out of borders and backgrounds.
		// These objects never move, so all we need to store are their Renderer components for color changes.
		borders = new Dictionary<HexCoord, Renderer>();		// Grid outline.
		cells = new Dictionary<HexCoord, Renderer>();		// Cell backgrounds.
		foreach (HexCoord hex in HexKit.WithinRect(bounds[0], bounds[1])) {
			// This could be done without the temporary GameObject, but it helps clarify what's happening.
			GameObject gridObj = GameObject.Instantiate(open,
			                                            hex.Position(),
			                                            Quaternion.identity
			                                            ) as GameObject;
			borders.Add(hex, gridObj.GetComponent<Renderer>());
			gridObj = GameObject.Instantiate(solid,
			                                 (Vector3)hex.Position() + Vector3.forward,		// Farther from camera.
			                                 Quaternion.identity
			                                 ) as GameObject;
			cells.Add(hex, gridObj.GetComponent<Renderer>());
		}

		// The cursor is a smaller outline, so it doesn't obscure anything.
		// This will move around, but won't change otherwise, so we only want the Transform.
		// Again, the temporary GameObject is for clarity.
		GameObject cursorObj = GameObject.Instantiate(open,
		                                              HexCoord.origin.Position(),
		                                              Quaternion.identity
		                                              ) as GameObject;
		cursor = cursorObj.transform;
		cursor.localScale = new Vector3(0.9f, 0.9f);		// Make it fit nicely just inside the cell border.

		// There are initially no blockers.
		blockers = new HashSet<HexCoord>();

		// Everything's ready. Render the initial scene!
		UpdateGrid();
	}


	// Update performs input handling, then conditionally updates the screen with UpdateGrid()
	void Update() {
		// Handle cursor position. Store old position, we'll check it a couple of times.
		HexCoord cursorHexWas = cursorHex;
		// Mouse position in pixels -> world position -> HexCoord position.
		cursorHex = HexCoord.AtPosition(GetComponent<Camera>().ScreenToWorldPoint(Input.mousePosition));
		// The screen boundary was defined in Start(). Make sure the mouse is inside it.
		// This is done in QR space because there will be partial hexes at the real screen border.
		if (cursorHex.IsWithinRectangle(bounds[0], bounds[1]))
			cursor.position = cursorHex.Position();
		else cursorHex = cursorHexWas;		// Mouse is somehow outside of screen! Keep old position.

		bool changed = false;		// Later, we'll only update if changed==true.

		// Escape clears all blockers.
		if (Input.GetKeyDown(KeyCode.Escape)) {
			changed = true;
			blockers.Clear();
		}
		// Tab switches between FOV and pathfinding modes.
		if (Input.GetKeyDown(KeyCode.Tab)) {
			changed = true;
			FOV = !FOV;
		}

		// In FOV mode, target is not used. In path mode, move it with right mouse button.
		if (target != cursorHex && (FOV? false: Input.GetMouseButton(1))) {
			changed = true;
			target = cursorHex;
		}
		// In FOV mode, move origin with right mouse button. In path mode, move it with space.
		if (origin != cursorHex && (FOV? Input.GetMouseButton(1): Input.GetKey(KeyCode.Space))) {
			changed = true;
			origin = cursorHex;
		}
		// In either mode, place/remove blockers with left mouse button.
		if (Input.GetMouseButtonDown(0)) {
			// At time of click, check the current cell.
			// If it's a blocker, we're clearing; otherwise setting.
			clear = (blockers.Contains(cursorHex));
		}
		if (Input.GetMouseButton(0)) {
			// If the block state is the same as the clear state, it will wind up flipped.
			if (blockers.Contains(cursorHex) == clear)
				changed = true;
			// We don't care what the previous state was here, only what it should be.
			if (clear)
				blockers.Remove(cursorHex);
			else
				blockers.Add(cursorHex);
		}

		// Input is taken care of. Update the screen if necessary.
		if (changed) UpdateGrid();

		// Note: Alt-F4 handling is built in.
	}


	// These functions are used as arguments to HexKit functions in UpdateHexes()
	bool IsBlocked(HexCoord hex) {
		return blockers.Contains(hex) || !hex.IsWithinRectangle(bounds[0], bounds[1]);
	}

	bool? IsTransparent(HexCoord hex) {
		if (hex.IsWithinRectangle(bounds[0], bounds[1]))
			return !blockers.Contains(hex);
		return null;
	}

	// Heavy lifting happens here. If grid status has changed, find new path/FOV and update display.
	void UpdateGrid() {
		// "Lit" hexes are the path from origin to target, or the visible area for FOV.
		HashSet<HexCoord> lit;
		if (FOV) {
			// Radiate returns IEnumerable<HexCoord>, which HashSet can use for initialization.
			lit = new HashSet<HexCoord>(HexKit.Radiate(origin, IsTransparent));
		}
		else {
			// Path returns HexPathNodes instead of HexCoords, and does so through an out argument.
			lit = new HashSet<HexCoord>();		// Initialize to empty set.
			HexPathNode[] nodes;		// For the out argument.
			// If there is no path, this evaluates false and lit stays empty.
			if (HexKit.Path(out nodes, origin, target, IsBlocked)) {
				// Iterate the nodes, and add their HexCoord locations to lit.
				foreach (HexPathNode node in nodes) {
					lit.Add(node.Location);
				}
			}
			// If no path was generated, the lit set remains empty (but not null).
		}

		// Iterate border objects, setting colors by status.
		foreach (HexCoord hex in borders.Keys) {
			if (hex == target && !FOV)		// Ignore target in FOV mode.
				borders[hex].material.color = Color.red;
			else if (lit.Contains(hex))
				borders[hex].material.color = Color.yellow;
			else
				borders[hex].material.color = new Color(0.125f, 0.125f, 0.125f);		// Dim gray.
		}

		// Now do the same for cell backgrounds.
		foreach (HexCoord hex in cells.Keys) {
			if (hex == origin)
				cells[hex].material.color = Color.green;
			else if (blockers.Contains(hex))
				cells[hex].material.color = Color.magenta;
			else
				cells[hex].material.color = Color.black;
		}
	}


	void OnGUI() {
		GUILayout.Label("RMB: move " + (FOV? "origin" : "target") + (FOV? "" : "\t\tSpace: move origin") +
		                "\nLMB: place or clear obstacles" +
		                "\nEsc: clear all obstacles" +
		                "\nTab: switch to " + (FOV? "pathfinding" : "field of view") +
		                "\nAlt-F4: Quit");
	}
}
