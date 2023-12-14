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

  void Start() {
    board = GridBoard.instance;
  }

  public void DoAbility(ABILITY ability) {
    if (hasActed) return;

    switch (ability) {
      case ABILITY.ROTATE:
        DoRotate();
        break;
      case ABILITY.MAGNETIZE:
        DoMagnetize();
        break;
      case ABILITY.SPAWN:
        DoSpawn();
        break;
      case ABILITY.ELECTROCUTE:
        DoElectrocute();
        return; // we're probably deleted here, let's get out asap.
    }

    hasActed = true;
    hasMoved = true;
    remainingMovement = 0;
    GetComponent<SpriteRenderer>().color = new Color(.4f, .4f, .4f); // TODO: temp!
  }

  void DoRotate() {
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
    int startingPoint = 1; // 1 is the default for if we're not next to a wall

    bool againstBorder = board.IsBoundaryCoord(coords);

    // Go through the sequence of neighbors to figure out if we border a tree.
    bool againstTree = false;
    for (int i = 0; i < sequence.Length; i++) {
      if (board.IsCoordTree(sequence[i])) {
        againstTree = true;
        startingPoint = i; // we should start the sequence by trying to rotate this tree
      }
    }

    GridEntity setAside = null;
    if (againstBorder) {
      // Starting point must be somewhere in the wall.
      if (coords.i == 0) {
        startingPoint = 2; // start left
      } else if (coords.i == board.Width() - 1) {
        startingPoint = 6; // start right
      } else if (coords.j == 0) {
        startingPoint = 0; // start up
      } else if (coords.j == board.Width() - 1) {
        startingPoint = 4; // start down
      }
    } 

    bool allNeighborsMovable = !againstBorder && !againstTree;
    if (allNeighborsMovable) {
      // Set aside the entity right above us.
      setAside = board.GetEntity(coords.Up());
      board.SetEntity(coords.Up(), null, 0);
    }

    // Do the actual switching, being careful to wrap around the sequence.
    int cursor = startingPoint;
    int nMoves = allNeighborsMovable ? sequence.Length - 1 : sequence.Length;
    for (int i = 0; i < nMoves; i++) {
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

    if (allNeighborsMovable) {
      // Restore the entity we set aside.
      board.SetEntity(coords.Go(DIR.DIAGUR), setAside, Mathf.Infinity);
    }
  }

  void DoMagnetize() {
    // Walk to the end of each row and column. Move entities in the opposite direction by one tile.
    MagnetizeLoop(coords.Up(), DIR.UP, DIR.DOWN);
    MagnetizeLoop(coords.Down(), DIR.DOWN, DIR.UP);
    MagnetizeLoop(coords.Left(), DIR.LEFT, DIR.RIGHT);
    MagnetizeLoop(coords.Right(), DIR.RIGHT, DIR.LEFT);
  }

  void MagnetizeLoop(GridCoords start, DIR primary, DIR opposite) {
    GridCoords c = start;
    while (board.IsCoordValid(c)) {
      board.Move(c, c.Go(opposite)); // try to pull one space
      c = c.Go(primary);
    }
  }

  // returns whether was spawn was performed successfully.
  void DoSpawn() {
    if (!board.IsCoordOccupied(coords.Left())) {
      board.InitTile(coords.Left(), TILE.ENEMY);
    }

    if (!board.IsCoordOccupied(coords.Right())) {
      board.InitTile(coords.Right(), TILE.ENEMY);
    }
  }

  // Encodes position and distance of a step in the traversal.
  public struct Step {
    public Step(GridCoords c, int d) {
      this.c = c;
      this.d = d;
    }

    public GridCoords c;
    public int d;
  }

  IEnumerator DoLightingSequence(List<Step> steps) {
    int currDist = 0;
    for (int i = 0; i < steps.Count; i++) {
      Step s = steps[i];
      if (s.d != currDist) {
        currDist = s.d;
        yield return new WaitForSeconds(0.5f);
      }

      GridEntity e = board.GetEntity(s.c);
      if (e.isEnemy) { // TODO: electrocute units :)
        (e).GetComponent<SpriteRenderer>().sprite = e.litBulb;
      }
    }
    yield return new WaitForSeconds(1.5f);
    GameManager.instance.currentLvl.readyToDie = true;
    yield return null;
  }

  void DoElectrocute() {
    GameManager.instance.currentLvl.levelIsDone = true;

    // Do BFS of connected entities. Win if every essential entity is hit.
    Queue<Step> queue = new Queue<Step>();
    List<Step> steps = new List<Step>(); // remember everything we examined
    isElectrocuted = true;
    queue.Enqueue(new Step(coords, 0));

    while (queue.Count > 0) {
      // Check each orthogonal direction.
      Step s = queue.Dequeue();
      steps.Add(s);
      int neighborDist = s.d + 1;
      ElectrocuteNeighbor(s.c.Up(), neighborDist, queue);
      ElectrocuteNeighbor(s.c.Right(), neighborDist,  queue);
      ElectrocuteNeighbor(s.c.Down(), neighborDist, queue);
      ElectrocuteNeighbor(s.c.Left(), neighborDist, queue);
    }

    // Start lighting animation.
    StartCoroutine(DoLightingSequence(steps));

    // Check if all are electrocuted.
    for (int i = 0; i < board.Width(); i++) {
      for (int j = 0; j < board.Height(); j++) {
        GridEntity e = board.GetEntity(new GridCoords(i, j));
        if (e != null && e.isConductive && !e.isElectrocuted) {
          GameManager.instance.currentLvl.failed = true;
          return;
        }
      }
    }

    GameManager.instance.currentLvl.failed = false;
  }

  void ElectrocuteNeighbor(GridCoords neighborCoords, int neighborDist, Queue<Step> queue) {
    GridEntity neighbor = board.GetEntity(neighborCoords);
    if (neighbor != null && neighbor.isConductive && !neighbor.isElectrocuted) {
      neighbor.isElectrocuted = true;
      queue.Enqueue(new Step(neighborCoords, neighborDist));
    }
  }

}
