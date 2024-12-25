using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace WorldShaper
{
    public class Letterbox : TransitionAnimation
    {
        [Header("UI Elements")]
        public RectTransform top;
        public RectTransform bottom;
        public float initialHeight = 1080f;
        [Range(0f, .5f)] public float targetRatio = 0.25f;
        public float duration = 1f;

        public override IEnumerator AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) yield break;

            // Set the animating in flag to true
            animatingIn = true;

            // Enable the images
            top.gameObject.SetActive(true);
            bottom.gameObject.SetActive(true);

            // Set the anchored position to the start position
            top.anchoredPosition = new Vector2(0f, initialHeight);
            bottom.anchoredPosition = new Vector2(0f, -initialHeight);

            // Slide the image towards the end position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var topTweener = top.DOAnchorPosY(GetTargetHeight(), duration).SetUpdate(true);
                var bottomTweener = bottom.DOAnchorPosY(-GetTargetHeight(), duration).SetUpdate(true);
                yield return new WaitForSecondsRealtime(topTweener.Duration());
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var topTweener = top.DOAnchorPosY(GetTargetHeight(), duration);
                var bottomTweener = bottom.DOAnchorPosY(-GetTargetHeight(), duration);
                yield return topTweener.WaitForCompletion();
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

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var topTweener = top.DOAnchorPosY(initialHeight, duration).SetUpdate(true);
                var bottomTweener = bottom.DOAnchorPosY(-initialHeight, duration).SetUpdate(true);
                yield return new WaitForSecondsRealtime(topTweener.Duration());
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var topTweener = top.DOAnchorPosY(initialHeight, duration);
                var bottomTweener = bottom.DOAnchorPosY(-initialHeight, duration);
                yield return topTweener.WaitForCompletion();
            }

            // Disable the images
            top.gameObject.SetActive(false);
            bottom.gameObject.SetActive(false);

            // Set the animating out flag to false
            animatingOut = false;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }

        public override float GetDuration()
        {
            return duration;
        }

        public float GetTargetHeight()
        {
            return initialHeight * (1 - targetRatio);
        }
    }
}
