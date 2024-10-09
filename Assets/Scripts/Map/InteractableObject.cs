using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InteractableObject : MonoBehaviour
{
    public abstract void Interact(object args = null);
    // Start is called before the first frame update

}
