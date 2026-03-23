using System.Threading.Tasks;
using UnityEngine;
using DG.Tweening;

namespace WorldShaper
{
    [System.Serializable]
    public class CrossFade : TransitionAnimation
    {
        [Header("UI Elements")]
        public CanvasGroup transitionCanvasGroup;
        public float duration = 1f;

        public override async Task AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) return;

            // Set the animating in flag to true
            animatingIn = true;

            // Set the alpha to 0
            transitionCanvasGroup.alpha = 0f;

            // Fade the image towards 1
            if (realTime)
            {
                // Update the alpha in real time, regardless of the time scale
                var tweener = transitionCanvasGroup.DOFade(1f, duration).SetUpdate(true);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the alpha in game time, respecting the time scale
                var tweener = transitionCanvasGroup.DOFade(1f, duration);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }

            // Set the animating in flag to false
            animatingIn = false;

            // Invoke the transition in event
            OnTransitionIn?.Invoke();

            // Return a completed task
            await Task.CompletedTask;
        }

        public override async Task AnimateTransitionOut(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingOut) return;

            // Set the animating out flag to true
            animatingOut = true;

            // Set the alpha to 1
            transitionCanvasGroup.alpha = 1f;

            // Fade the image towards 0
            if (realTime)
            {
                // Update the alpha in real time, regardless of the time scale
                var tweener = transitionCanvasGroup.DOFade(0f, duration).SetUpdate(true);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }
            else
            {
                // Update the alpha in game time, respecting the time scale
                var tweener = transitionCanvasGroup.DOFade(0f, duration);

                // Await the completion of the tween
                await tweener.AsyncWaitForCompletion();
            }

            // Set the animating out flag to false
            animatingOut = false;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }

        public override void SetTransitionState(bool status) => transitionCanvasGroup.alpha = status ? 1f : 0f;

        public override float GetDuration() => duration;
    }
}
