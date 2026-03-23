using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace WorldShaper
{
    public class Letterbox : TransitionAnimation
    {
        [Header("UI Elements")]
        public RectTransform top;
        public RectTransform bottom;
        public Ease easing = Ease.InOutSine;
        public float initialHeight = 1080f;
        [Range(0f, 0.5f)] public float targetRatio = 0.25f;
        public float duration = 1f;

        private void OnValidate()
        {
            // Set the top anchored position to the initial height
            if (top != null) top.anchoredPosition = new Vector2(0f, initialHeight);

            // Set the bottom anchored position to the negative initial height
            if (bottom != null) bottom.anchoredPosition = new Vector2(0f, -initialHeight);
        }

        public override async Task AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) return;

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
                var topTweener = top.DOAnchorPosY(GetTargetHeight(), duration).SetEase(easing).SetUpdate(true);
                var bottomTweener = bottom.DOAnchorPosY(-GetTargetHeight(), duration).SetEase(easing).SetUpdate(true);

                // Await the completion of the tween
                await topTweener.AsyncWaitForCompletion();
                await bottomTweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var topTweener = top.DOAnchorPosY(GetTargetHeight(), duration).SetEase(easing);
                var bottomTweener = bottom.DOAnchorPosY(-GetTargetHeight(), duration).SetEase(easing);

                // Await the completion of the tween
                await topTweener.AsyncWaitForCompletion();
                await bottomTweener.AsyncWaitForCompletion();
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

            // Enable the images
            top.gameObject.SetActive(true);
            bottom.gameObject.SetActive(true);

            // Set the anchored position to the end position
            top.anchoredPosition = new Vector2(0f, GetTargetHeight());
            bottom.anchoredPosition = new Vector2(0f, -GetTargetHeight());

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var topTweener = top.DOAnchorPosY(initialHeight, duration).SetEase(easing).SetUpdate(true);
                var bottomTweener = bottom.DOAnchorPosY(-initialHeight, duration).SetEase(easing).SetUpdate(true);

                // Await the completion of the tween
                await topTweener.AsyncWaitForCompletion();
                await bottomTweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var topTweener = top.DOAnchorPosY(initialHeight, duration).SetEase(easing);
                var bottomTweener = bottom.DOAnchorPosY(-initialHeight, duration).SetEase(easing);

                // Await the completion of the tween
                await topTweener.AsyncWaitForCompletion();
                await bottomTweener.AsyncWaitForCompletion();
            }

            // Disable the images
            top.gameObject.SetActive(false);
            bottom.gameObject.SetActive(false);

            // Set the animating out flag to false
            animatingOut = false;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }

        public override void SetTransitionState(bool status)
        {
            // Set the active state of the top and bottom images
            if (status)
            {
                // Set the top and bottom anchored positions to the target height
                if (top != null) top.anchoredPosition = new Vector2(0f, GetTargetHeight());
                if (bottom != null) bottom.anchoredPosition = new Vector2(0f, -GetTargetHeight());
            }
            else
            {
                // Reset the top and bottom anchored positions to the initial height
                if (top != null) top.anchoredPosition = new Vector2(0f, initialHeight);
                if (bottom != null) bottom.anchoredPosition = new Vector2(0f, -initialHeight);
            }
        }

        public override float GetDuration() => duration;

        public float GetTargetHeight() => initialHeight * (1 - targetRatio);
    }
}