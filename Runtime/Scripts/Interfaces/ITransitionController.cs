using System.Threading.Tasks;

namespace WorldShaper
{
    public interface ITransitionController
    {
        Task AnimateTransitionIn(bool realTime = false);
        Task AnimateTransitionOut(bool realTime = false);
        void SetInTransition(TransitionAnimation transition);
        void SetOutTransition(TransitionAnimation transition);
        void SetInTransition(TransitionIdentifier transitionId);
        void SetOutTransition(TransitionIdentifier transitionId);
    }
}
