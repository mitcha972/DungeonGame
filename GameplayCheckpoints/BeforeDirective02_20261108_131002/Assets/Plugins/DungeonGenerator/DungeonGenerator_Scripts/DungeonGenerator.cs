using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DungeonGenerator : MonoBehaviour
{
    public class Cell
    {
        public bool visited = false;
        public bool[] status = new bool[4];
        public bool startCell;
        public bool endCell;
    }

    [System.Serializable]
    public class Rule
    {
        public GameObject room;
        public Vector2Int minPosition;
        public Vector2Int maxPosition;

        public bool obligatory;
        public enum RoomType {Enemy,Chest};
        public RoomType roomType;
        public int numb;

        public int ProbabilityOfSpawning(int x, int y)
        {
            // 0 - cannot spawn 1 - can spawn 2 - HAS to spawn

            if (x>= minPosition.x && x<=maxPosition.x && y >= minPosition.y && y <= maxPosition.y)
            {
                return obligatory ? 2 : 1;
            }

            return 0;
        }
    }

    public Vector2Int size;
    public int startPos = 0;
    public Rule[] rooms;
    public Vector2 offset;
    public GameObject startRoom;
    public GameObject endRoom;

    public GameObject player;

    List<Cell> board;

    // Start is called before the first frame update
    void Start()
    {
        MazeGenerator();
    }

    public void ReStart()
    {
        //Clear List
        board.Clear();

        //Destroy all rooms
        foreach(Transform child in this.transform)
        {
            Destroy(child.gameObject);
        }       

        StartCoroutine(TT());
    }

    public IEnumerator TT()
    {
        yield return new WaitForSeconds(1f);

        MazeGenerator();
    }

    bool FindStartRoom()
    {
        for (int i = 0; i < size.x; i++)      
            for (int j = 0; j < size.y; j++)
            {         
                Cell currentCell = board[(i + j * size.x)];
                if (currentCell.visited) //if an active cell
                {
                    int randi = Random.Range(0,size.x/2);
                    int randj = Random.Range(0,size.y/2);

                    if(i == randi && j == randj)
                    {
                        Debug.Log("Start room located");
                        currentCell.startCell = true;
                        return true;
                    }
                }
            }
            Debug.Log("No Start Room");
            FindStartRoom();
            return false;
    }

    bool FindEndRoom()
    {
        for (int i = 0; i < size.x; i++)      
            for (int j = 0; j < size.y; j++)
            {         
                Cell currentCell = board[(i + j * size.x)];
                if (currentCell.visited) //if an active cell
                {
                    int randi = Random.Range(size.x/2,size.x);
                    int randj = Random.Range(size.y/2,size.y);

                    if(i == randi && j == randj)
                    {
                        Debug.Log("End room located");
                        currentCell.endCell = true;
                        return true;
                    }
                }
            }
            Debug.Log("No End Room");
            FindEndRoom();
            return false;
    }


    void GenerateDungeon()
    {
        //FindStartRoom()        
    
        for (int i = 0; i < size.x; i++)
        {        
            for (int j = 0; j < size.y; j++)
            {         
                Cell currentCell = board[(i + j * size.x)];
                if (currentCell.visited) //if an active cell
                {
                    int randomRoom = -1;
                    List<int> availableRooms = new List<int>();

                    for (int k = 0; k < rooms.Length; k++)
                    {
                        int p = rooms[k].ProbabilityOfSpawning(i, j);
                        int numm = rooms[k].numb;
                        //Room type

                        if(p == 2) //2 - HAS to spawn
                        {
                            randomRoom = k;
                            break;
                        } else if (p == 1 && numm > 0)//1 - Can spawn
                        {
                            availableRooms.Add(k);
                        }
                    }

                    if(randomRoom == -1)
                    {
                        if (availableRooms.Count > 0)
                        {
                            randomRoom = availableRooms[Random.Range(0, availableRooms.Count)];
                        }
                        else
                        {
                            randomRoom = 0;
                        }
                    }

                    RoomBehaviour newRoom = null;
                    if(currentCell.startCell)
                    {
                        newRoom = Instantiate(startRoom, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                        //move palyer
                        //Find Strat Room Player Spaw
                        Vector3 spawnPos = newRoom.gameObject.transform.Find("PlayerSpawn").transform.position;
                        player.transform.position = spawnPos;
                    }
                    
                    else if(currentCell.endCell)
                        newRoom = Instantiate(endRoom, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();
                    else
                        newRoom = Instantiate(rooms[randomRoom].room, new Vector3(i * offset.x, 0, -j * offset.y), Quaternion.identity, transform).GetComponent<RoomBehaviour>();

                    newRoom.UpdateRoom(currentCell.status);
                    newRoom.name += " " + i + "-" + j;

                     //Check if single room
                    rooms[randomRoom].numb -= 1;   
                    if( rooms[randomRoom].numb <= 0 ) 
                        availableRooms.Remove(randomRoom); 

                    if(i == size.x && j == size.y)
                        availableRooms.Clear();                   

                }
            }
        }      

    }

    void MazeGenerator()
    {
        board = new List<Cell>();

        for (int i = 0; i < size.x; i++)
        {
            for (int j = 0; j < size.y; j++)
            {
                board.Add(new Cell());
            }
        }

        int currentCell = startPos;

        Stack<int> path = new Stack<int>();

        int k = 0;

        //Fuck??
        while (k<1000)
        {
            k++;

            board[currentCell].visited = true;

            if(currentCell == board.Count - 1)
            {
                break;
            }

            //Check the cell's neighbors
            List<int> neighbors = CheckNeighbors(currentCell);

            if (neighbors.Count == 0)
            {
                if (path.Count == 0)
                {
                    break;
                }
                else
                {
                    currentCell = path.Pop();
                }
            }
            else
            {
                path.Push(currentCell);

                int newCell = neighbors[Random.Range(0, neighbors.Count)];

                if (newCell > currentCell)
                {
                    //down or right
                    if (newCell - 1 == currentCell)
                    {
                        board[currentCell].status[2] = true;
                        currentCell = newCell;
                        board[currentCell].status[3] = true;
                    }
                    else
                    {
                        board[currentCell].status[1] = true;
                        currentCell = newCell;
                        board[currentCell].status[0] = true;
                    }
                }
                else
                {
                    //up or left
                    if (newCell + 1 == currentCell)
                    {
                        board[currentCell].status[3] = true;
                        currentCell = newCell;
                        board[currentCell].status[2] = true;
                    }
                    else
                    {
                        board[currentCell].status[0] = true;
                        currentCell = newCell;
                        board[currentCell].status[1] = true;
                    }
                }

            }
            
        }
        int cellsMade = 0;
        //Chck Dugeon size
        for (int i = 0; i < size.x; i++)      
            for (int j = 0; j < size.y; j++)
            {         
                Cell currentCel = board[(i + j * size.x)];
                if (currentCel.visited) //if an active cell
                {
                    cellsMade += 1;
                }
            }

        if(cellsMade < (size.x * size.y)/2)
        {
            Debug.Log("Too small :(((");
            //remake cells
            board.Clear();
            MazeGenerator();
            return;
        }


        FindStartRoom();            
        FindEndRoom();
        
        GenerateDungeon();
        //board.Clear();
    }

    List<int> CheckNeighbors(int cell)
    {
        List<int> neighbors = new List<int>();

        //check up neighbor
        if (cell - size.x >= 0 && !board[(cell-size.x)].visited)
        {
            neighbors.Add((cell - size.x));
        }

        //check down neighbor
        if (cell + size.x < board.Count && !board[(cell + size.x)].visited)
        {
            neighbors.Add((cell + size.x));
        }

        //check right neighbor
        if ((cell+1) % size.x != 0 && !board[(cell +1)].visited)
        {
            neighbors.Add((cell +1));
        }

        //check left neighbor
        if (cell % size.x != 0 && !board[(cell - 1)].visited)
        {
            neighbors.Add((cell -1));
        }

        return neighbors;
    }
}
