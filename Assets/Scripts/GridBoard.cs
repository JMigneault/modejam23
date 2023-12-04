using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GridBoard : MonoBehaviour
{

  public GameObject unitPrefab;
  public GameObject enemyPrefab;

  public float gridSize = 1.0f;

  public GridEntity[,] entities;

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

  public bool SetEntity(GridCoords coords, GridEntity entity) {
    if (!IsCoordValid(coords)) {
      return false;
    }

    if (entity != null) {
      entity.SetCoords(coords);
    }
    entities[coords.i, coords.j] = entity;
    return true;
  }

  public bool IsBoundaryCoord(GridCoords coords) {
    return coords.i == 0 || coords.i == Width() - 1 || coords.j == 0 || coords.j == Height() - 1;
  }

  public bool IsCoordValid(GridCoords coords) {
    return 0 <= coords.i && coords.i < Width() && 0 <= coords.j && coords.j < Height();
  }

  // Returns true if there is an entity at coords or if coords is out of bounds.
  public bool IsCoordOccupied(GridCoords coords) {
    if (!IsCoordValid(coords)) return true; // out of bounds == blocked
    return GetEntity(coords) != null;
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
      Debug.LogError("Failed to FindPath because the ending coord was not valid");
      return null;
    }

    if (IsCoordOccupied(end)) {
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
  public bool Move(GridCoords source, GridCoords dest) {
    if (!IsCoordValid(source)) return false;
    if (IsCoordOccupied(dest)) return false;
    SetEntity(dest, GetEntity(source));
    SetEntity(source, null);
    return true;
  }

  // Initialization.
  public void InitBoard(int width, int height) {
    if (entities != null) {
      for (int i = 0; i < entities.GetLength(0); i++) {
        for (int j = 0; j < entities.GetLength(1); j++) {
          if (entities[i, j] != null) {
            GameObject.Destroy(entities[i, j].gameObject);
            entities[i, j] = null;
          }
        }
      }
    }
    entities = new GridEntity[width, height];
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

    SetEntity(coords, ent);
  }
}
