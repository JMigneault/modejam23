using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class GameLevel : MonoBehaviour
{
  public int width;
  public int height;

  public Unit lastMoved;
  public Unit selectedUnit;

  // TODO: used electrocute??
  public bool usedRotate = false;
  public bool usedMagnetize = false;
  public bool usedSpawn = false;

  private TMP_Text levelText = null;

  void Awake() {
    levelText = GetComponentInChildren<TMP_Text>();
  }

  // -- Game Actions.

  // Expects a valid tile.
  public void ClickTile(GridCoords coords) {
    if (selectedUnit != null && selectedUnit.coords.Equals(coords)) {
      // deselect
      SwitchSelection(null);
      return;
    }

    // try to select a unit
    GridEntity entity = GridBoard.instance.GetEntity(coords);
    if (entity != null && entity.isUnit) {
      Unit unit = (Unit) entity;
      if (!unit.hasActed) {
        SwitchSelection(unit);
      }
      // TODO: right now you can't select units with no action remaining
      return;
    }

    if (selectedUnit != null) {
      // try to move
      List<GridCoords> path = GridBoard.instance.FindPath(selectedUnit.coords, coords, 
                                                          selectedUnit.remainingMovement);
      if (path != null) {
        selectedUnit.remainingMovement -= selectedUnit.coords.DistanceTo(coords);
        GridBoard.instance.Move(selectedUnit.coords, coords);
        if (lastMoved && lastMoved != selectedUnit) {
          lastMoved.DoAbility(ABILITY.NONE);
        }
        lastMoved = selectedUnit;
      } else {
        SwitchSelection(null);
      }
    }
  }

  void SwitchSelection(Unit newSelection) {
    if (selectedUnit != null) {
      selectedUnit.SetSelected(false);
    }
    if (newSelection != null) {
      newSelection.SetSelected(true);
    }
    selectedUnit = newSelection;
  }

  public void DoAbility(ABILITY ability) {
    if (selectedUnit != null) {
      switch (ability) {
        case ABILITY.ROTATE:
          if (!usedRotate) {
            usedRotate = selectedUnit.DoAbility(ability);
            if (usedRotate && lastMoved != null && lastMoved != selectedUnit) {
              lastMoved.DoAbility(ABILITY.NONE); // TODO: let's replace this stuff at some point (bitflag?)
            }
          }
          break;
        case ABILITY.MAGNETIZE:
          if (!usedMagnetize) {
            usedMagnetize = selectedUnit.DoAbility(ability);
            if (usedMagnetize && lastMoved != null && lastMoved != selectedUnit) {
              lastMoved.DoAbility(ABILITY.NONE);
            }
          }
          break;
        case ABILITY.HSPAWN:
        case ABILITY.VSPAWN:
          if (!usedSpawn) {
            usedSpawn = selectedUnit.DoAbility(ability);
            if (usedSpawn && lastMoved != null && lastMoved != selectedUnit) {
              lastMoved.DoAbility(ABILITY.NONE);
            }
          }
          break;
        case ABILITY.ELECTROCUTE:
          selectedUnit.DoAbility(ability);
          break;
        default:
          Debug.LogError("Did not recognize requested ability");
          break;
      }
    }
  }

  // -- Level setup.
  // Returns if parsing succeeded.
  bool ParseHeader(byte[] template) {
    if (template[0] != (char)'B') return false;
    if (template[1] != (char)'M') return false;
    int version;
    Int32.TryParse("" + (char) template[2], out version);
    if (version != 1) {
      Debug.LogError("We only support level file version 1 but failed to parse version or got version " + version);
      return false;
    }
    string widthStr = ("" + (char)template[3]) + (char)template[4];
    Int32.TryParse(widthStr, out width);
    if (width == 0) return false;
    string heightStr = ("" + (char)template[5]) + (char)template[6];
    Int32.TryParse(widthStr, out height);
    if (height == 0) return false;
    return true;
  }

  public void InitFrom(byte[] template, string fileName) {

    levelText.text = fileName;

    // ----- Parse the template.

    // Read the header.
    if (template.Length == 0) {
      Debug.LogError("Failed to parse level file " + fileName + " because the file was empty.");
      return;
    }

    // Check Header. An example header is: BM10808
    // The format is:
    // - first two bytes are 'BM'
    // - third byte is the version number (always 1 for now)
    // - fourth and fifth bytes are the width of the level grid
    // - sixth and seventh bytes are the height of the level grid 
    int headerLength = 7;

    if (template.Length < headerLength) {
      Debug.LogError("Failed to parse level file " + fileName + " because the file was too short for the header.");
      return;
    }

    int off = headerLength + 1;
    if (template[headerLength] == '\r') {
      off++; // Windows :)
    }

    if (template[off-1] != '\n') {
      Debug.Log(template[off-1]);
      Debug.LogError("Failed to parse level file " + fileName + " because the header was the incorrect size.");
      return;
    }

    bool valid = ParseHeader(template);

    if (!valid) {
      Debug.LogError("Failed to parse level file " + fileName + " because the header was invalid.");
      return;
    }

    if (template.Length < (headerLength + ((width + 1) * height))) {
      Debug.LogError("Failed to parse level file " + fileName + " because the file was too short.");
      return;
    }

    GridBoard.instance.InitBoard(width, height);

    // Read the contents of the grid-board.
    int row = 0;
    int col = 0;
    int bytesRemaining = template.Length - off;
    for (int i = 0; i < bytesRemaining; i++) {
      GridCoords gc = new GridCoords(col, row);
      switch ((char)template[off++]) {
        case ' ': // ignore
          continue;
        case '\r': // ignore
          continue;
        case '\n': // next row
          if (col != width) {
            Debug.LogError("Failed to parse level file " + fileName + "because row " + row 
                           + " contained " + col + " entities instead of the expected " + width);
            return;
          }
          col = 0;
          row++;
          continue;
        case '.':
          GridBoard.instance.InitTile(gc, TILE.EMPTY);
          break;
        case 'X':
          GridBoard.instance.InitTile(gc, TILE.ENEMY);
          break;
        case 'C':
          GridBoard.instance.InitTile(gc, TILE.UNIT);
          break;
        default:
          Debug.LogError("Failed to parse level file " + fileName 
                          + " because there is an unexpected byte '" + template[off-1] + "' at offset " + (off-1)
                          + " (row " + row + ", col " + col + ")");
          return;
      }
      if (row >= height) {
        Debug.LogError("Failed to parse level file " + fileName 
                        + " because there are trailing, non-whitespace bytes after the expected " + height 
                        + " rows.");
        return;
      }
      col++;
    }

    if (row < height && !((row == height - 1) && col == width)) {
      Debug.LogError("Failed to parse level file " + fileName 
                      + " because there were only " + row + " complete rows instead of the expected " + height); 
      return;
    }
  }
}
