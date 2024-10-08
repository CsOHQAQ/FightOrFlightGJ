using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameControl : MonoBehaviour
{
    public GameObject Player;

    private static GameControl _gameControl = null;
    public static GameControl Game
    {
        get
        {
            if (_gameControl is null)
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
    }



    // Update is called once per frame
    void Update()
    {
        
    }
}
