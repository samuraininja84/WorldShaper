using System.Threading.Tasks;
using UnityEngine;

namespace WorldShaper
{
    public abstract class BaseLocationPointer : MonoBehaviour, ILocationPointer
    {
        public virtual string Name => gameObject.name;

        public abstract void SetActive(bool status);

        public virtual async Task Initialize() => await Task.CompletedTask;

        public virtual async Task Activate() => await Task.CompletedTask;

        public virtual async Task Enter() => await Task.CompletedTask;

        public virtual async Task Exit() => await Task.CompletedTask;

        public virtual Vector3 GetPosition() => transform.position;

        public abstract string GetEndpoint();
    }
}
