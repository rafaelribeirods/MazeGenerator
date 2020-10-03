using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Dynamic;
using System.Linq;
using System.Runtime.CompilerServices;
using UnityEngine;

public class MazeGenerator : MonoBehaviour
{

    public Cell[,] grid;
    public int rows = 6;
    public int columns = 5;

    public float cellWidth = 1;
    public float cellHeight = 1;
    public float cellThickness = 1;

    public bool top = false;
    public bool bottom = true;

    public Material material;

    // Start is called before the first frame update
    void Start()
    {
        UnityEngine.Debug.Log("Creating a maze with " + this.rows + " rows and " + this.columns + " columns");
        this.initilizeGrid();
        this.generate(0, 0);
        this.build();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void initilizeGrid() {
        this.grid = new Cell[this.rows, this.columns];
        for (int i = 0; i < this.rows; i++) {
            for (int j = 0; j < this.columns; j++) {
                this.grid[i, j] = new Cell(i, j);
            }
        }
    }

    private void debugGrid() {
        string grid = "";
        for (int i = 0; i < this.rows; i++) {
            for (int j = 0; j < this.columns; j++) {
                grid += this.grid[i, j].hasBeenVisited() ? "1 " : "0 ";
            }
            grid += "\n";
        }
        UnityEngine.Debug.Log(grid);
    }

    private void debugNeighbors(List<Vector2> neighbors) {
        string neighbors_str = "";
        foreach(Vector2 neighbor in neighbors) {
            neighbors_str += "[" + neighbor.x + ", " + neighbor.y + "], ";
        }
        UnityEngine.Debug.Log(neighbors_str);
    }

    private bool thereAreUnvisitedCells() {
        for (int i = 0; i < this.rows; i++) {
            for (int j = 0; j < this.columns; j++) {
                if (!this.grid[i, j].hasBeenVisited()) {
                    return true;
                }
            }
        }
        return false;
    }

    private List<Vector2> getUnvisitedNeighbors(List<Vector2> neighbors) {
        List<Vector2> unvisited_neighbors = new List<Vector2>();
        foreach (Vector2 neighbor in neighbors) {
            if (!this.grid[(int)neighbor.x, (int)neighbor.y].hasBeenVisited()) {
                unvisited_neighbors.Add(neighbor);
            }
        }
        return unvisited_neighbors;
    }

    private Cell chooseRandomNeighbor(List<Vector2> neighbors) {
        Vector2 coordinates = neighbors.ElementAt(Random.Range(0, neighbors.Count));
        return this.grid[(int)coordinates.x, (int)coordinates.y];
    }

    private void generate(int initialCellRow, int initialCellColumn) {
        Stack<Cell> stack = new Stack<Cell>();
        Cell currentCell = this.grid[initialCellRow, initialCellColumn];
        currentCell.visit();
        while(this.thereAreUnvisitedCells()) {
            List<Vector2> neighbors = currentCell.getNeighbors(this.rows, this.columns);
            List<Vector2> unvisited_neighbors = this.getUnvisitedNeighbors(neighbors);
            if (unvisited_neighbors.Count > 0) {
                Cell chosenCell = this.chooseRandomNeighbor(unvisited_neighbors);
                stack.Push(currentCell);
                currentCell.removeWall(chosenCell);
                chosenCell.removeWall(currentCell);
                currentCell = chosenCell;
                currentCell.visit();
            }
            else if (stack.Count > 0) {
                currentCell = stack.Pop();
            }
        }
    }

    private void build() {
        GameObject maze = new GameObject("Maze");
        for (int i = 0; i < this.rows; i++) {
            for (int j = 0; j < this.columns; j++) {
                this.grid[i, j].build(this.cellWidth, this.cellHeight, this.cellThickness, this.rows, this.columns, this.top, this.bottom, maze, this.material);
            }
        }
    }

}

public class Cell
{

    private int row;
    private int column;
    private bool visited;
    private bool front;
    private bool back;
    private bool left;
    private bool right;

    public Cell(int row, int column) {
        this.row = row;
        this.column = column;
        this.visited = false;
        this.front = true;
        this.back = true;
        this.left = true;
        this.right = true;
    }

    public bool hasBeenVisited() {
        return this.visited;
    }

    public void visit() {
        this.visited = true;
    }

    public List<Vector2> getNeighbors(int rows, int columns) {
        
        List<Vector2> neighbors = new List<Vector2>();

        if (this.row - 1 >= 0) { // Top neighbor
            neighbors.Add(new Vector2(this.row - 1, this.column)); 
        }

        if (this.column + 1 <= columns - 1) { // Right neighbor
            neighbors.Add(new Vector2(this.row, this.column + 1));
        }

        if (this.row + 1 <= rows - 1) { // Bottom neighbor
            neighbors.Add(new Vector2(this.row + 1, this.column));
        }

        if (this.column - 1 >= 0) { // Left neighbor
            neighbors.Add(new Vector2(this.row, this.column - 1));
        }

        return neighbors;

    }

    public void removeWall(Cell neighbor) {

        if(neighbor.row == this.row - 1) { // Top neighbor
            this.front = false;
        }

        if (neighbor.column == this.column + 1) { // Right neighbor
            this.right = false;
        }

        if (neighbor.row == this.row + 1) { // Bottom neighbor
            this.back = false;
        }

        if (neighbor.column == this.column - 1) { // Left neighbor
            this.left = false;
        }

    }

    public void build(float width, float height, float thickness, int rows, int columns, bool top, bool bottom, GameObject maze, Material material) {

        if (bottom) {
            GameObject bttm = GameObject.CreatePrimitive(PrimitiveType.Cube);
            bttm.transform.localScale = new Vector3(width, thickness, width);
            bttm.transform.position = new Vector3(this.column * width, thickness / 2, -this.row * width);
            bttm.GetComponent<Renderer>().material = material;
            bttm.transform.parent = maze.transform;
        }

        if (this.front) {

            GameObject front = GameObject.CreatePrimitive(PrimitiveType.Cube);

            if (this.column == 0) {
                front.transform.localScale = new Vector3(width + thickness, height, thickness);
                front.transform.position = new Vector3(this.column * width + thickness / 2, thickness + height / 2, width / 2 - thickness / 2 - this.row * width);
            }
            else if (this.column == columns - 1) {
                front.transform.localScale = new Vector3(width + thickness, height, thickness);
                front.transform.position = new Vector3(this.column * width - thickness / 2, thickness + height / 2, width / 2 - thickness / 2 - this.row * width);
            }
            else {
                front.transform.localScale = new Vector3(width + thickness * 2, height, thickness);
                front.transform.position = new Vector3(this.column * width, thickness + height / 2, width / 2 - thickness / 2 - this.row * width);
            }

            front.GetComponent<Renderer>().material = material;
            front.transform.parent = maze.transform;

        }

        if (this.right) {
            GameObject right = GameObject.CreatePrimitive(PrimitiveType.Cube);
            right.transform.localScale = new Vector3(thickness, height, width);
            right.transform.position = new Vector3(this.column * width + (width / 2) - (thickness / 2), thickness + height / 2, -this.row * width);
            right.GetComponent<Renderer>().material = material;
            right.transform.parent = maze.transform;
        }

        if (this.back) {

            GameObject back = GameObject.CreatePrimitive(PrimitiveType.Cube);

            if (this.column == 0) {
                back.transform.localScale = new Vector3(width + thickness, height, thickness);
                back.transform.position = new Vector3(this.column * width + thickness / 2, thickness + height / 2, - (width / 2 - thickness / 2) - this.row * width);
            }
            else if (this.column == columns - 1) {
                back.transform.localScale = new Vector3(width + thickness, height, thickness);
                back.transform.position = new Vector3(this.column * width - thickness / 2, thickness + height / 2, -(width / 2 - thickness / 2) - this.row * width);
            }
            else {
                back.transform.localScale = new Vector3(width + thickness * 2, height, thickness);
                back.transform.position = new Vector3(this.column * width, thickness + height / 2, -(width / 2 - thickness / 2) - this.row * width);
            }

            back.GetComponent<Renderer>().material = material;
            back.transform.parent = maze.transform;

        }

        if (this.left) {
            GameObject left = GameObject.CreatePrimitive(PrimitiveType.Cube);
            left.transform.localScale = new Vector3(thickness, height, width);
            left.transform.position = new Vector3(this.column * width - (width / 2) + (thickness / 2), thickness + height / 2, -this.row * width);
            left.GetComponent<Renderer>().material = material;
            left.transform.parent = maze.transform;
        }

        if(top) {
            GameObject tp = GameObject.CreatePrimitive(PrimitiveType.Cube);
            tp.transform.localScale = new Vector3(width, thickness, width);
            tp.transform.position = new Vector3(this.column * width, thickness / 2 + height + thickness, -this.row * width);
            tp.GetComponent<Renderer>().material = material;
            tp.transform.parent = maze.transform;
        }

    }

}
