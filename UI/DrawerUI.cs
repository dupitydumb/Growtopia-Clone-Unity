using UnityEngine;
using UnityEngine.EventSystems;

public class DrawerUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private RectTransform rectTransform;
    private Vector2 initialPosition;
    public float topLimit = 0f; // Set these in the inspector
    public float bottomLimit = -500f; // Set these in the inspector

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        initialPosition = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Optional: Implement logic for when dragging starts
    }

    public void OnDrag(PointerEventData eventData)
    {
        // Calculate new position
        var newPosition = rectTransform.anchoredPosition + eventData.delta;
        
        // Constrain movement to vertical only within limits
        newPosition.x = initialPosition.x;
        newPosition.y = Mathf.Clamp(newPosition.y, bottomLimit, topLimit);

        rectTransform.anchoredPosition = newPosition;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Optional: Implement logic for when dragging ends
        // For example, snapping to top or bottom
    }
}