using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class GameLevel : MonoBehaviour
{
  public int width;
  public int height;

  public Unit selectedUnit;
  GridCoords startingCoords;

  AbilityUsage abilities;

  private TMP_Text levelText = null;

  void Awake() {
    levelText = GetComponentInChildren<TMP_Text>();
  }

  // -- Game Actions.

  // Expects a valid tile.
  public void ClickTile(GridCoords coords) {

    // try to select a unit
    GridEntity entity = GridBoard.instance.GetEntity(coords);

    if (entity != null && entity == selectedUnit) {
      return; // already selected
    }

    bool selectedMustAct = (selectedUnit != null) && selectedUnit.hasMoved && !selectedUnit.hasActed;

    if (entity != null && entity.isUnit) {
      if (selectedMustAct) {
        // You can't select another character if you've already moved the current one.
        InputHandler.instance.FlashButtons();
        return;
      }

      Unit unit = (Unit) entity;
      if (!unit.hasActed) {
        SwitchSelection(unit);
      }
      return;
    }

    if (selectedUnit != null) {
      // try to move
      List<GridCoords> path = GridBoard.instance.FindPath(selectedUnit.coords, coords, 
                                                          selectedUnit.remainingMovement);
      if (path != null) {
        MoveSelected(coords);
      } else {
        // TODO: should you be able to deselect characters at all?
        if (selectedMustAct) {
          // You can't deselect the character if you've already moved it, but haven't used an ability.
          InputHandler.instance.FlashButtons();
          return;
        }
        SwitchSelection(null);
      }
    }
  }

  void SwitchSelection(Unit newSelection) {
    if (selectedUnit == newSelection) 
      return; // same unit
    if (selectedUnit != null) {
      selectedUnit.SetSelected(false);
    }
    if (newSelection != null) {
      newSelection.SetSelected(true);
      startingCoords = newSelection.coords;
    }
    selectedUnit = newSelection;
    HighlightMovable();
  }

  public void UndoLastMovement() {
    if (selectedUnit != null && selectedUnit.hasMoved) {
      GridBoard.instance.Move(selectedUnit.coords, startingCoords, Mathf.Infinity);
      selectedUnit.remainingMovement = selectedUnit.totalMovement;
      selectedUnit.hasMoved = false;
      HighlightMovable();
    }
  }

  public void HighlightMovable() {
    if (selectedUnit == null) {
      GridBoard.instance.UnhighlightAll();
    } else {
      GridBoard.instance.HighlightMovable(selectedUnit.coords, selectedUnit.remainingMovement);
    }
  }

  public void MoveSelected(GridCoords coords) {
    selectedUnit.remainingMovement -= selectedUnit.coords.DistanceTo(coords);
    selectedUnit.hasMoved = true;
    GridBoard.instance.Move(selectedUnit.coords, coords);
    HighlightMovable();
  }

  public void DoAbility(ABILITY ability) {
    if (selectedUnit != null && abilities.IsAvailable(ability)) {
      bool used = selectedUnit.DoAbility(ability);
      if (used) {
        SwitchSelection(null); // deselect current unit
        abilities.Use(ability); // track that we can't use this ability again
      }
    }
  }

  // -- Level setup.
  // Returns if parsing succeeded.
  bool ParseHeader(byte[] template, int length) {
    if (template.Length < 3) {
      Debug.LogError("Header length is less than 3 bytes!");
      return false;
    }

    if (template[0] != (char)'B') return false;
    if (template[1] != (char)'M') return false;
    int version;
    Int32.TryParse("" + (char) template[2], out version);
    if (version < 1 || 2 < version) {
      Debug.LogError("We only support level file versions 1 and 2 but failed to parse version or got version " + version);
      return false;
    }

    int minLen = 7;
    int maxLen = (version == 1) ? 7 : 11;
    if (length < minLen || length > maxLen) {
      Debug.LogError("Header of size " + length + " is the wrong length.");
      return false;
    }

    string widthStr = ("" + (char)template[3]) + (char)template[4];
    Int32.TryParse(widthStr, out width);
    if (width == 0) return false;
    string heightStr = ("" + (char)template[5]) + (char)template[6];
    Int32.TryParse(widthStr, out height);
    if (height == 0) return false;

    if (version == 2) {
      // Parse available abilities
      List<ABILITY> abs = new List<ABILITY>();
      for (int i = 7; i < length; i++) {
        ABILITY a;
        switch ((char)template[i]) {
          case 'M':
            a = ABILITY.MAGNETIZE;
            break;
          case 'O':
            a = ABILITY.ROTATE;
            break;
          case 'D':
            a = ABILITY.SPAWN;
            abs.Add(ABILITY.VSPAWN); // TODO: temp
            break;
          case 'E':
            a = ABILITY.ELECTROCUTE;
            break;
          default:
            Debug.LogError("Invalid ability code specified: " + (char)template[i]);
            return false;
        }

        if (abs.Contains(a)) {
          Debug.LogError("Found a duplicate ability code: " + (char)template[i]);
          return false;
        }

        abs.Add(a);
      }

      if (!abs.Contains(ABILITY.ELECTROCUTE)) {
        Debug.LogError("E ability code not specified :'(");
        return false;
      }

      abilities = new AbilityUsage(abs);
    } else {
      abilities = new AbilityUsage();
    }

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

    // Read the header.
    int headerLength = 0;
    {
      while (headerLength < template.Length && template[headerLength] != '\n' && template[headerLength] != '\r') {
        headerLength++;
      }

    }

    int off = headerLength + 1;
    if (template[headerLength] == '\r') {
      off++; // Windows :)
    }

    bool valid = ParseHeader(template, headerLength);

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
        case 'T':
          GridBoard.instance.InitTile(gc, TILE.TREE);
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
