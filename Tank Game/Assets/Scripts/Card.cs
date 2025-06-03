using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Card : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerClickHandler
{
    public enum Category
    {
        TankLoadout,
        Upgrade,
        Other
    }

    [SerializeField] bool holder = false;
    [SerializeField] Category category = Category.Other;
    [SerializeField] Card currentlyHeld = null;
    [SerializeField] GameObject dragVisualPrefab; // Set in Inspector, also used for held visual

    private GameObject dragVisualInstance;
    private RectTransform dragVisualRect;

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (holder) return;

        // Create drag visual
        dragVisualInstance = Instantiate(dragVisualPrefab, transform.root);
        dragVisualRect = dragVisualInstance.GetComponent<RectTransform>();

        // Copy this card's image to the drag visual
        RawImage originalImage = GetComponent<RawImage>();
        RawImage visualImage = dragVisualInstance.GetComponent<RawImage>();
        visualImage.texture = originalImage.texture;
        visualImage.color = originalImage.color;
        visualImage.rectTransform.sizeDelta = originalImage.rectTransform.sizeDelta;

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

        Card dropTarget = null;
        if (eventData.pointerEnter != null)
        {
            dropTarget = eventData.pointerEnter.GetComponentInParent<Card>();
        }

        if (dropTarget != null && dropTarget.holder && dropTarget.category == this.category)
        {
            dropTarget.SetHeldCard(this);
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

    public void SetHeldCard(Card heldCard)
    {
        currentlyHeld = heldCard;

        // Destroy any previous held visuals
        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        // Instantiate held visual using the drag prefab
        if (dragVisualPrefab != null)
        {
            GameObject visual = Instantiate(dragVisualPrefab, transform);
            visual.transform.localPosition = Vector3.zero;
            visual.transform.localScale = Vector3.one;

            RawImage source = heldCard.GetComponent<RawImage>();
            RawImage target = visual.GetComponent<RawImage>();
            target.texture = source.texture;
            target.color = source.color;
            target.rectTransform.sizeDelta = source.rectTransform.sizeDelta;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (holder && eventData.button == PointerEventData.InputButton.Right)
        {
            if (currentlyHeld != null)
            {
                currentlyHeld = null;

                foreach (Transform child in transform)
                {
                    Destroy(child.gameObject);
                }
            }
        }
    }
}
