using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace WorldShaper
{
    [System.Serializable]
    public class DirectionalSlide : TransitionAnimation
    {
        [Header("UI Elements")]
        public RectTransform rect;

        [Header("Settings")]
        public Vector2 startPosition;
        public Vector2 endPosition;
        public float duration = 1f;
        public bool adjust = false;

        private void OnValidate()
        {
            // Adjust the anchored position to the start position if needed
            if (rect != null && adjust) rect.anchoredPosition = startPosition;
        }

        public override async Task AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) return;

            // Set the animating in flag to true
            animatingIn = true;

            // Set the anchored position to the start position, if it's not already there
            rect.anchoredPosition = startPosition;

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var tweener = rect.DOAnchorPos(endPosition, duration).SetUpdate(true);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var tweener = rect.DOAnchorPos(endPosition, duration);

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

            // Ensure the rect is at the end position before starting the slide out
            rect.anchoredPosition = endPosition;

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var tweener = rect.DOAnchorPos(startPosition, duration).SetUpdate(true);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var tweener = rect.DOAnchorPos(startPosition, duration);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }

            // Set the animating out flag to false
            animatingOut = false;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }

        public override void SetTransitionState(bool status) => rect.anchoredPosition = status ? endPosition : startPosition;

        public override float GetDuration() => duration;
    }
}
