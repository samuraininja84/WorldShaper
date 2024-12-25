using System.Collections;
using UnityEngine;
using DG.Tweening;

namespace WorldShaper
{
    [System.Serializable]
    public class CrossFade : TransitionAnimation
    {
        [Header("UI Elements")]
        public CanvasGroup crossFade;
        public float duration = 1f;

        public override IEnumerator AnimateTransitionIn(bool realTime = false)
        {
            // If the animation is already running, exit early
            if (animatingIn) yield break;

            // Set the animating in flag to true
            animatingIn = true;

            // Set the alpha to 0
            crossFade.alpha = 0f;

            // Fade the image towards 1
            if (realTime)
            {
                // Update the alpha in real time, regardless of the time scale
                var tweener = crossFade.DOFade(1f, duration).SetUpdate(true);
                yield return new WaitForSecondsRealtime(tweener.Duration());
            }
            else
            {
                // Update the alpha in game time, respecting the time scale
                var tweener = crossFade.DOFade(1f, duration);
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

            // Set the alpha to 1
            crossFade.alpha = 1f;

            // Fade the image towards 0
            if (realTime)
            {
                // Update the alpha in real time, regardless of the time scale
                var tweener = crossFade.DOFade(0f, duration).SetUpdate(true);
                yield return new WaitForSecondsRealtime(tweener.Duration());
            }
            else
            {
                // Update the alpha in game time, respecting the time scale
                var tweener = crossFade.DOFade(0f, duration);
                yield return tweener.WaitForCompletion();
            }

            // Set the animating out flag to false
            animatingOut = false;

            // Invoke the transition out event
            OnTransitionOut?.Invoke();
        }

        public override float GetDuration()
        {
            return duration;
        }
    }
}
