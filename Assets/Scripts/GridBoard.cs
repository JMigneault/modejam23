using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBoard : MonoBehaviour
{

  public GameObject unitPrefab;
  public GameObject enemyPrefab;
  public GameObject treePrefab;
  public GameObject tilePrefab;

  public float gridSize = 1.0f;

  public GridEntity[,] entities;
  public GridTile[,] tiles;

  // Singleton
  public static GridBoard instance = null;
  void Awake() {
    instance = this;
  }

  // Helper/utility functions.
  public int Width() {
    return entities.GetLength(0);
  }

  public int Height() {
    return entities.GetLength(1);
  }

  public GridEntity GetEntity(GridCoords coords) {
    return IsCoordValid(coords) ? entities[coords.i, coords.j] : null;
  }

  public GridTile GetTile(GridCoords coords) {
    return IsCoordValid(coords) ? tiles[coords.i, coords.j] : null;
  }

  public bool SetEntity(GridCoords coords, GridEntity entity, float speed) {
    if (!IsCoordValid(coords)) {
      return false;
    }

    if (entity != null) {
      entity.SetCoords(coords, speed);
    }
    entities[coords.i, coords.j] = entity;
    return true;
  }

  public bool IsBoundaryCoord(GridCoords coords) {
    return coords.i == 0 || coords.i == Width() - 1 || coords.j == 0 || coords.j == Height() - 1;
  }

  public bool IsCoordValid(GridCoords coords) {
    if (coords == null) return false;
    return 0 <= coords.i && coords.i < Width() && 0 <= coords.j && coords.j < Height();
  }

  // Returns true if there is an entity at coords or if coords is out of bounds.
  public bool IsCoordOccupied(GridCoords coords) {
    if (!IsCoordValid(coords)) return true; // out of bounds == blocked
    return GetEntity(coords) != null;
  }

  public bool IsCoordTree(GridCoords coords) {
    GridEntity e = GetEntity(coords);
    return e != null && e.isTree;
  }

  Vector3 LocalTopLeft() {
    return new Vector3(-entities.GetLength(0) * gridSize * 0.5f, 
                       entities.GetLength(1) * gridSize * 0.5f);
  }

  public Vector3 GetLocalPos(GridCoords coords) {
    return new Vector3((coords.i + 0.5f) * gridSize, -(coords.j + 0.5f) * gridSize) + LocalTopLeft();
  }

  public GridCoords WorldToGrid(Vector3 pos) {
    Vector3 posRelativeTL = pos - (transform.position + LocalTopLeft());
    Vector3 posCoordSys = new Vector3(posRelativeTL.x, -1.0f * posRelativeTL.y, 0);
    return new GridCoords((int)(posCoordSys.x / gridSize), (int)(posCoordSys.y / gridSize));
  }

  public void UnhighlightAll() {
    for (int i = 0; i < Width(); i++) {
      for (int j = 0; j < Height(); j++) {
        tiles[i, j].Unhighlight();
      }
    }
  }

  public void UndarkenAll() {
    for (int i = 0; i < Width(); i++) {
      for (int j = 0; j < Height(); j++) {
        tiles[i, j].SetDarkened(false);
      }
    }
  }

  public void HighlightMovable(GridCoords start, int distance) {
    if (distance > 2) {
      Debug.LogError("Failed to HighlightMovable because a highlight distance of greater than 2 was requested.");
      return;
    }

    UnhighlightAll();

    // Because the max movement is two, we can just brute force this. A more principled movement system
    // using BFS would be nice, but this should be good enough.

    // center
    HighlightIfAccessible(start, start, distance);
    // distance 1
    HighlightIfAccessible(start, start.Up(), distance);
    HighlightIfAccessible(start, start.Right(), distance);
    HighlightIfAccessible(start, start.Down(), distance);
    HighlightIfAccessible(start, start.Left(), distance);
    // distance 2, orthogonal
    HighlightIfAccessible(start, start.Up().Up(), distance);
    HighlightIfAccessible(start, start.Right().Right(), distance);
    HighlightIfAccessible(start, start.Down().Down(), distance);
    HighlightIfAccessible(start, start.Left().Left(), distance);
    // distance 2, diagonal
    HighlightIfAccessible(start, start.Go(DIR.DIAGUL), distance);
    HighlightIfAccessible(start, start.Go(DIR.DIAGUR), distance);
    HighlightIfAccessible(start, start.Go(DIR.DIAGDR), distance);
    HighlightIfAccessible(start, start.Go(DIR.DIAGDL), distance);
  }

  public void HighlightIfAccessible(GridCoords start, GridCoords target, int distance) {
    bool accessible = FindPath(start, target, distance) != null;
    if (accessible) {
      GetTile(target).Highlight();
    }
  }

  // Returns a list of the path (including start and end) or returns null if no path exists.
  public List<GridCoords> FindPath(GridCoords start, GridCoords end, int maxDistance) {
    if (maxDistance > 2) {
      Debug.LogError("Failed to FindPath because a path of length greater than 2 was requested.");
      return null;
    }

    if (!IsCoordValid(start)) {
      Debug.LogError("Failed to FindPath because the starting coord was not valid");
      return null;
    }

    if (!IsCoordValid(end)) {
      return null; // trying to find a path off the map
    }

    if (IsCoordOccupied(end) && !start.Equals(end)) {
      return null; // something is already there
    }

    int dist = start.DistanceTo(end);

    if (dist > maxDistance) {
      return null; // too far away
    }

    if (dist == 0) { // start == end
      return new List<GridCoords>(){end};
    }
    if (dist == 1) {
      return new List<GridCoords>(){start, end};
    }
    if (dist == 2) {
      if (start.IsOrthogonal(end)) {
        GridCoords middle = start.Go(start.DirectionTo(end));
        return IsCoordOccupied(middle) ? null : new List<GridCoords>(){start, middle, end} ;
      } else {
        // diagonal
        GridCoords middle1 = null;
        GridCoords middle2 = null;
        // try 1
        switch (start.DirectionTo(end)) {
          case DIR.DIAGUL:
            middle1 = start.Left();
            middle2 = start.Up();
            break;
          case DIR.DIAGDL:
            middle1 = start.Left();
            middle2 = start.Down();
            break;
          case DIR.DIAGUR:
            middle1 = start.Right();
            middle2 = start.Up();
            break;
          case DIR.DIAGDR:
            middle1 = start.Right();
            middle2 = start.Down();
            break;
          default:
            Debug.LogError("Got non-diagonal direction in the diagonal move case");
            return null;
        }

        if (!IsCoordOccupied(middle1)) {
          return new List<GridCoords>(){start, middle1, end};
        }

        if (!IsCoordOccupied(middle2)) {
          return new List<GridCoords>(){start, middle2, end};
        }

        return null; // both paths are blocked
      }
    }

    Debug.LogError("Failed to FindPath because pathfinding farther than 2 spaces in NYI.");
    return null;
  }

  // Returns whether the move succeeded.
  public bool Move(GridCoords source, GridCoords dest, float speed) {
    if (!IsCoordValid(source)) return false;
    if (IsCoordOccupied(dest)) return false;
    if (IsCoordTree(source)) return false; // trees don't move
    SetEntity(dest, GetEntity(source), speed);
    SetEntity(source, null, 0);

    return true;
  }

  // Initialization.
  public void InitBoard(int width, int height) {
    if (tiles == null) {
      tiles = new GridTile[width, height];
    }
    if (entities == null) {
      entities = new GridEntity[width, height];
    }

    for (int i = 0; i < entities.GetLength(0); i++) {
      for (int j = 0; j < entities.GetLength(1); j++) {
        // Tiles (TODO: improve with better assets)
        if (tiles[i, j] != null) {
          GameObject.Destroy(tiles[i, j].gameObject);
        }
        tiles[i, j] = GameObject.Instantiate(tilePrefab, this.transform).GetComponent<GridTile>();
        tiles[i, j].transform.localPosition = GridBoard.instance.GetLocalPos(new GridCoords(i, j));

        // Entities
        if (entities[i, j] != null) {
          GameObject.Destroy(entities[i, j].gameObject);
          entities[i, j] = null;
        }
      }
    }
  }

  // NOTE: also used by Unit spawning ability code to create entities.
  public void InitTile(GridCoords coords, TILE t) {
    if (t == TILE.EMPTY) return;

    GridEntity ent = null;
    if (t == TILE.ENEMY) {
      ent = GameObject.Instantiate(enemyPrefab, this.transform).GetComponent<GridEntity>();
    }
    if (t == TILE.UNIT) {
      ent = GameObject.Instantiate(unitPrefab, this.transform).GetComponent<GridEntity>();
    }
    if (t == TILE.TREE) {
      ent = GameObject.Instantiate(treePrefab, this.transform).GetComponent<GridEntity>();
    }

    SetEntity(coords, ent, Mathf.Infinity);
  }
}
