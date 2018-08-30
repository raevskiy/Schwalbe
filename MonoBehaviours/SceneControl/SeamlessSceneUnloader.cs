using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SeamlessSceneUnloader : MonoBehaviour {

    [SerializeField]
    private SeamlessSceneLoader loader;

    private void OnTriggerExit(Collider other)
    {
        loader.Unload();
    }
}
