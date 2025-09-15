using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static PathNode;


public class GridScript : MonoBehaviour
{
    public static Func<PathNode, PathNode, float> Heuristic = Dist;

    /// <summary>
    ///  Модель для отрисовки узла сетки
    /// </summary>
    public GameObject nodeModel;

    /// <summary>
    ///  Ландшафт (Terrain) на котором строится путь
    /// </summary>
    [SerializeField] private Terrain landscape = null;

    /// <summary>
    ///  Шаг сетки (по x и z) для построения точек
    /// </summary>
    [SerializeField] private int gridDelta = 20;

    /// <summary>
    ///  Номер кадра, на котором будет выполнено обновление путей
    /// </summary>
    private int updateAtFrame = 0;

    /// <summary>
    ///  Массив узлов - создаётся один раз, при первом вызове скрипта
    /// </summary>
    private PathNode[,] grid = null;

    private void CheckWalkableNodes()
    {
        foreach (PathNode node in grid)
        {
            //  Пока что считаем все вершины проходимыми, без учёта препятствий
            // node.walkable = true;
            node.SetState(Physics.CheckSphere(node.body.transform.position, 1) switch
            {
                true => NodeState.Obstructed,
                false => NodeState.Walkable
            });

            if (node.Walkable)
                node.SetBlue();
            else
            {
                node.SetRed();
            }
        }
    }

    // Метод вызывается однократно перед отрисовкой первого кадра
    void Start()
    {
        //  Создаём сетку узлов для навигации - адаптивную, под размер ландшафта
        Vector3 terrainSize = landscape.terrainData.bounds.size;
        int sizeX = (int)(terrainSize.x / gridDelta);
        int sizeZ = (int)(terrainSize.z / gridDelta);

        //  Создаём и заполняем сетку вершин, приподнимая на 25 единиц над ландшафтом
        grid = new PathNode[sizeX, sizeZ];
        for (int x = 0; x < sizeX; ++x)
        {
            for (int z = 0; z < sizeZ; ++z)
            {
                Vector3 position = new(x * gridDelta, 0, z * gridDelta);
                position.y = landscape.SampleHeight(position) + 25;
                grid[x, z] = new PathNode(nodeModel, NodeState.Walkable, position)
                {
                    ParentNode = null
                };
                grid[x, z].SetBlue();
            }
        }
    }

    /// <summary>
    /// Получение списка соседних узлов для вершины сетки и расстояния до них
    /// </summary>
    /// <param name="current">индексы текущей вершины</param>
    /// <returns></returns>
    private List<(Vector2Int, float)> GetNeighbors(Vector2Int current)
    {
        List<(Vector2Int, float)> nodes = new();
        for (int dx = -1; dx <= 1; dx++)
        {
            for (int dy = -1; dy <= 1; dy++)
            {
                if (dx == 0 && dy == 0)
                    continue;

                int x = current.x + dx;
                int y = current.y + dy;

                if (0 <= x && x < grid.GetLength(0) && 0 <= y && y < grid.GetLength(1))
                {
                    nodes.Add((
                        new Vector2Int(x, y),
                        Dist(grid[current.x, current.y], grid[x, y])
                    ));
                }
            }
        }
        return nodes;
    }

    /// <summary>
    /// Вычисление кратчайшего пути между двумя вершинами сетки алгоритмом A*
    /// </summary>
    /// <param name="startNode">Координаты начального узла пути (индексы элемента в массиве grid)</param>
    /// <param name="finishNode">Координаты конечного узла пути (индексы элемента в массиве grid)</param>
    void CalculatePathA_star(Vector2Int startNode, Vector2Int finishNode)
    {
        foreach (var node in grid)
        {
            node.ParentNode = null;
            node.Distance = float.PositiveInfinity;
        }

        var start = grid[startNode.x, startNode.y];
        start.Distance = 0;

        PriorityQueue<Vector2Int, float> open = new();
        HashSet<Vector2Int> closed = new();
        open.Enqueue(Heuristic(grid[startNode.x, startNode.y], grid[finishNode.x, finishNode.y]), startNode);

        while (open.Count > 0)
        {
            var (_, node) = open.Dequeue();

            if (node == finishNode)
                break;

            if (closed.Contains(node))
                continue;

            var current = grid[node.x, node.y];
            foreach (var (coordinates, neighbor_dist) in GetNeighbors(node))
            {
                var neighbor = grid[coordinates.x, coordinates.y];
                if (!neighbor.Walkable)
                    continue;

                float new_dist = current.Distance + neighbor_dist;

                if (new_dist < neighbor.Distance)
                {
                    neighbor.ParentNode = current;
                    neighbor.Distance = new_dist;
                    float heuristic = Heuristic(neighbor, grid[finishNode.x, finishNode.y]) + new_dist;
                    open.Enqueue(heuristic, coordinates);
                }
            }
            closed.Add(node);
        }

        PathNode path = grid[finishNode.x, finishNode.y];
        print($"A* dist: {path.Distance}");
        print($"Nodes visited: {closed.Count}");
        while (path != null)
        {
            path.SetRed(NodeState.ActiveA_star);
            path = path.ParentNode;
        }
    }

    /// <summary>
    /// Вычисление кратчайшего пути между двумя вершинами сетки алгоритмом Дейкстры
    /// </summary>
    /// <param name="startNode">Координаты начального узла пути (индексы элемента в массиве grid)</param>
    /// <param name="finishNode">Координаты конечного узла пути (индексы элемента в массиве grid)</param>
    void CalculatePathDijkstra(Vector2Int startNode, Vector2Int finishNode)
    {
        foreach (var node in grid)
        {
            node.ParentNode = null;
            node.Distance = float.PositiveInfinity;
        }

        var start = grid[startNode.x, startNode.y];
        start.Distance = 0;

        PriorityQueue<Vector2Int, float> queue = new();
        HashSet<Vector2Int> visited = new();
        queue.Enqueue(0, startNode);

        while (queue.Count > 0)
        {
            var (dist, node) = queue.Dequeue();

            if (node == finishNode)
                break;

            if (visited.Contains(node))
                continue;

            var current = grid[node.x, node.y];
            foreach (var (coordinates, neighbor_dist) in GetNeighbors(node))
            {
                var neighbor = grid[coordinates.x, coordinates.y];
                if (!neighbor.Walkable)
                    continue;

                var new_dist = dist + neighbor_dist;

                if (new_dist < neighbor.Distance)
                {
                    neighbor.ParentNode = current;
                    neighbor.Distance = new_dist;
                    queue.Enqueue(new_dist, coordinates);
                }
            }
            visited.Add(node);
        }

        PathNode path = grid[finishNode.x, finishNode.y];
        print($"Dijkstra dist: {path.Distance}");
        print($"Nodes visited: {visited.Count}");
        while (path != null)
        {
            path.SetRed(NodeState.ActiveDijkstra);
            path = path.ParentNode;
        }
    }

    // Метод вызывается каждый кадр
    void Update()
    {
        //  Чтобы не вызывать этот метод каждый кадр, устанавливаем интервал вызова в 1000 кадров
        if (Time.frameCount < updateAtFrame)
            return;
        updateAtFrame = Time.frameCount + 1000;

        CheckWalkableNodes();

        Vector2Int start = new(0, 0);
        Vector2Int finish = new(grid.GetLength(0) - 1, grid.GetLength(1) - 1);

        CalculatePathA_star(start, finish);
        CalculatePathDijkstra(start, finish);
    }
}
