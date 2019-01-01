using KopliSoft.Behaviour;
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
        [Tooltip("Should be a child of every activated/deactivated object. Used to overcome stupid limitation of GameObject.Find, which operates only wit active objects")]
        private string aggregatorObjectName = "Aggregator";
        [SerializeField]
        private BehaviorTreeController[] charactersToBan;
        [SerializeField]
        private GameObject ban;

        public void Switch()
        {
            foreach (string objectName in objectsToDeactivate)
            {
                GameObject gameObject = GameObject.Find(objectName);
                if (gameObject != null)
                {
                    SeamlessSceneLoader loader = gameObject.GetComponentInChildren<SeamlessSceneLoader>();
                    if (loader != null)
                    {
                        loader.Unload();
                    }

                    gameObject.transform.Find(aggregatorObjectName).gameObject.SetActive(false);
                }
            }

            foreach (string objectName in objectsToActivate)
            {
                GameObject gameObject = GameObject.Find(objectName);
                if (gameObject != null)
                {
                    gameObject.transform.Find(aggregatorObjectName).gameObject.SetActive(true);
                }
            }

            for (int i = 0; i < charactersToBan.Length; i++)
            {
                BehaviorTreeController character = charactersToBan[i];
                character.TeleportToBan(ban, Vector3.forward * i);
            }
        }
    }
}
