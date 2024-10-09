using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonTransition : MonoBehaviour
{
    public void TransitionButtonOnClicked(string scene)
    {
        GameSceneManager.Instance.TransitionToScene(scene);
    }

    public void TransitionButtonQUITOnClicked()
    {
        GameSceneManager.Instance.QuitGame();
    }
}
