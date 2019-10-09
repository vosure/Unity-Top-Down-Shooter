using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Map[] maps;
    public int mapIndex;

    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Transform navmeshFloor;
    public Transform navmeshMaskPrefab;

    public Vector2 maxMapSize;

    [Range(0, 1)]
    public float oulinePercent;

    public float tileSize;

    List<Coordinate> tileCoords;
    Queue<Coordinate> shuffledTileCoords;
    Queue<Coordinate> shuffledOpenTileCoords;

    Transform[,] tileMap;

    Map currentMap;

    private void Start()
    {
        FindObjectOfType<Spawner>().OnNewWave += OnNewWave;
    }

    void OnNewWave(int waveNumber)
    {
        mapIndex = waveNumber - 1;
        GenerateMap();
    }

    public void GenerateMap()
    {
        currentMap = maps[mapIndex];

        tileMap = new Transform[currentMap.mapSize.x, currentMap.mapSize.y];

        System.Random randomizer = new System.Random(currentMap.seed);

        GetComponent<BoxCollider>().size = new Vector3(currentMap.mapSize.x * tileSize, 0.05f, currentMap.mapSize.y * tileSize);

        tileCoords = new List<Coordinate>();
        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                tileCoords.Add(new Coordinate(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coordinate>(Utility.ShuffleArray(tileCoords.ToArray(), currentMap.seed));

        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < currentMap.mapSize.x; x++)
        {
            for (int y = 0; y < currentMap.mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - oulinePercent) * tileSize;
                newTile.parent = mapHolder;
                tileMap[x, y] = newTile;
            }
        }

        bool[,] obstacleMap = new bool[(int)currentMap.mapSize.x, (int)currentMap.mapSize.y];

        int obstacleCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y * currentMap.obstaclePercent);
        int currentObstacleCount = 0;

        List<Coordinate> openCoords = new List<Coordinate>(tileCoords);

        for (int i = 0; i < obstacleCount; i++)
        {
            Coordinate randomCoord = GetRandomCoordinate();
            obstacleMap[randomCoord.x, randomCoord.y] = true;
            currentObstacleCount++;

            if (randomCoord != currentMap.mapCentre && MapIsFullyAccessible(obstacleMap, currentObstacleCount))
            {
                float obstacleHeight = Mathf.Lerp(currentMap.minObstacleHeight, currentMap.maxObstacleHeight, (float)randomizer.NextDouble());
                Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);

                Transform obstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up *obstacleHeight / 2, Quaternion.identity) as Transform;
                obstacle.localScale = new Vector3((1 - oulinePercent) * tileSize, obstacleHeight, (1 - oulinePercent) * tileSize);
                obstacle.parent = mapHolder;

                Renderer obstacleRenderer = obstacle.GetComponent<Renderer>();
                Material obstacleMaterial = new Material(obstacleRenderer.sharedMaterial);
                float colorPercent = randomCoord.y / (float)currentMap.mapSize.y;
                obstacleMaterial.color = Color.Lerp(currentMap.foregroundColor, currentMap.backgroundColor, colorPercent);
                obstacleRenderer.sharedMaterial = obstacleMaterial;

                openCoords.Remove(randomCoord);
            }
            else
            {
                obstacleMap[randomCoord.x, randomCoord.y] = false;
                currentObstacleCount--;
            }
        }

        shuffledOpenTileCoords = new Queue<Coordinate>(Utility.ShuffleArray(openCoords.ToArray(), currentMap.seed));

        Transform maskLeft = Instantiate(navmeshMaskPrefab, Vector3.left * (currentMap.mapSize.x + maxMapSize.x) / 4.0f * tileSize, Quaternion.identity) as Transform;
        maskLeft.parent = mapHolder;
        maskLeft.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2.0f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskRight = Instantiate(navmeshMaskPrefab, Vector3.right * (currentMap.mapSize.x + maxMapSize.x) / 4.0f * tileSize, Quaternion.identity) as Transform;
        maskRight.parent = mapHolder;
        maskRight.localScale = new Vector3((maxMapSize.x - currentMap.mapSize.x) / 2.0f, 1, currentMap.mapSize.y) * tileSize;

        Transform maskTop = Instantiate(navmeshMaskPrefab, Vector3.forward * (currentMap.mapSize.y + maxMapSize.y) / 4.0f * tileSize, Quaternion.identity) as Transform;
        maskTop.parent = mapHolder;
        maskTop.localScale = new Vector3((maxMapSize.x), 1, (maxMapSize.y - currentMap.mapSize.y) / 2.0f) * tileSize;

        Transform maskBottom = Instantiate(navmeshMaskPrefab, Vector3.back * (currentMap.mapSize.y + maxMapSize.y) / 4.0f * tileSize, Quaternion.identity) as Transform;
        maskBottom.parent = mapHolder;
        maskBottom.localScale = new Vector3((maxMapSize.x), 1, (maxMapSize.y - currentMap.mapSize.y) / 2.0f) * tileSize;

        navmeshFloor.localScale = new Vector3(maxMapSize.x, maxMapSize.y) * tileSize;
    }

    bool MapIsFullyAccessible(bool[,] obstacleMap, int currentObstacleCount)
    {
        bool[,] mapFlags = new bool[obstacleMap.GetLength(0), obstacleMap.GetLength(1)];
        Queue<Coordinate> queue = new Queue<Coordinate>();

        queue.Enqueue(currentMap.mapCentre);
        mapFlags[currentMap.mapCentre.x, currentMap.mapCentre.y] = true;

        int accessibleTileCount = 1;

        while (queue.Count > 0)
        {
            Coordinate tile = queue.Dequeue();

            for (int x = -1; x <= 1; x++)
            {
                for (int y = -1; y <= 1; y++)
                {
                    int neighbourX = tile.x + x;
                    int neighbourY = tile.y + y;
                    if (x == 0 || y == 0)
                    {
                        if (neighbourX >= 0 && neighbourX < obstacleMap.GetLength(0) && neighbourY >= 0 && neighbourY < obstacleMap.GetLength(1))
                        {
                            if (!mapFlags[neighbourX, neighbourY] && !obstacleMap[neighbourX, neighbourY])
                            {
                                mapFlags[neighbourX, neighbourY] = true;
                                queue.Enqueue(new Coordinate(neighbourX, neighbourY));
                                accessibleTileCount++;
                            }
                        }
                    }
                }
            }
        }

        int targetAccessibleTileCount = (int)(currentMap.mapSize.x * currentMap.mapSize.y - currentObstacleCount);
        return targetAccessibleTileCount == accessibleTileCount;

    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-currentMap.mapSize.x / 2.0f + 0.5f + x, 0.0f, -currentMap.mapSize.y / 2.0f + 0.5f + y) * tileSize;
    }

    public Transform GetTileFromPosition(Vector3 position)
    {
        int x = Mathf.RoundToInt(position.x / tileSize + (currentMap.mapSize.x - 1.0f) / 2.0f);
        int y = Mathf.RoundToInt(position.z / tileSize + (currentMap.mapSize.y - 1.0f) / 2.0f);
        x = Mathf.Clamp(x, 0, tileMap.GetLength(0) - 1);
        y = Mathf.Clamp(y, 0, tileMap.GetLength(1) - 1);

        return tileMap[x, y];
    }

    public Coordinate GetRandomCoordinate()
    {
        Coordinate randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);

        return randomCoord;
    }

    public Transform GetRandomOpenTile()
    {
        Coordinate randomCoord = shuffledOpenTileCoords.Dequeue();
        shuffledOpenTileCoords.Enqueue(randomCoord);

        return tileMap[randomCoord.x, randomCoord.y];
    }

    [System.Serializable]
    public struct Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }

        public static bool operator ==(Coordinate left, Coordinate right)
        {
            return (left.x == right.x && left.y == right.y);
        }

        public static bool operator !=(Coordinate left, Coordinate right)
        {
            return !(left == right);
        }
    }

    [System.Serializable]
    public class Map
    {
        public Coordinate mapSize;

        [Range(0, 1)]
        public float obstaclePercent;
        public int seed;
        public float minObstacleHeight;
        public float maxObstacleHeight;
        public Color foregroundColor;
        public Color backgroundColor;

        public Coordinate mapCentre
        {
            get
            {
                return new Coordinate(mapSize.x / 2, mapSize.y / 2);
            }
        }
    }
}
