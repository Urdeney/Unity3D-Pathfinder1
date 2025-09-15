using UnityEngine;
using System.Collections;
using static PathNode;
using UnityEditor;

public class FlyCamera : MonoBehaviour
{
    public GameObject objPrefab;
    private bool spawning = false;
    private static bool attached = true;
    private int heuristic_index = 0;
    /*
    Written by Windexglow 11-13-10.  Use it, edit it, steal it I don't care.
    Converted to C# 27-02-13 - no credit wanted.
    Simple flycam I made, since I couldn't find any others made public.
    Made simple to use (drag and drop, done) for regular keyboard layout
    WASD : basic movement
    Shift : Makes camera accelerate
    Space : Moves camera on X and Z axis only.  So camera doesn't gain any height*/

    /// <summary>
    /// regular speed
    /// </summary>
    readonly float mainSpeed = 250.0f;
    /// <summary>
    /// Multiplied by how long shift is held. Basically running.
    /// </summary>
    readonly float shiftAdd = 500.0f;
    /// <summary>
    /// Maximum speed when holding shift
    /// </summary>
    readonly float maxShift = 2000.0f;
    /// <summary>
    /// How sensitive it with mouse
    /// </summary>
    readonly float camSens = 0.25f;
    /// <summary>
    /// Kind of in the middle of the screen, rather than at the top (play)
    /// </summary>
    private Vector3 lastMouse = new(255, 255, 255);
    private float totalRun = 1.0f;
    readonly ModalWindow win = new(0, () => attached = true);

    void OnGUI()
    {
        if (win.Active)
            win.MainLoop();
    }

    void Update()
    {

        if (Input.GetKeyDown(KeyCode.F2))
        {
            win.Active = true;
            attached = false;
        }

        if (Input.GetKeyDown(KeyCode.F1))
        {
            heuristic_index = (heuristic_index + 1) % 3;
            switch (heuristic_index)
            {
                case 0:
                    GridScript.Heuristic = Dist;
                    print("Switched heuristic to weighted Euclidean distance");
                    break;
                case 1:
                    GridScript.Heuristic = Manhattan;
                    print("Switched heuristic to Manhattan distance");
                    break;
                case 2:
                    GridScript.Heuristic = Chebyshev;
                    print("Switched heuristic to Chebyshev distance");
                    break;
            }
        }

        if (Input.GetKeyDown(KeyCode.Backspace))
        {
            Application.Quit();
#if UNITY_EDITOR
            EditorApplication.isPlaying = false;
#endif
        }

        // Detach from camera controls
        if (Input.GetKeyDown(KeyCode.Escape))
            attached = !attached;
        if (!attached)
            return;

        lastMouse = Input.mousePosition - lastMouse;
        lastMouse = new Vector3(-lastMouse.y * camSens, lastMouse.x * camSens, 0);
        lastMouse = new Vector3(transform.eulerAngles.x + lastMouse.x, transform.eulerAngles.y + lastMouse.y, 0);
        transform.eulerAngles = lastMouse;
        lastMouse = Input.mousePosition;
        //Mouse  camera angle done.

        //Keyboard commands
        //float f = 0.0f;
        Vector3 p = GetBaseInput();
        if (Input.GetKey(KeyCode.LeftShift))
        {
            totalRun += Time.deltaTime;
            p = shiftAdd * totalRun * p;
            p.x = Mathf.Clamp(p.x, -maxShift, maxShift);
            p.y = Mathf.Clamp(p.y, -maxShift, maxShift);
            p.z = Mathf.Clamp(p.z, -maxShift, maxShift);
        }
        else
        {
            totalRun = Mathf.Clamp(totalRun * 0.5f, 1f, 1000f);
            p *= mainSpeed;
        }

        p *= Time.deltaTime;
        Vector3 newPosition = transform.position;
        if (Input.GetKey(KeyCode.Space))
        { //If player wants to move on X and Z axis only
            transform.Translate(p);
            newPosition.x = transform.position.x;
            newPosition.z = transform.position.z;
            transform.position = newPosition;
        }
        else
        {
            transform.Translate(p);
        }

        if (Input.GetKey(KeyCode.E))
        {
            if (!spawning)
            {
                Instantiate(objPrefab, gameObject.transform.position, Quaternion.identity);
                spawning = true;
            }
        }
        else
            spawning = false;
    }

    /// <summary>
    /// Returns the basic values, if it's 0 than it's not active.
    /// </summary>
    /// <returns></returns>
    private Vector3 GetBaseInput()
    {
        Vector3 p_Velocity = new();
        if (Input.GetKey(KeyCode.W))
        {
            p_Velocity += new Vector3(0, 0, 1);
        }
        if (Input.GetKey(KeyCode.S))
        {
            p_Velocity += new Vector3(0, 0, -1);
        }
        if (Input.GetKey(KeyCode.A))
        {
            p_Velocity += new Vector3(-1, 0, 0);
        }
        if (Input.GetKey(KeyCode.D))
        {
            p_Velocity += new Vector3(1, 0, 0);
        }
        return p_Velocity;
    }
}
