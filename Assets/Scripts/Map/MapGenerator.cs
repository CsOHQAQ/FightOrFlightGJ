using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    public Vector2Int MapSize;
    public int[,] Map;
    public int RoomCount;
    public List<GameObject> RoomList;
    public GameObject Passage;
    List<GameObject> testMap;
    void Start()
    {
        testMap = new List<GameObject>();
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
            GenerateMap();
    }

    public void GenerateMap()
    {
        Randomer rnd=new Randomer();

        //Init the Map
        Map=new int[MapSize.x,MapSize.y];
        for(int i = 0; i < MapSize.x; i++)
        {
            for(int j = 0; j < MapSize.y; j++)
            {
                Map[i,j] = 0;
            }
        }
        //Generate map
        Vector2Int[] direction = new Vector2Int[4] { Vector2Int.up, Vector2Int.down, Vector2Int.left, Vector2Int.right};
        List<Vector2Int> cellList=new List<Vector2Int>();
        cellList.Add(new Vector2Int((int)MapSize.x/2, (int)MapSize.y/ 2));
        Map[(int)MapSize.x / 2, (int)MapSize.y / 2] = 1;
        int curRoomCount = 1;
        while (cellList.Count > 0) 
        {
            Vector2Int curCell=cellList[0];
            cellList.RemoveAt(0);
            for (int i = 0; i < 4; i++)
            {
                Vector2Int nextCell=curCell+direction[i];
                if(nextCell.x<0||nextCell.x>=MapSize.x|| nextCell.y < 0 || nextCell.y >= MapSize.y)//Out of border check
                {
                    continue;
                }
                
                if (Map[nextCell.x, nextCell.y] == 1||
                    curRoomCount>=RoomCount||
                    (rnd.nextFloat()>0.5f&&cellList.Count>1))
                {
                    continue;
                }
                int neighborCount = 0;
                for(int j=0;j<4; j++)
                {
                    Vector2Int neighbor=nextCell+direction[j];
                    if (neighbor.x < 0 || neighbor.x >= MapSize.x || neighbor.y < 0 || neighbor.y >= MapSize.y)//Out of border check
                    {
                        continue;
                    }
                    if (Map[neighbor.x,neighbor.y]==1)
                        neighborCount++;
                }
                if (neighborCount >= 2)
                    continue;

                curRoomCount++;
                Map[nextCell.x, nextCell.y] = 1; 
                cellList.Add(nextCell);
            }

        }

        //Putting gameobjs
        foreach(var go in testMap)
            Destroy(go);
        testMap.Clear();

        for (int i = 0; i < MapSize.x; i++)
        {
            for (int j = 0; j < MapSize.y; j++)
            {
                if (Map[i, j] == 1)
                {
                    GameObject g=Instantiate(Passage);
                    g.transform.position = new Vector2(i, j);
                    testMap.Add(g);
                }

            }
        }

    }


}
