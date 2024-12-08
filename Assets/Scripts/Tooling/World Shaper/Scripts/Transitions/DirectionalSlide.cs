using System.Collections;
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

        public override IEnumerator AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) yield break;

            // Set the animating in flag to true
            animatingIn = true;

            // Set the anchored position to the start position, if it's not already there
            rect.anchoredPosition = startPosition;

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var tweener = rect.DOAnchorPos(endPosition, duration).SetUpdate(true);
                yield return new WaitForSecondsRealtime(tweener.Duration());
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var tweener = rect.DOAnchorPos(endPosition, duration);
                yield return tweener.WaitForCompletion();
            }

            // Set the animating in flag to false
            animatingIn = false;

            // Invoke the transition in event
            OnTransitionIn?.Invoke();
        }

        public override IEnumerator AnimateTransitionOut(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingOut) yield break;

            // Set the animating out flag to true
            animatingOut = true;

            // Set the anchored position to the end position, if it's not already there
            rect.anchoredPosition = endPosition;

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var tweener = rect.DOAnchorPos(startPosition, duration).SetUpdate(true);
                yield return new WaitForSecondsRealtime(tweener.Duration());
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var tweener = rect.DOAnchorPos(startPosition, duration);
                yield return tweener.WaitForCompletion();
            }

            // Set the animating out flag to false
            animatingOut = false;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }
    }
}
