using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BattleBoard : MonoBehaviour
{
    public int width = 3;
    public int height = 6;
    public float cellSize = 1f;
    public GameObject Player2Prefab, Player1Prefab;

    public Material blue, red;
    public float offSet = 0.3f;

    private void Start()
    {
        GenerateGrid();
        transform.Rotate(0, 90, 0);
    }

    //Generate 3x6 battle stage, half of them will be colored blue, the other one will be red
    void GenerateGrid()
    {
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height - 3; y++)
            {
                Vector3 cellPosition = new Vector3(x * (cellSize + offSet), 0f, y * (cellSize + offSet));
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = cellPosition;
                cube.transform.localScale = new Vector3(cellSize, .3f, cellSize);
                cube.transform.parent = transform;
                cube.tag = "RedField";
                if (red != null)
                {
                    cube.GetComponent<Renderer>().material = red;
                }
            }

            for (int y = 3; y < height; y++)
            {
                Vector3 cellPosition = new Vector3(x * (cellSize + offSet), 0f, y * (cellSize + offSet));
                GameObject cube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                cube.transform.position = cellPosition;
                cube.transform.localScale = new Vector3(cellSize, .3f, cellSize);
                cube.transform.parent = transform;
                cube.tag = "BlueField";

                if (blue != null)
                {
                    cube.GetComponent<Renderer>().material = blue;
                }
            }
        }
    }

    public void SpawnPrefabAtCoordinate(Vector2Int coordinates, GameObject prefab)
    {
        float xPosition = coordinates.x * (cellSize + offSet);
        float zPosition = coordinates.y * (cellSize + offSet);
        Vector3 spawnPosition = new Vector3(xPosition, 0.3f, zPosition);

        GameObject enemy = Instantiate(prefab, spawnPosition, Quaternion.identity);
        enemy.transform.parent = transform;
    }


}

