using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;

namespace KopliSoft.Interaction
{
    public class CutsceneTrigger : MonoBehaviour
    {
        [SerializeField]
        private GameObject target;
        [SerializeField]
        private PlayableDirector playableDirector;

        private bool cutscenePlayed;

        private void OnTriggerEnter(Collider other)
        {
            if (!cutscenePlayed && other.gameObject == target)
            {
                playableDirector.enabled = true;
                cutscenePlayed = true;
            }
        }

    }
}
