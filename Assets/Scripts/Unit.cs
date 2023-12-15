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

  public GameObject deployOutlinePrefab = null;
  public GameObject rotateOutlinePrefab = null;
  public GameObject magnetizeOutlinePrefab = null;
  List<GameObject> magnetizeOutlines = null;

  public Sprite defaultSprite = null;
  public Sprite pickedUpSprite = null;

  void Start() {
    board = GridBoard.instance;
  }

  public void DoAbility(ABILITY ability) {
    if (hasActed) return;

    GameManager.instance.currentLvl.animating = true;

    switch (ability) {
      case ABILITY.ROTATE:
        StartCoroutine(DoRotate());
        break;
      case ABILITY.MAGNETIZE:
        StartCoroutine(DoMagnetize());
        break;
      case ABILITY.SPAWN:
        StartCoroutine(DoSpawn());
        break;
      case ABILITY.ELECTROCUTE:
        StartCoroutine(DoElectrocute());
        return; // we're probably deleted here, let's get out asap.
    }

    hasActed = true;
    hasMoved = true;
    remainingMovement = 0;
    GetComponent<SpriteRenderer>().color = new Color(.4f, .4f, .4f); // TODO: temp!
  }

  IEnumerator DoRotate() {
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
        board.Move(sequence[0], sequence[sequence.Length - 1], Globals.DEFAULT_MOVE_SPEED);
      } else {
        board.Move(sequence[cursor], sequence[cursor - 1], Globals.DEFAULT_MOVE_SPEED);
      }
      cursor++;
      if (cursor == sequence.Length) {
        cursor = 0;
      }
    }

    if (allNeighborsMovable) {
      // Restore the entity we set aside.
      board.SetEntity(coords.Go(DIR.DIAGUR), setAside, Globals.DEFAULT_MOVE_SPEED);
    }

    // Highlight effected area.
    for (int i = 0; i < sequence.Length; i++) {
      if (board.IsCoordValid(sequence[i])) {
        board.GetTile(sequence[i]).Highlight();
      }
    }

    GameObject outline = GameObject.Instantiate(rotateOutlinePrefab, transform);
    outline.transform.position = transform.position;

    float t = 0;
    while (t < 0.7f) {
      outline.transform.Rotate(0, 0, -64.0f * Time.deltaTime);
      t += Time.deltaTime;
      yield return null;
    }

    GameObject.Destroy(outline);

    board.UnhighlightAll();
    GameManager.instance.currentLvl.animating = false;
    yield return null;
  }

  IEnumerator DoMagnetize() {
    // Walk to the end of each row and column. Move entities in the opposite direction by one tile.
    magnetizeOutlines = new List<GameObject>();
    board.GetTile(coords).Highlight();
    MagnetizeLoop(coords.Right(), DIR.RIGHT, DIR.LEFT, 0);
    MagnetizeLoop(coords.Up(), DIR.UP, DIR.DOWN, 90);
    MagnetizeLoop(coords.Left(), DIR.LEFT, DIR.RIGHT, 180);
    MagnetizeLoop(coords.Down(), DIR.DOWN, DIR.UP, 270);

    yield return new WaitForSeconds(0.7f);

    board.UnhighlightAll();
    foreach (GameObject go in magnetizeOutlines) {
      GameObject.Destroy(go);
    }
    GameManager.instance.currentLvl.animating = false;
    yield return null;
  }

  void MagnetizeLoop(GridCoords start, DIR primary, DIR opposite, float rotationDegrees) {
    GridCoords c = start;
    while (board.IsCoordValid(c)) {
      board.Move(c, c.Go(opposite), Globals.DEFAULT_MOVE_SPEED); // try to pull one space
      board.GetTile(c).Highlight();
      if (!board.IsCoordTree(c)) {
        GameObject outline = GameObject.Instantiate(magnetizeOutlinePrefab, transform);
        outline.transform.position = board.GetLocalPos(c) + board.transform.position;
        outline.transform.Rotate(0, 0, rotationDegrees);
        magnetizeOutlines.Add(outline);
      }
      c = c.Go(primary);
    }
  }

  // returns whether was spawn was performed successfully.
  IEnumerator DoSpawn() {
    if (board.IsCoordValid(coords.Left())) {
      board.GetTile(coords.Left()).Highlight();
    }

    GameObject deployOutlineL = null;
    GameObject deployOutlineR = null;

    if (!board.IsCoordOccupied(coords.Left())) {
      deployOutlineL = GameObject.Instantiate(deployOutlinePrefab, transform);
      deployOutlineL.transform.position = board.GetLocalPos(coords.Left()) + board.transform.position;
    }

    if (board.IsCoordValid(coords.Right())) {
      board.GetTile(coords.Right()).Highlight();
    }

    if (!board.IsCoordOccupied(coords.Right())) {
      deployOutlineR = GameObject.Instantiate(deployOutlinePrefab, transform);
      deployOutlineR.transform.position = board.GetLocalPos(coords.Right()) + board.transform.position;
    }

    yield return new WaitForSeconds(0.7f);

    if (!board.IsCoordOccupied(coords.Left())) {
      board.InitTile(coords.Left(), TILE.ENEMY);
      GameObject.Destroy(deployOutlineL);
    }

    if (!board.IsCoordOccupied(coords.Right())) {
      board.InitTile(coords.Right(), TILE.ENEMY);
      GameObject.Destroy(deployOutlineR);
    }

    board.UnhighlightAll();
    GameManager.instance.currentLvl.animating = false;
    yield return null;
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
    TutorialCursor.instance.SetHintsPaused(true);

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

    // Check if all are electrocuted.
    bool done = false;
    for (int i = 0; i < board.Width(); i++) {
      for (int j = 0; j < board.Height(); j++) {
        GridEntity e = board.GetEntity(new GridCoords(i, j));
        if (e != null && e.isConductive && !e.isElectrocuted) {
          GameManager.instance.currentLvl.failed = true;
          done = true;
          break;
        }
      }
      if (done) break;
    }

    GameManager.instance.currentLvl.animating = false;
    GameManager.instance.currentLvl.readyToDie = true;
    TutorialCursor.instance.SetHintsPaused(false);
    yield return null;
  }

  IEnumerator DoElectrocute() {
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
    return DoLightingSequence(steps);
  }

  void ElectrocuteNeighbor(GridCoords neighborCoords, int neighborDist, Queue<Step> queue) {
    GridEntity neighbor = board.GetEntity(neighborCoords);
    if (neighbor != null && neighbor.isConductive && !neighbor.isElectrocuted) {
      neighbor.isElectrocuted = true;
      queue.Enqueue(new Step(neighborCoords, neighborDist));
    }
  }

  public void Pickup() {
    if (pickedUpSprite != null) {
      GetComponent<SpriteRenderer>().sprite = pickedUpSprite;
    }
  }

  public void Drop() {
    if (pickedUpSprite != null) {
      GetComponent<SpriteRenderer>().sprite = defaultSprite;
    }
  }

}
