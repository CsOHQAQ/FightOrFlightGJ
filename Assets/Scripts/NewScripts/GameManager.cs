using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    private PlayerCharacter playerCharacter;
    public PlayerCharacter PlayerCharacter{get{return playerCharacter;}}
    public float CurrentScore = 0;
    [SerializeField]
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
        // Find the PlayerCharacter in the scene
        
        playerCharacter = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerCharacter>();

        if (playerCharacter == null)
        {
            Debug.LogError("PlayerCharacter not found in the scene. Make sure it has the tag 'Player'.");
        }
    }

    public void AddScore(float scoreToAdd)
    {
        CurrentScore+=scoreToAdd;
    }
}
