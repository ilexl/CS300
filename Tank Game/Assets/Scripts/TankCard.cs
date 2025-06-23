using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

/// <summary>
/// Represents a draggable and selectable UI tank card used for loadouts or selection screens.
/// Supports drag-and-drop swapping, preference saving, and visual updates.
/// </summary>
[System.Serializable]
public class TankCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    [SerializeField] public bool holder = false;
    [SerializeField] public bool canChange = true;
    [SerializeField] public RawImage tankImage;
    [SerializeField] public TextMeshProUGUI tankRank, tankName;
    [SerializeField] public TankVarients tankVarient;

    private GameObject dragVisualInstance;
    private RectTransform dragVisualRect;

    /// <summary>
    /// Called when drag starts. Instantiates a visual copy of the card for dragging.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        if (holder) return;

        // Create drag visual
        dragVisualInstance = Instantiate(this.gameObject, transform.root);
        dragVisualInstance.GetComponent<TankCard>().canChange = false;
        dragVisualRect = dragVisualInstance.GetComponent<RectTransform>();
        dragVisualInstance.GetComponent<RawImage>().raycastTarget = false;
        dragVisualInstance.GetComponent<TankCard>().enabled = false;

        // Set anchors and pivot for correct screen positioning
        dragVisualRect = dragVisualInstance.GetComponent<RectTransform>();
        dragVisualRect.anchorMin = new Vector2(0.5f, 0.5f);
        dragVisualRect.anchorMax = new Vector2(0.5f, 0.5f);
        dragVisualRect.pivot = new Vector2(0.5f, 0.5f);
        dragVisualRect.SetParent(transform.root, true); // not under layout

        UpdateDragVisualPosition(eventData);
        CameraMainMenu.mouseBusy = true;
    }

    /// <summary>
    /// Updates the visual drag instance while dragging.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (holder || dragVisualInstance == null) return;
        UpdateDragVisualPosition(eventData);
    }

    /// <summary>
    /// Called when drag ends. Handles swapping with another card if valid.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        if (holder) return;

        TankCard dropTarget = null;
        if (eventData.pointerEnter != null)
        {
            dropTarget = eventData.pointerEnter.GetComponentInParent<TankCard>();
        }

        // Only allow swapping into a valid target holder
        if (dropTarget != null && dropTarget.holder && dropTarget.canChange)
        {
            dropTarget.SetCard(tankVarient);
        }

        if (dragVisualInstance != null)
        {
            Destroy(dragVisualInstance);
        }

        CameraMainMenu.mouseBusy = false;
    }

    /// <summary>
    /// Moves the drag visual to match the pointer position.
    /// </summary>
    private void UpdateDragVisualPosition(PointerEventData eventData)
    {
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            transform.root as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out pos
        );

        dragVisualRect.anchoredPosition = pos;
    }

    /// <summary>
    /// Called in editor when values change; refreshes the card.
    /// </summary>
    public void OnValidate() { Start(); }

    /// <summary>
    /// Initializes the card display based on whether a tank is assigned.
    /// </summary>
    void Start()
    {
        if(holder == false && canChange)
        {
            if(tankVarient != null)
            {
                SetCardNoPref(tankVarient);
            }
            else
            {
                ResetCardNoPref();
            }
        }
    }

    /// <summary>
    /// Clears the card and deletes saved preferences.
    /// </summary>
    void ResetCard()
    {
        tankVarient = null;
        tankImage.texture = TankVarients.GetTextureFromString("");
        tankName.text = "Empty";
        tankRank.text = "";
        SetPrefs();
    }

    /// <summary>
    /// Clears the card without modifying PlayerPrefs.
    /// </summary>
    void ResetCardNoPref()
    {
        tankVarient = null;
        tankImage.texture = TankVarients.GetTextureFromString("");
        tankName.text = "Empty";
        tankRank.text = "";
    }

    /// <summary>
    /// Assigns a new tank variant to this card and updates UI + preferences.
    /// </summary>
    public void SetCard(TankVarients tank)
    {
        tankVarient = tank;
        if(tankVarient != null)
        {
            tankImage.texture = TankVarients.GetTextureFromString(tank.Icon);
            tankName.text = tank.tankName;
            tankRank.text = tank.Rank;
            SetPrefs();
        }
        else
        {
            ResetCard();
        }
    }

    /// <summary>
    /// Handles left/right click logic. Left selects, right clears.
    /// </summary>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (holder && eventData.button == PointerEventData.InputButton.Right)
        {
            ResetCard();
        }
        if (holder && eventData.button == PointerEventData.InputButton.Left)
        {
            if(tankVarient != null) { TankSelection.Singleton.SelectTank(this); }
        }
    }

    /// <summary>
    /// Constructs a unique PlayerPrefs key for this card.
    /// </summary>
    string GetPrefString() { return $"TankCard-{name}"; }

    /// <summary>
    /// Loads saved preferences and applies them without re-saving.
    /// </summary>
    public void LoadPrefs()
    {
        string tankName = PlayerPrefs.GetString(GetPrefString(), "");
        TankVarients t = TankVarients.GetFromString(tankName);
        SetCardNoPref(t); // Avoid saving immediately after loading
    }

    /// <summary>
    /// Assigns a tank to the card without writing to preferences.
    /// </summary>
    void SetCardNoPref(TankVarients tank)
    {
        tankVarient = tank;
        if (tankVarient != null)
        {
            tankImage.texture = TankVarients.GetTextureFromString(tank.Icon);
            tankName.text = tank.tankName;
            tankRank.text = tank.Rank;
        }
        else
        {
            ResetCardNoPref();
        }
    }

    /// <summary>
    /// Saves the current tank variant to PlayerPrefs.
    /// </summary>
    void SetPrefs()
    {
        if (tankVarient == null) { PlayerPrefs.DeleteKey(GetPrefString()); }
        else { PlayerPrefs.SetString(GetPrefString(), tankVarient.tankName); }
        if(TankSelection.Singleton != null)
        {
            TankSelection.Singleton.SavedPrefsToCards();
        }
    }
}

#if UNITY_EDITOR
/// <summary>
/// Custom inspector for TankCard that exposes setup and debug tools in editor.
/// </summary>
[CustomEditor(typeof(TankCard))]
public class EDITOR_TankCard : Editor
{
    public override void OnInspectorGUI()
    {
        TankCard tc = (TankCard)target;
        tc.holder = EditorGUILayout.Toggle(label: "Holder", tc.holder);
        if (tc.holder)
        {
            tc.canChange = EditorGUILayout.Toggle(label: "Can Change?", tc.canChange);
        }
        EditorGUILayout.Space(10);
        tc.tankImage = (RawImage)EditorGUILayout.ObjectField("CARD Tank Image", tc.tankImage, typeof(RawImage), true);
        tc.tankRank = (TextMeshProUGUI)EditorGUILayout.ObjectField("CARD Tank Rank", tc.tankRank, typeof(TextMeshProUGUI), true);
        tc.tankName = (TextMeshProUGUI)EditorGUILayout.ObjectField("CARD Tank Name", tc.tankName, typeof(TextMeshProUGUI), true);
        EditorGUILayout.Space(10);

        if (tc.holder)
        {
            tc.tankVarient = (TankVarients)EditorGUILayout.ObjectField("Held Tank Varient", tc.tankVarient, typeof(TankVarients), true);
        }
        else
        {
            tc.tankVarient = (TankVarients)EditorGUILayout.ObjectField("Tank Varient", tc.tankVarient, typeof(TankVarients), true);   
        }


        if (GUI.changed)
        {
            EditorUtility.SetDirty(tc); // Makes sure changes are saved
            tc.OnValidate();
        }
    }
}
#endif