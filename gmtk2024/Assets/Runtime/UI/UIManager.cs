using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class GameStateUI
{
    public GameState state;       // Assuming GameState is an enum or class you've defined elsewhere
    public GameObject obj;
}

public class UIManager : MonoBehaviour
{
    public List<GameStateUI> uis;

    void Start()
    {
        GameManager.Instance.uiManager = this;
        GameManager.Instance.OnGameStateChanged += ActivateFor;
        ActivateFor(GameManager.Instance.GameState);
    }

    public void ActivateFor(GameState gameState)
    {
        foreach (GameStateUI ui in uis)
        {
            if (ui.state == gameState)
            {
                ui.obj.SetActive(true);  // Enable the GameObject associated with the current GameState
            }
            else
            {
                ui.obj.SetActive(false); // Disable all others
            }
        }
    }
}
