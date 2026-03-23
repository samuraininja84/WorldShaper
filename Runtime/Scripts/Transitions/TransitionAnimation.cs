using System;
using System.Threading.Tasks;
using UnityEngine;

namespace WorldShaper
{
    public abstract class TransitionAnimation : MonoBehaviour, ITransition
    {
        [Header("Transition Identifier")]
        [SerializeField] protected TransitionIdentifier identifier;
        [HideInInspector] public bool animatingIn;
        [HideInInspector] public bool animatingOut;

        public Action OnTransitionIn;
        public Action OnTransitionOut;
        public Action<bool> OnTransition;

        public virtual async Task AnimateTransitionIn(bool realTime = false) { await Task.CompletedTask; }

        public virtual async Task AnimateTransitionOut(bool realTime = false) { await Task.CompletedTask; }

        public abstract void SetTransitionState(bool status);

        public abstract float GetDuration();

        public virtual TransitionIdentifier GetIdentifier() => identifier;
    }
}