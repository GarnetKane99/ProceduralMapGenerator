using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

enum gridSpace
{
    EMPTY,
    WALL,
    FLOOR
}

public class MapGeneration : MonoBehaviour
{
    gridSpace[,] grid;
    public int roomHeight, roomWidth;

    Vector2 roomSizeUnits = new Vector2(60, 60); //roomsize units can change depending on current floor/rooms encountered
    float unitsInOneCell = 1; //give it a dimension of 1x1

    public Tilemap TilesInScene;

    [Header("Walls")]
    public Tile WallTiles;

    [Header("Floors")]
    public Tile[] FloorTiles;

    public struct WalkerCoords
    {
        public Vector2 Dir;
        public Vector2 Pos;
    }

    [SerializeField]
    List<WalkerCoords> DrunkBois;

    float chanceDestroyed = 0.05f;
    float chanceSpawn = 0.05f;
    float chanceChange = 0.5f;
    float waitTime = 0.01f;
    float percentToFill = 0.2f;
    int maxWalkers = 10;
    public int floorCounter = 0;
    public bool cancelGen = false;

    // Start is called before the first frame update
    void Start()
    {
        //TilesInScene = GameObject.FindGameObjectWithTag("Ground").GetComponent<Tilemap>();

        //SetupRoom();
    }

    public void SetupRoom()
    {
        roomWidth = Mathf.RoundToInt(roomSizeUnits.x / unitsInOneCell);
        roomHeight = Mathf.RoundToInt(roomSizeUnits.y / unitsInOneCell);

        grid = new gridSpace[roomWidth, roomHeight];

        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                grid[x, y] = gridSpace.EMPTY;
            }
        }

        TilesInScene.SetTile(new Vector3Int(roomWidth / 2, roomHeight / 2, 0), FloorTiles[Random.Range(0, FloorTiles.Length)]);

        for (int i = TilesInScene.cellBounds.xMin; i < TilesInScene.cellBounds.xMax; i++)
        {
            for (int j = TilesInScene.cellBounds.yMin; j < TilesInScene.cellBounds.yMax; j++)
            {
                Vector3Int LocalPos = new Vector3Int(i, j, (int)TilesInScene.transform.position.y);
                Vector3 Pos = TilesInScene.CellToWorld(LocalPos);

                if (TilesInScene.HasTile(LocalPos))
                {
                    Vector3 WalkerPos = Pos;
                    //WalkerPos.x += 0.5f;
                    //WalkerPos.y += 0.5f;

                    //For struct object
                    DrunkBois = new List<WalkerCoords>();
                    WalkerCoords newWalker = new WalkerCoords();
                    newWalker.Dir = RandomDir();
                    newWalker.Pos = Pos;
                    DrunkBois.Add(newWalker);
                    StartCoroutine(CreateFloors());
                }
                else
                {
                    //nothing
                }
            }
        }
    }

    Vector2 RandomDir()
    {
        int choice = Mathf.FloorToInt(Random.value * 3.99f);

        switch (choice)
        {
            case 0:
                return Vector2.down;
            case 1:
                return Vector2.left;
            case 2:
                return Vector2.up;
            default:
                return Vector2.right;
        }
    }

    public IEnumerator CreateFloors()
    {
        int iterations = 0;
        do
        {
            bool Spawned = false;
            foreach (WalkerCoords Walkers in DrunkBois)
            {
                if (grid[(int)Walkers.Pos.x, (int)Walkers.Pos.y] != gridSpace.FLOOR)
                {
                    TilesInScene.SetTile(new Vector3Int((int)Walkers.Pos.x, (int)Walkers.Pos.y, 0), FloorTiles[Random.Range(0, FloorTiles.Length)]);
                    Spawned = true;
                    floorCounter++;
                }
                grid[(int)Walkers.Pos.x, (int)Walkers.Pos.y] = gridSpace.FLOOR;
            }
            //Random chance to remove Walker
            int Checks = DrunkBois.Count;
            for (int i = 0; i < Checks; i++)
            {
                if (Random.value < chanceDestroyed && DrunkBois.Count > 1)
                {
                    DrunkBois.RemoveAt(i);
                    break;
                }
            }
            //Change direction of walker
            for (int i = 0; i < DrunkBois.Count; i++)
            {
                if (Random.value < chanceChange)
                {
                    WalkerCoords CurWalker = DrunkBois[i];
                    CurWalker.Dir = RandomDir();
                    DrunkBois[i] = CurWalker;
                }
            }
            //Create new walker
            Checks = DrunkBois.Count;
            for (int i = 0; i < Checks; i++)
            {
                if (Random.value < chanceSpawn && DrunkBois.Count < maxWalkers)
                {
                    WalkerCoords newWalker = new WalkerCoords();
                    newWalker.Dir = RandomDir();
                    //newWalker.Pos = DrunkBois[i].Pos;
                    newWalker.Pos = DrunkBois[i].Pos;
                    DrunkBois.Add(newWalker);
                    //break;
                }
            }
            //Moving position of walker
            for (int i = 0; i < DrunkBois.Count; i++)
            {
                WalkerCoords CurWalker = DrunkBois[i];
                CurWalker.Pos += CurWalker.Dir;
                DrunkBois[i] = CurWalker;
            }
            for (int i = 0; i < DrunkBois.Count; i++)
            {
                WalkerCoords CurWalker = DrunkBois[i];
                CurWalker.Pos.x = Mathf.Clamp(CurWalker.Pos.x, 1, roomWidth - 2);
                CurWalker.Pos.y = Mathf.Clamp(CurWalker.Pos.y, 1, roomHeight - 2);
                DrunkBois[i] = CurWalker;
            }

            iterations++;
            if (Spawned)
                yield return new WaitForSeconds(waitTime);

            if ((float)floorCounter / (float)grid.Length > percentToFill)
            {
                StartCoroutine(StartWalls());
                break;
            }
            else if (cancelGen)
            {
                break;
            }
        } while (iterations < 100000); // define iteration amount externally so that the map can change size when it needs to : )
    }

    public IEnumerator StartWalls()
    {
        for (int x = 0; x < roomWidth - 1; x++)
        {
            for (int y = 0; y < roomHeight - 1; y++)
            {
                if (grid[x, y] == gridSpace.FLOOR)
                {
                    bool spawned = false;

                    if (grid[x, y + 1] == gridSpace.EMPTY)
                    {
                        TilesInScene.SetTile(new Vector3Int(x, y + 1, 0), WallTiles);
                        spawned = true;
                        grid[x, y + 1] = gridSpace.WALL;
                    }
                    if (grid[x, y - 1] == gridSpace.EMPTY)
                    {
                        TilesInScene.SetTile(new Vector3Int(x, y - 1, 0), WallTiles);
                        spawned = true;
                        grid[x, y - 1] = gridSpace.WALL;
                    }
                    if (grid[x + 1, y] == gridSpace.EMPTY)
                    {
                        TilesInScene.SetTile(new Vector3Int(x + 1, y, 0), WallTiles);
                        spawned = true;
                        grid[x + 1, y] = gridSpace.WALL;
                    }
                    if (grid[x - 1, y] == gridSpace.EMPTY)
                    {
                        TilesInScene.SetTile(new Vector3Int(x - 1, y, 0), WallTiles);
                        spawned = true;
                        grid[x - 1, y] = gridSpace.WALL;
                    }

                    if (spawned)
                    {
                        yield return new WaitForSeconds(waitTime);
                    }
                }
            }
        }
    }

    public void clearMap()
    {
        for (int x = 0; x < roomWidth; x++)
        {
            for (int y = 0; y < roomHeight; y++)
            {
                if (grid[x, y] == gridSpace.FLOOR || grid[x, y] == gridSpace.WALL)
                {
                    TilesInScene.SetTile(new Vector3Int(x, y, 0), null);
                    grid[x, y] = gridSpace.EMPTY;
                }
            }
        }
    }
}
