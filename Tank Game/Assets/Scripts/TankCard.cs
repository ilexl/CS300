using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;


#if UNITY_EDITOR
using UnityEditor;
#endif

[System.Serializable]
public class TankCard : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{

    [SerializeField] public bool holder = false;
    [SerializeField] public RawImage tankImage;
    [SerializeField] public TextMeshProUGUI tankRank, tankName;
    [SerializeField] public TankVarients tankVarient;

    private GameObject dragVisualInstance;
    private RectTransform dragVisualRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (holder) return;

        // Create drag visual
        dragVisualInstance = Instantiate(this.gameObject, transform.root);
        dragVisualRect = dragVisualInstance.GetComponent<RectTransform>();
        dragVisualInstance.GetComponent<RawImage>().raycastTarget = false;
        dragVisualInstance.GetComponent<TankCard>().enabled = false;

        UpdateDragVisualPosition(eventData);
        CameraMainMenu.mouseBusy = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (holder || dragVisualInstance == null) return;
        UpdateDragVisualPosition(eventData);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (holder) return;

        TankCard dropTarget = null;
        if (eventData.pointerEnter != null)
        {
            dropTarget = eventData.pointerEnter.GetComponentInParent<TankCard>();
        }

        if (dropTarget != null && dropTarget.holder)
        {
            dropTarget.SetCard(tankVarient);
        }

        if (dragVisualInstance != null)
        {
            Destroy(dragVisualInstance);
        }

        CameraMainMenu.mouseBusy = false;
    }

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

    public void OnValidate() { Start(); }

    void Start()
    {
        if(holder == false)
        {
            if(tankVarient != null)
            {
                SetCard(tankVarient);
            }
            else
            {
                ResetCard();
            }
        }
    }

    void ResetCard()
    {
        tankVarient = null;
        tankImage.texture = TankVarients.GetTextureFromString("");
        tankName.text = "Empty";
        tankRank.text = "";
    }

    public void SetCard(TankVarients tank)
    {
        tankVarient = tank;
        tankImage.texture = TankVarients.GetTextureFromString(tank.Icon);
        tankName.text = tank.tankName;
        tankRank.text = tank.Rank;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (holder && eventData.button == PointerEventData.InputButton.Right)
        {
            ResetCard();
        }
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TankCard))]
public class EDITOR_TankCard : Editor
{

    public override void OnInspectorGUI()
    {
        TankCard tc = (TankCard)target;
        tc.holder = EditorGUILayout.Toggle(label: "Holder", tc.holder);
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