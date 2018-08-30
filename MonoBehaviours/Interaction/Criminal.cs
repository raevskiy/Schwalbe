using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Criminal : MonoBehaviour {
    [SerializeField]
    private GameObject criminalIcon;
    private bool inCriminalMode;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetButtonDown("Criminal"))
        {
            inCriminalMode = !inCriminalMode;
            if (criminalIcon != null)
            {
                criminalIcon.SetActive(inCriminalMode);
            }
        }
	}

    public bool IsInCriminalMode()
    {
        return inCriminalMode;
    }
}
