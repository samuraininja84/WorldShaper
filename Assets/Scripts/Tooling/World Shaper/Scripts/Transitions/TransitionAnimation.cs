using System;
using System.Collections;
using UnityEngine;

namespace WorldShaper
{
    public abstract class TransitionAnimation : MonoBehaviour
    {
        public Action OnTransitionIn;
        public Action OnTransitionOut;
        public Action<bool> OnTransition;

        [HideInInspector] public bool animatingIn;
        [HideInInspector] public bool animatingOut;

        public abstract IEnumerator AnimateTransitionIn(bool realTime = false);
        public abstract IEnumerator AnimateTransitionOut(bool realTime = false);
    }
}