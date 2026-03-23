using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

namespace WorldShaper
{
    public class ScreenWipe : TransitionAnimation
    {
        [Header("UI Elements")]
        public Image image;
        public float duration = 1f;

        public override async Task AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) return;

            // Set the animating in flag to true
            animatingIn = true;

            // Enable the image
            image.enabled = true;

            // Get the width of the image for the start position
            float width = image.rectTransform.rect.width;

            // Set the anchored position to the start position
            image.rectTransform.anchoredPosition = new Vector2(-width, 0f);

            // Slide the image towards the end position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var tweener = image.rectTransform.DOAnchorPosX(0f, duration).SetUpdate(true);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var tweener = image.rectTransform.DOAnchorPosX(0f, duration);

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

            // Enable the image
            image.enabled = true;

            // Put the image in the start position
            image.rectTransform.anchoredPosition = Vector2.zero;

            // Get the width of the image for the end position
            float width = image.rectTransform.rect.width;

            // Slide the image towards the start position
            if (realTime)
            {
                // Update the position in real time, regardless of the time scale
                var tweener = image.rectTransform.DOAnchorPosX(width, duration).SetUpdate(true);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the position in game time, respecting the time scale
                var tweener = image.rectTransform.DOAnchorPosX(width, duration);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }

            // Disable the image
            image.enabled = false;

            // Set the animating out flag to false
            animatingOut = false;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }

        public override void SetTransitionState(bool status) => image.rectTransform.anchoredPosition = status ? Vector2.zero : new Vector2(-image.rectTransform.rect.width, 0f);

        public override float GetDuration() => duration;
    }
}