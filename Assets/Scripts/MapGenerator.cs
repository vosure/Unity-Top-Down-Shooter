using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Transform tilePrefab;
    public Transform obstaclePrefab;
    public Vector2 mapSize;

    [Range(0, 1)]
    public float oulinePercent;

    List<Coordinate> tileCoords;
    Queue<Coordinate> shuffledTileCoords;

    public int seed = 10;

    private void Start()
    {
        GenerateMap();
    }

    public void GenerateMap()
    {
        tileCoords = new List<Coordinate>();
        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                tileCoords.Add(new Coordinate(x, y));
            }
        }
        shuffledTileCoords = new Queue<Coordinate>(Utility.ShuffleArray(tileCoords.ToArray(), seed));

        string holderName = "Generated Map";
        if (transform.FindChild(holderName))
        {
            DestroyImmediate(transform.FindChild(holderName).gameObject);
        }

        Transform mapHolder = new GameObject(holderName).transform;
        mapHolder.parent = transform;

        for (int x = 0; x < mapSize.x; x++)
        {
            for (int y = 0; y < mapSize.y; y++)
            {
                Vector3 tilePosition = CoordToPosition(x, y);
                Transform newTile = Instantiate(tilePrefab, tilePosition, Quaternion.Euler(Vector3.right * 90)) as Transform;
                newTile.localScale = Vector3.one * (1 - oulinePercent);
                newTile.parent = mapHolder;
            }
        }

        int obstacleCount = 10;
        for (int i = 0; i < obstacleCount; i++)
        {
            Coordinate randomCoord = GetRandomCoordinate();
            Vector3 obstaclePosition = CoordToPosition(randomCoord.x, randomCoord.y);
            Transform obstacle = Instantiate(obstaclePrefab, obstaclePosition + Vector3.up * 0.5f, Quaternion.identity) as Transform;
            obstacle.parent = mapHolder;
        }
    }

    Vector3 CoordToPosition(int x, int y)
    {
        return new Vector3(-mapSize.x / 2 + 0.5f + x, 0, -mapSize.y / 2 + 0.5f + y);
    }

    public Coordinate GetRandomCoordinate()
    {
        Coordinate randomCoord = shuffledTileCoords.Dequeue();
        shuffledTileCoords.Enqueue(randomCoord);
        return randomCoord;
    }


    public struct Coordinate
    {
        public int x;
        public int y;

        public Coordinate(int x, int y)
        {
            this.x = x;
            this.y = y;
        }
    }
}
