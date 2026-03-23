using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace WorldShaper
{
    public class ExpandShape : TransitionAnimation
    {
        [Header("Canvas Settings")]
        public Canvas canvas;
        public CanvasScaler canvasScaler;
        public CanvasGroup canvasGroup;

        [Header("UI Elements")]
        public GameObject background;
        public RectTransform shape;

        [Header("Location Settings")]
        public ObjectLocator player = ObjectLocator.Default;
        public Vector2 targetPosition;

        [Header("Animation Settings")]
        public Ease easing = Ease.InOutQuart;
        [Range(0, 25)] public float targetScale = 25f;
        public float duration = 1f;

        private void OnValidate()
        {
            // Confirm the shape reference is valid
            if (shape != null)
            {
                // Set the anchored position and scale for previewing in the editor
                shape.anchoredPosition = targetPosition;
                shape.localScale = Vector3.one * targetScale;
            }
        }

        public override async Task AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) return;

            // Set the animating in flag to true
            animatingIn = true;

            // Enable the canvas group to ensure visibility
            canvasGroup.alpha = 1f;

            // Ensure the player reference is valid
            player.FindIfNull();

            // Set the position of the shape to the player's position
            SetPosition(player.Position);

            // Turn on the background
            if (background != null) background.SetActive(true);

            // Set the anchored position to the start position, if it's not already there
            shape.anchoredPosition = targetPosition;

            // Set the scale to zero before starting the animation
            shape.localScale = Vector3.one * targetScale;

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var tweener = shape.DOScale(Vector3.zero, duration).SetEase(easing).SetUpdate(true);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var tweener = shape.DOScale(Vector3.zero, duration).SetEase(easing);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }

            // Set the animating in flag to false
            animatingIn = false;

            // Invoke the transition in event
            OnTransitionIn?.Invoke();
        }

        public override async Task AnimateTransitionOut(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingOut) return;

            // Set the animating out flag to true
            animatingOut = true;

            // Enable the canvas group to ensure visibility
            canvasGroup.alpha = 1f;

            // Ensure the player reference is valid
            player.FindIfNull();

            // Set the position of the shape to the player's position
            SetPosition(player.Position);

            // Turn on the background
            if (background != null) background.SetActive(true);

            // Ensure the rect is at the end position before starting the slide out
            shape.anchoredPosition = targetPosition;

            // Reset the scale to the original size before starting the animation
            shape.localScale = Vector3.zero;

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var tweener = shape.DOScale(Vector3.one * targetScale, duration).SetEase(easing).SetUpdate(true);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var tweener = shape.DOScale(Vector3.one * targetScale, duration).SetEase(easing);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }

            // Turn off the background
            if (background != null) background.SetActive(false);

            // Set the animating out flag to false
            animatingOut = false;

            // Disable the canvas group to hide it
            canvasGroup.alpha = 0f;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }

        public override void SetTransitionState(bool status)
        {
            // Ensure the player reference is valid
            player.FindIfNull();

            // Set the position of the shape to the player's position
            SetPosition(player.Position);

            if (status)
            {
                // Turn off the background
                if (background != null) background.SetActive(true);

                // If transitioning in, reset the shape to the initial state
                shape.anchoredPosition = targetPosition;
                shape.localScale = Vector3.zero;
            }
            else
            {
                // If transitioning out, set the shape to the target position and scale
                shape.anchoredPosition = targetPosition;
                shape.localScale = Vector3.one * targetScale;

                // Turn on the background
                if (background != null) background.SetActive(false);
            }
        }

        public void SetPosition(Vector3 position) => targetPosition = WorldToAnchoredPosition(shape, position, 0.05f);

        private Vector2 WorldToAnchoredPosition(RectTransform shape, Vector3 worldPos, float constrainToViewportMargin = -1f)
        {
            // Initialize the screen position to the world position
            Vector2 screenPos = Vector2.zero;

            // to force the dialogue bubble to be fully on screen, clamp the bubble rectangle within the screen bounds
            if (constrainToViewportMargin >= 0f)
            {
                bool useCanvasResolution = canvasScaler != null && canvasScaler.uiScaleMode != CanvasScaler.ScaleMode.ConstantPixelSize;
                Vector2 screenSize = Vector2.zero;
                screenSize.x = useCanvasResolution ? canvasScaler.referenceResolution.x : Screen.width;
                screenSize.y = useCanvasResolution ? canvasScaler.referenceResolution.y : Screen.height;

                // calculate "half" values because we are measuring margins based on the center, like a radius
                var halfBubbleWidth = shape.rect.width / 2;
                var halfBubbleHeight = shape.rect.height / 2;

                // to calculate margin in UI-space pixels, use a % of the smaller screen dimension
                var margin = screenSize.x < screenSize.y ? screenSize.x * constrainToViewportMargin : screenSize.y * constrainToViewportMargin;

                // finally, clamp the screenPos fully within the screen bounds, while accounting for the bubble's rectTransform anchors
                screenPos.x = Mathf.Clamp( 
                    screenPos.x,
                    margin + halfBubbleWidth - shape.anchorMin.x * screenSize.x,
                    -(margin + halfBubbleWidth) - shape.anchorMax.x * screenSize.x + screenSize.x
                );

                screenPos.y = Mathf.Clamp( 
                    screenPos.y, 
                    margin + halfBubbleHeight - shape.anchorMin.y * screenSize.y, 
                    -(margin + halfBubbleHeight) - shape.anchorMax.y * screenSize.y + screenSize.y
                );
            }

            // Return the calculated anchored position
            return screenPos;
        }

        public override float GetDuration() => duration;
    }
}
