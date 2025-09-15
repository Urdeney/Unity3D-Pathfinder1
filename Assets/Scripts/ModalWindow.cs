using System;
using System.Globalization;
using UnityEngine;

public class ModalWindow
{
    readonly int Id;
    public bool Active;
    string WindowInput;
    readonly Action restore;

    public ModalWindow(int id, Action action)
    {
        Id = id;
        Active = false;
        restore = action;
        WindowInput = PathNode.Coefficient.ToString();
    }

    public void MainLoop() => GUI.Window(Id, new(20, 20, 300, 75), WinMain, "Коэффициент взвешивания высоты:");

    void WinMain(int _)
    {
        WindowInput = GUILayout.TextField( WindowInput); // new(10, 22, 280, 20),
        if (GUILayout.Button("Принять")) // new(10, 45, 280, 20),
        {
            if (float.TryParse(WindowInput, NumberStyles.Number, CultureInfo.InvariantCulture, out PathNode.Coefficient))
            {
                Active = false;
                restore();
            }
        }
    }
}
