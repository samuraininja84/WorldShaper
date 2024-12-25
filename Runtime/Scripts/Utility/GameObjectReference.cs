using UnityEngine;

namespace WorldShaper
{
    [CreateAssetMenu(fileName = "New Game Object Reference", menuName = "World Shaper/New Game Object Reference")]
    public class GameObjectReference : ScriptableObject
    {
        public GameObject gameObject;
    }
}
