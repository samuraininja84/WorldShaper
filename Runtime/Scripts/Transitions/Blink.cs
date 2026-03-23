using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace WorldShaper
{
    public class Blink : TransitionAnimation
    {
        [Header("UI Elements")]
        public RectTransform mask;
        public CanvasGroup canvasGroup;

        [Header("Blink Settings")]
        public Ease easing = Ease.InOutBounce;
        [Range(0, 1)] public float blinkBlend = 0.5f;
        [Tooltip("Heights in pixels for the closed and open states of the blink mask.")]
        public Vector2 heights = new Vector2(0f, 1800f);
        public float duration = 3f;

        private void OnValidate() => SetMaskHeight(Mathf.Lerp(heights.y, heights.x, blinkBlend));

        public override async Task AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) return;

            // Set the animating in flag to true
            animatingIn = true;

            // Enable the mask
            canvasGroup.alpha = 1f;

            // Set the mask height to the open height
            SetMaskHeight(heights.y);

            // Animate the mask height to the closed height
            if (realTime)
            {
                // Animate the mask height to the closed height
                var tweener = DOTween.To(() => heights.y, x => SetMaskHeight(x), heights.x, GetDuration()).SetEase(easing).SetUpdate(true);

                // Wait for the animation to complete
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Animate the mask height to the closed height
                var tweener = DOTween.To(() => heights.y, x => SetMaskHeight(x), heights.x, GetDuration()).SetEase(easing);

                // Wait for the animation to complete
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

            // Enable the mask
            canvasGroup.alpha = 1f;

            // Set the mask height to the closed height
            SetMaskHeight(heights.x);

            // Animate the mask height to the open height
            if (realTime)
            {
                // Animate the mask height to the open height
                var tweener = DOTween.To(() => heights.x, x => SetMaskHeight(x), heights.y, GetDuration()).SetEase(easing).SetUpdate(true);

                // Wait for the animation to complete
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Animate the mask height to the open height
                var tweener = DOTween.To(() => heights.x, x => SetMaskHeight(x), heights.y, GetDuration()).SetEase(easing);

                // Wait for the animation to complete
                await tweener.AsyncWaitForCompletion();
            }

            // Disable the mask
            canvasGroup.alpha = 0f;

            // Set the animating out flag to false
            animatingOut = false;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }

        public override void SetTransitionState(bool status)
        {
            // Set the mask height based on the transition state
            if (status) SetMaskHeight(heights.x);
            else SetMaskHeight(heights.y);
        }

        public override float GetDuration() => duration;

        private void SetMaskHeight(float height)
        {
            // Set the size delta of the mask to the specified height
            if (mask != null) mask.sizeDelta = new Vector2(mask.sizeDelta.x, height);
        }
    }
}