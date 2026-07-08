using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;

namespace WorldShaper
{
    public sealed class UnityEventBehaviour : MonoBehaviour, IInitializeBehaviour, IActivateBehaviour, IEnterBehaviour, IExitBehaviour
    {
        public UnityEvent onInitialize;
        public UnityEvent onActivate;
        public UnityEvent onEnter;
        public UnityEvent onExit;

        public Task OnInitialize()
        {
            // Invoke the OnInitialize UnityEvent
            onInitialize?.Invoke();

            // Return a completed task
            return Task.CompletedTask;
        }

        public Task OnActivate()
        {
            // Invoke the OnActivate UnityEvent
            onActivate?.Invoke();

            // Return a completed task
            return Task.CompletedTask;
        }

        public Task OnEnter()
        {
            // Invoke the OnEnter UnityEvent
            onEnter?.Invoke();

            // Return a completed task
            return Task.CompletedTask;
        }

        public Task OnExit()
        {
            // Invoke the OnExit UnityEvent
            onExit?.Invoke();

            // Return a completed task
            return Task.CompletedTask;
        }
    }
}
