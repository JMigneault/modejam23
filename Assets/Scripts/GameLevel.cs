using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class GameLevel : MonoBehaviour
{
  public int width;
  public int height;

  GridCoords startingCoords;

  public AbilityUsage abilities;

  private TMP_Text levelText = null;

  public GameObject suitPrefab;
  public Suit[] suits;
  public Vector3 boxTop;
  public float boxDist = 5.0f;
  public float boxLength = 2.0f;

  public Vector3 dragStartPos;
  public Vector3 dragOffset;
  public GameObject draggingObject = null;
  public DRAG dragging = DRAG.NONE;

  void Awake() {
    levelText = GetComponentInChildren<TMP_Text>();
  }

  public void MousePosition(Vector3 mousePos) {
    if (dragging != DRAG.NONE) {
      draggingObject.transform.position = mousePos + dragOffset;
    }
  }

  // -- Game Actions.
  public void Release() {
    if (dragging != DRAG.NONE) {
      GridCoords coords = GridBoard.instance.WorldToGrid(draggingObject.transform.position);
      if (GridBoard.instance.IsCoordValid(coords)) {
        if (dragging == DRAG.UNIT) {
          // Try to move the unit. e must be null.
          Unit unit = draggingObject.GetComponent<Unit>();
          List<GridCoords> path = GridBoard.instance.FindPath(unit.coords, coords, unit.remainingMovement);
          if (path != null && path.Count > 1) {
            GridBoard.instance.Move(unit.coords, coords, Mathf.Infinity);
            unit.hasMoved = true;
            unit.remainingMovement = 0;
            dragging = DRAG.NONE;
            GridBoard.instance.UnhighlightAll();
          } else {
            ReturnDragged();
          }
        } else if (dragging == DRAG.SUIT) {
          GridEntity e = GridBoard.instance.GetEntity(coords);
          // Try to drop the suit on a unit.
          if (e != null && e.isUnit && !((Unit)e).hasActed) {
            DoAbility(draggingObject.GetComponent<Suit>().ability, (Unit) e);
            dragging = DRAG.NONE;
            // TODO: put on suit!
            draggingObject.SetActive(false);
          } else {
            ReturnDragged();
          }
        }
      } else {
        ReturnDragged();
      }
    }
  }

  void ReturnDragged() {
    // Snap the object back to it's starting place.
    GridBoard.instance.UnhighlightAll();
    draggingObject.transform.position = dragStartPos;
    dragging = DRAG.NONE;
  }

  public void Click(Vector3 mousePos) {
    if (dragging != DRAG.NONE) {
      Debug.LogError("Received a click even though something is already being dragged!");
    }

    GridCoords coords = GridBoard.instance.WorldToGrid(mousePos);
    if (GridBoard.instance.IsCoordValid(coords)) {
      // Check if we clicked a tile.
      GridEntity e = GridBoard.instance.GetEntity(coords);
      if (e != null && e.isUnit && !((Unit)e).hasMoved) {
        dragging = DRAG.UNIT;
        SetDragging(e.gameObject, mousePos);
        GridBoard.instance.HighlightMovable(e.coords, ((Unit)e).remainingMovement);
      }
    } else {
      // Check if we clicked a jumpsuit.
      int suitIndex = abilities.ClickForSuit(mousePos);
      if (suitIndex >= 0) {
        dragging = DRAG.SUIT;
        SetDragging(suits[suitIndex].gameObject, mousePos);
      }
    }
  }

  void SetDragging(GameObject g, Vector3 mousePosition) {
    draggingObject = g;
    dragStartPos = g.transform.position;
    dragOffset = g.transform.position - mousePosition;
  }

  // NOTE: no longer used.
  /*
  public void UndoLastMovement() {
    if (selectedUnit != null && selectedUnit.hasMoved) {
      GridBoard.instance.Move(selectedUnit.coords, startingCoords, Mathf.Infinity);
      selectedUnit.remainingMovement = selectedUnit.totalMovement;
      selectedUnit.hasMoved = false;
      HighlightMovable();
    }
  }
  */

  public void DoAbility(ABILITY ability, Unit doer) {
    if (abilities.IsAvailable(ability)) {
      doer.DoAbility(ability);
      if (ability == ABILITY.ELECTROCUTE) {
        // We've already reloaded the level. Get out quick.
        return;
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

    suits = new Suit[4]; // we'll init these

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

    List<ABILITY> abs = new List<ABILITY>();
    if (version == 2) {
      // Parse available abilities
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

    } else {
      abs = new List<ABILITY>() {ABILITY.MAGNETIZE, ABILITY.ROTATE, ABILITY.SPAWN, ABILITY.ELECTROCUTE};
    }
    abilities = new AbilityUsage(abs, this);
    foreach (ABILITY a in abs) {
      Suit s = GameObject.Instantiate(suitPrefab, this.transform).GetComponent<Suit>();
      s.ability = a;
      s.transform.position = boxTop + Vector3.down * ((int) a) * boxDist;
      suits[(int) a] = s;
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

    // Reset.
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
