
using UnityEngine;
using UnityEngine.EventSystems;

public class ButtonNeonEffect : MonoBehaviour,
    IPointerEnterHandler, IPointerExitHandler
{
    [Header("Escala")]
    public float hoverScale = 1.08f;
    public float speed = 12f;

    private Vector3 normalScale;
    private Vector3 targetScale;

    private void Start()
    {
        normalScale = transform.localScale;
        targetScale = normalScale;
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(
            transform.localScale,
            targetScale,
            Time.unscaledDeltaTime * speed
        );
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        targetScale = normalScale * hoverScale;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        targetScale = normalScale;
    }
}
