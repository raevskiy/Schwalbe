using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Opsive.ThirdPersonController;
using KopliSoft.Inventory;

public class StorageInteractable : MonoBehaviour, IInteractable
{
    [Tooltip("The ID of the Interactable. Used for ability filtering by the character. -1 indicates no ID")]
    [SerializeField]
    protected int m_ID = -1;
    [Tooltip("The layer of objects that can perform the interaction")]
    [SerializeField]
    protected LayerMask m_InteractorLayer;
    [Tooltip("The offset that the interactor should move to when interacting. A value of -1 means no movement on that axis")]
    [SerializeField]
    protected Vector3 m_TargetInteractorOffset = new Vector3(-1, -1, -1);
    [SerializeField]
    private int lockLevel = 0;
    [SerializeField]
    private int pickpocketLevel = 0;


    private Transform m_Interactor;
    private Transform m_Transform;
    private GameObject m_InteractorGameObject;
    private StorageInventory storageInventory;

    void Start()
    {
        storageInventory = GetComponent<StorageInventory>();
    }

    /// <summary>
    /// Cache the component references and initialize the default values.
    /// </summary>
    private void Awake()
    {
        m_Transform = transform;

        // Activate when an object with the specified interactor layer is within the trigger.
        enabled = false;
    }

    /// <summary>
    /// An object has entered the trigger. Determine if it is an interactor.
    /// </summary>
    /// <param name="other">The potential interactor.</param>
    private void OnTriggerEnter(Collider other)
    {
        if (Utility.InLayerMask(other.gameObject.layer, m_InteractorLayer.value))
        {
            m_Interactor = other.transform;
            m_InteractorGameObject = other.gameObject;
            EventHandler.ExecuteEvent<IInteractable>(m_Interactor.gameObject, "OnInteractableHasInteractable", this);
            enabled = true;
        }
    }

    /// <summary>
    /// The interactor can no longer interact with the object if it leaves the trigger.
    /// </summary>
    /// <param name="other">The potential interactor.</param>
    private void OnTriggerExit(Collider other)
    {
        if (other.transform.Equals(m_Interactor))
        {
            EventHandler.ExecuteEvent<IInteractable>(m_Interactor.gameObject, "OnInteractableHasInteractable", null);
            m_Interactor = null;
            enabled = false;

            if (storageInventory != null && storageInventory.IsStorageOpened())
            {
                storageInventory.CloseStorage();
            }
        }
    }

    /// <summary>
    /// Returns the ID of the interactable object.
    /// </summary>
    /// <returns>The ID of the interactable object.</returns>
    public int GetInteractableID()
    {
        return m_ID;
    }

    /// <summary>
    /// Determines if the Interactor can interact with the InteractableTarget. Cases where the Interactor cannot interact include not facing the Interactable object or the camera
    /// not looking at the Interactable object.
    /// </summary>
    /// <returns>True if the Interactor can interact with the InteractableTarget</returns>
    public bool CanInteract()
    {
        return m_Interactor != null 
            && (pickpocketLevel == 0 || IsInCriminalMode());    //We can interact with storages in normal mode as well, only pickpockets require criminal mode
    }

    private bool IsInCriminalMode()
    {
        Criminal criminal = m_Interactor.GetComponent<Criminal>();
        return criminal != null && criminal.IsInCriminalMode();
    }

    /// <summary>
    /// Does the interactable object require the interactor to be in a target position?
    /// </summary>
    /// <returns>True if the interactor is required to be in a target position.</returns>
    public bool RequiresTargetInteractorPosition()
    {
        return m_TargetInteractorOffset.x != -1 || m_TargetInteractorOffset.y != -1 || m_TargetInteractorOffset.z != -1;
    }

    /// <summary>
    /// Returns the rotation that the interactor should face before interacting with the object.
    /// </summary>
    /// <returns>The target interactor rotation.</returns>
    public Quaternion GetTargetInteractorRotation()
    {
        // The character should be facing the opposite direction the interactable object is facing.
        return Quaternion.LookRotation(-m_Transform.forward);
    }

    /// <summary>
    /// Returns the position that the interactor should move to before interacting with the object.
    /// </summary>
    /// <param name="interactorTransform">The transform of the interactor.</param>
    /// <returns>The target interactor position.</returns>
    public Vector3 GetTargetInteractorPosition(Transform interactorTransform)
    {
        // Ignore the axis if the offset has a -1 value.
        var position = m_Transform.InverseTransformPoint(interactorTransform.position);
        if (m_TargetInteractorOffset.x != -1)
        {
            position.x = m_TargetInteractorOffset.x;
        }
        if (m_TargetInteractorOffset.y != -1)
        {
            position.y = m_TargetInteractorOffset.y;
        }
        if (m_TargetInteractorOffset.z != -1)
        {
            position.z = m_TargetInteractorOffset.z;
        }
        return m_Transform.TransformPoint(position);
    }

    /// <summary>
    /// The interactor is looking at the object and wants to perform an interaction. Perform that interaction.
    /// </summary>
    public void Interact()
    {
        if (storageInventory != null)
        {
            if (pickpocketLevel > 0)
            {
                pickPocket();
            } else {
                openLocker();
            }
        }
    }

    private void openLocker()
    {
        if (lockLevel > 0 && !IsInCriminalMode())
        {
            Fungus.Flowchart flowchart = GameObject.Find("/Fungus/Flowcharts/Messages/lockpick_requires_criminal_mode").GetComponent<Fungus.Flowchart>();
            flowchart.ExecuteBlock("Main");
            return;
        }

        int lockpick = Random.Range(1, 9);
        if (lockpick >= lockLevel)
        {
            lockLevel = 0;  //Now it is inlocked forever
            storageInventory.ToggleStorage();
        }
        else
        {
            Fungus.Flowchart flowchart = GameObject.Find("/Fungus/Flowcharts/Messages/lockpick_failed").GetComponent<Fungus.Flowchart>();
            flowchart.ExecuteBlock("Main");
        }

    }

    private void pickPocket()
    {
        int pickpocket = Random.Range(1, 9);
        if (pickpocket >= pickpocketLevel)
        {
            storageInventory.ToggleStorage();
        }
        else
        {
            Fungus.Flowchart flowchart = GameObject.Find("/Fungus/Flowcharts/Messages/pickpocket_failed").GetComponent<Fungus.Flowchart>();
            flowchart.ExecuteBlock("Main");
        }
    }
}
