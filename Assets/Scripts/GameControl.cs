using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    public GameObject Player;
    public BlackOutUI blackOutUI;
    public EscapeBtn EscapeBtn;

    private static GameControl _gameControl = null;
    public static GameControl Game
    {
        get
        {
            if (_gameControl == null)
            {
                //_gameControl = new GameControl();
                _gameControl= GameObject.Find("GameControl").GetComponent<GameControl>();//Really Ugly 
                _gameControl.Init();
            }
            return _gameControl;
        }
    }

    void Init()
    {
        Player = GameObject.FindGameObjectWithTag("Player");
        blackOutUI= GameObject.Find("BlackOut").GetComponent<BlackOutUI>();
        blackOutUI.SetTransparent(0);
        EscapeBtn= GameObject.Find("EscapeBtn").GetComponent<EscapeBtn>();
    }



    // Update is called once per frame
    void Update()
    {
        
    }

    public static void Reset()
    {
        Debug.LogWarning("GameControl Reset!");
        _gameControl = null;
    }
}
