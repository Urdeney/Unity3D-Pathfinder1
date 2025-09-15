using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PathNode
{
    public enum NodeState
    {
        Walkable,
        ActiveA_star,
        ActiveDijkstra,
        ActiveBoth,
        Obstructed
    }
    NodeState State;
    /// <summary>
    /// Свободна для перемещения
    /// </summary>
    public bool Walkable => State != NodeState.Obstructed;
    public static float Coefficient = 40.0f;
    /// <summary>
    /// Позиция в глобальных координатах
    /// </summary>
    public Vector3 worldPosition;
    /// <summary>
    /// Шаблон объекта
    /// </summary>
    private readonly GameObject objPrefab;
    /// <summary>
    /// Объект для отрисовки
    /// </summary>
    public GameObject body;
    /// <summary>
    /// Откуда пришли
    /// </summary>
    private PathNode parentNode = null;
    static readonly Color lime = Color.green + Color.yellow;

    /// <summary>
    /// Родительская вершина - предшествующая текущей в пути от начальной к целевой
    /// </summary>
    public PathNode ParentNode
    {
        get => parentNode;
        set => SetParent(value);
    }

    /// <summary>
    /// Расстояние от начальной вершины
    /// </summary>
    private float distance = float.PositiveInfinity;

    /// <summary>
    /// Расстояние от начальной вершины до текущей (+infinity если ещё не развёртывали)
    /// </summary>
    public float Distance
    {
        get => distance;
        set => distance = value;
    }

    /// <summary>
    /// Устанавливаем родителя и обновляем расстояние от него до текущей вершины. Неоптимально - дважды расстояние считается
    /// </summary>
    /// <param name="parent"></param>
    private void SetParent(PathNode parent)
    {
        //  Указываем родителя
        parentNode = parent;
        //  Вычисляем расстояние
        if (parent != null)
            distance = parent.Distance + Vector3.Distance(body.transform.position, parent.body.transform.position);
        else
            distance = float.PositiveInfinity;
    }

    /// <summary>
    /// Конструктор вершины
    /// </summary>
    /// <param name="objPrefab">объект, который визуализируется в вершине</param>
    /// <param name="state">проходима ли вершина</param>
    /// <param name="position">мировые координаты</param>
    public PathNode(GameObject objPrefab, NodeState state, Vector3 position)
    {
        State = state;
        this.objPrefab = objPrefab;
        worldPosition = position;
        body = Object.Instantiate(this.objPrefab, worldPosition, Quaternion.identity);
    }

    /// <summary>
    /// Расстояние между вершинами - разброс по высоте учитывается дополнительно
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Dist(PathNode a, PathNode b)
    {
        Vector3 a_pos = a.body.transform.position;
        Vector3 b_pos = b.body.transform.position;
        // return Vector3.Distance(a_pos, b_pos) + Coefficient * Mathf.Abs(a_pos.y - b_pos.y);
        float dist = Vector3.Distance(a_pos, b_pos);
        float dy = b_pos.y - a_pos.y;
        if (dy > 0) // учитываем подъем в гору
            return dist + Coefficient * dy;
        else
            return dist + Coefficient / 2 * -dy;
    }

    /// <summary>
    /// Манхеттенское расстояние между двумя вершинами
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Manhattan(PathNode a, PathNode b)
    {
        Vector3 a_pos = a.body.transform.position;
        Vector3 b_pos = b.body.transform.position;
        return Mathf.Abs(a_pos.x - b_pos.x) + Mathf.Abs(a_pos.y - b_pos.y) + Mathf.Abs(a_pos.z - b_pos.z);
    }

    /// <summary>
    /// Расстояние Чебышева между двумя вершинами
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static float Chebyshev(PathNode a, PathNode b)
    {
        Vector3 a_pos = a.body.transform.position;
        Vector3 b_pos = b.body.transform.position;
        float dx = Mathf.Abs(a_pos.x - b_pos.x);
        float dy = Mathf.Abs(a_pos.y - b_pos.y);
        float dz = Mathf.Abs(a_pos.z - b_pos.z);
        return Mathf.Max(dx, dy, dz);
    }

    /// <summary>
    /// Подсветить вершину - перекрасить в красный
    /// </summary>
    public void Illuminate(NodeState state = NodeState.Walkable)
    {
        if (State == NodeState.Obstructed)
            return;
        SetState(state);
    }

    /// <summary>
    /// Снять подсветку с вершины - перекрасить в синий
    /// </summary>
    public void Fade()
    {
        if (State == NodeState.Obstructed)
            return;
        SetState(NodeState.Walkable);
    }

    public void SetState(NodeState state)
    {
        if ((State == NodeState.ActiveA_star && state == NodeState.ActiveDijkstra) ||
            (State == NodeState.ActiveDijkstra && state == NodeState.ActiveA_star))
            State = NodeState.ActiveBoth;
        else
            State = state;
        switch (State)
        {
            case NodeState.Walkable:
                body.GetComponent<Renderer>().material.color = Color.blue;
                break;
            case NodeState.Obstructed:
                body.GetComponent<Renderer>().material.color = Color.red;
                break;
            case NodeState.ActiveA_star:
                body.GetComponent<Renderer>().material.color = Color.green;
                break;
            case NodeState.ActiveDijkstra:
                body.GetComponent<Renderer>().material.color = Color.yellow;
                break;
            case NodeState.ActiveBoth:
                body.GetComponent<Renderer>().material.color = lime;
                break;
        }
    }
}
