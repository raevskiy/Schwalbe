using KopliSoft.Behaviour;
using System;
using UnityEngine;

namespace KopliSoft.SceneControl
{
    public class GameObjectSwitch : MonoBehaviour
    {
        [SerializeField]
        private string[] objectsToDeactivate;
        [SerializeField]
        private string[] objectsToActivate;
        [SerializeField]
        private BehaviorTreeController[] charactersToBan;
        [SerializeField]
        private GameObject ban;

        public void Switch()
        {
            foreach (string objectName in objectsToDeactivate)
            {
                GameObject gameObject = DoFind(objectName);
                if (gameObject != null)
                {
                    SeamlessSceneLoader loader = gameObject.GetComponentInChildren<SeamlessSceneLoader>();
                    if (loader != null)
                    {
                        loader.Unload();
                    }

                    gameObject.SetActive(false);
                }
            }

            foreach (string objectName in objectsToActivate)
            {
                GameObject gameObject = DoFind(objectName);
                if (gameObject != null)
                {
                    gameObject.SetActive(true);
                }
            }

            for (int i = 0; i < charactersToBan.Length; i++)
            {
                BehaviorTreeController character = charactersToBan[i];
                if (character.isActiveAndEnabled)
                {
                    character.TeleportToBan(ban, Vector3.forward * i);
                }
            }
        }

        private GameObject DoFind(string objectPath)
        {
            string[] tokens = objectPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
            string pathToParent = "/" + string.Join("/", tokens, 0, tokens.Length - 1);
            GameObject parentGameObject = GameObject.Find(pathToParent);
            return parentGameObject.transform.Find(tokens[tokens.Length - 1]).gameObject;
        }
    }
}
