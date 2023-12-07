using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Unit : GridEntity
{
  public int totalMovement = 2;
  public int remainingMovement = 2;
  public GridCoords startingCoords = null;
  public bool hasMoved = false;
  public bool hasActed = false;

  public GridBoard board = null;

  bool selected = false;

  void Start() {
    board = GridBoard.instance;
  }

  public void SetSelected(bool selected) {
    this.selected = selected;
    if (!hasActed) 
      GetComponent<SpriteRenderer>().color = selected ? new Color(.7f, .7f, .7f) : Color.white; // TODO: temp!
  }

  public bool DoAbility(ABILITY ability) {
    if (hasActed) return false;

    bool success = false;
    switch (ability) {
      case ABILITY.ROTATE:
        success = DoRotate();
        break;
      case ABILITY.MAGNETIZE:
        success = DoMagnetize();
        break;
      case ABILITY.SPAWN:
        success = DoSpawn(false);
        break;
      case ABILITY.VSPAWN:
        success = DoSpawn(true);
        break;
      case ABILITY.ELECTROCUTE:
        success = DoElectrocute();
        return success; // we're probably deleted here, let's get out asap.
    }

    hasActed = success;
    if (hasActed) {
      remainingMovement = 0;
      GetComponent<SpriteRenderer>().color = new Color(.4f, .4f, .4f); // TODO: temp!
    }
    return success;
  }

  bool DoRotate() {
    // Iterate counter-clockwise through entities, moving each one space clockwise.
    // We have two strategies depending on whether we're up against a wall (which block rotation).
    // If we are against a wall, we just want to start out iteration from somewhere within the wall.
    // If we're not against a wall, we need to make a space in the circle by setting an entity aside. Then
    // once everyone else has been rotated, which can restore it to its new position. We arbitrarily choose
    // the entity directly above us.

    // The counter clock-wise sequence of coordinates.
    GridCoords[] sequence = {coords.Up(), 
                             coords.Go(DIR.DIAGUL), 
                             coords.Left(), 
                             coords.Go(DIR.DIAGDL), 
                             coords.Down(), 
                             coords.Go(DIR.DIAGDR), 
                             coords.Right(), 
                             coords.Go(DIR.DIAGUR)};

    bool againstWall = board.IsBoundaryCoord(coords);

    GridEntity setAside = null;
    int startingPoint = 1;
    if (againstWall) {
      // Starting point can be anywhere in the wall.
      if (coords.i == 0) {
        startingPoint = 2; // start left
      } else if (coords.i == board.Width() - 1) {
        startingPoint = 6; // start right
      } else if (coords.j == 0) {
        startingPoint = 0; // start up
      } else if (coords.j == board.Width() - 1) {
        startingPoint = 4; // start down
      }
    } else {
      // Set aside the entity right above us.
      setAside = board.GetEntity(coords.Up());
      board.SetEntity(coords.Up(), null);
    }

    // Do the actual switching, being careful to wrap around the sequence.
    int cursor = startingPoint;
    for (int i = 0; i < sequence.Length - 1; i++) {
      if (cursor == 0) {
        board.Move(sequence[0], sequence[sequence.Length - 1]);
      } else {
        board.Move(sequence[cursor], sequence[cursor - 1]);
      }
      cursor++;
      if (cursor == sequence.Length) {
        cursor = 0;
      }
    }

    if (!againstWall) {
      // Restore the entity we set aside.
      board.SetEntity(coords.Go(DIR.DIAGUR), setAside);
    }

    return true;
  }

  bool DoMagnetize() {
    // Walk to the end of each row and column. Move entities in the opposite direction by one tile.

    MagnetizeLoop(coords.Up(), DIR.UP, DIR.DOWN);
    MagnetizeLoop(coords.Down(), DIR.DOWN, DIR.UP);
    MagnetizeLoop(coords.Left(), DIR.LEFT, DIR.RIGHT);
    MagnetizeLoop(coords.Right(), DIR.RIGHT, DIR.LEFT);

    return true;
  }

  void MagnetizeLoop(GridCoords start, DIR primary, DIR opposite) {
    GridCoords c = start;
    while (board.IsCoordValid(c)) {
      if (!board.GetEntity(c).isTree) { // can't pull trees
        board.Move(c, c.Go(opposite)); // try to pull one space
      }
      c = c.Go(primary);
    }
  }

  // returns whether was spawn was performed successfully.
  bool DoSpawn(bool vertical) {
    GridCoords target1 = vertical ? coords.Up() : coords.Left();
    GridCoords target2 = vertical ? coords.Down() : coords.Right();

    // check validity
    if (board.IsCoordOccupied(target1) || board.IsCoordOccupied(target2)) {
      return false;
    }

    board.InitTile(target1, TILE.ENEMY); // TODO: should we make some new entity type instead?
    board.InitTile(target2, TILE.ENEMY);
    return true;
  }

  bool DoElectrocute() {
    // Do BFS of connected entities. Win if every essential entity is hit.
    Queue<GridCoords> queue = new Queue<GridCoords>();
    isElectrocuted = true;
    queue.Enqueue(coords);

    while (queue.Count > 0) {
      // Check each orthogonal direction.
      GridCoords gc = queue.Dequeue();
      ElectrocuteNeighbor(gc.Up(), queue);
      ElectrocuteNeighbor(gc.Right(), queue);
      ElectrocuteNeighbor(gc.Down(), queue);
      ElectrocuteNeighbor(gc.Left(), queue);
    }

    // Check if all are electrocuted.
    for (int i = 0; i < board.Width(); i++) {
      for (int j = 0; j < board.Height(); j++) {
        GridEntity e = board.GetEntity(new GridCoords(i, j));
        if (e != null && e.isConductive && !e.isElectrocuted) {
          GameManager.instance.FailLevel();
          return true;
        }
      }
    }

    GameManager.instance.BeatLevel();
    return true;
  }

  void ElectrocuteNeighbor(GridCoords neighborCoords, Queue<GridCoords> queue) {
    GridEntity neighbor = board.GetEntity(neighborCoords);
    if (neighbor != null && neighbor.isConductive && !neighbor.isElectrocuted) {
      neighbor.isElectrocuted = true;
      queue.Enqueue(neighborCoords);
    }
  }

}
