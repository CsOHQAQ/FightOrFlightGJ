using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{

    public static GameSceneManager Instance;

    [Header("Transition Properties")]
    [SerializeField]
    private float minLoadDelay;

    [SerializeField]
    private float maxLoadDelay;

    [SerializeField]
    private string loadingSceneName;

    private bool canTransition;
    

    // Start is called before the first frame update
    void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Start()
    {
        canTransition = true;
    }

    public void QuitGame()
    {
        Application.Quit();
    }

    public void TransitionToScene(string sceneName)
    {

        if (canTransition)
        {
            canTransition = false;

            float randomDelay = Random.Range(minLoadDelay, maxLoadDelay);

            StartCoroutine(LoadSceneTransition(randomDelay, sceneName));
        }
    }

    private IEnumerator LoadSceneTransition(float delay, string sceneName)
    {

        SceneManager.LoadScene(loadingSceneName);

        yield return new WaitForSeconds(delay);

        SceneManager.LoadScene(sceneName);

        canTransition = true;
    }

    public void OnDeath()
    {
        InventoryManager.Instance.OnDeath();

        TransitionToScene("DeathScene");
    }
}
