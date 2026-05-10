using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace WorldShaper
{
    public class UnityEventBehaviour : MonoBehaviour, IBehaviour
    {
        public UnityEvent onActivate;
        public UnityEvent onEnter;
        public UnityEvent onExit;

        public virtual Task OnActivate()
        {
            // Invoke the OnActivate UnityEvent
            onActivate?.Invoke();

            // Return a completed task
            return Task.CompletedTask;
        }

        public virtual Task OnEnter()
        {
            // Invoke the OnEnter UnityEvent
            onEnter?.Invoke();

            // Return a completed task
            return Task.CompletedTask;
        }

        public virtual Task OnExit()
        {
            // Invoke the OnExit UnityEvent
            onExit?.Invoke();

            // Return a completed task
            return Task.CompletedTask;
        }
    }
}
