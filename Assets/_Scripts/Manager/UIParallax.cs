using UnityEngine;

public class UIParallax : MonoBehaviour
{
    [Header("Parallax Settings")]
    [Tooltip("Drag your Background UI Image here")]
    public RectTransform backgroundToMove;
    
    [Tooltip("How far the image will move. Keep it subtle!")]
    public float movementAmount = 30f; 
    
    [Tooltip("How smooth and floaty the movement is.")]
    public float smoothSpeed = 5f;

    private Vector2 startPos;

    void Start()
    {
        if (backgroundToMove != null)
        {
            // Remember exactly where the background started
            startPos = backgroundToMove.anchoredPosition;
        }
    }

    void Update()
    {
        if (backgroundToMove == null) return;

        // 1. Get the mouse position
        Vector2 mousePos = Input.mousePosition;

        // 2. Normalize it so the math works regardless of screen resolution (-1 to 1)
        float normalizedX = (mousePos.x / Screen.width) * 2f - 1f;
        float normalizedY = (mousePos.y / Screen.height) * 2f - 1f;

        // 3. Calculate where the background SHOULD be based on the mouse
        // We invert it (minus sign) so the background moves OPPOSITE the mouse, which looks more 3D
        Vector2 targetPos = new Vector2(
            startPos.x - (normalizedX * movementAmount),
            startPos.y - (normalizedY * movementAmount)
        );

        // 4. Smoothly glide the background to that target position
        backgroundToMove.anchoredPosition = Vector2.Lerp(
            backgroundToMove.anchoredPosition, 
            targetPos, 
            Time.deltaTime * smoothSpeed
        );
    }
}