using System.Threading.Tasks;

namespace WorldShaper
{
    public interface ITransitionController
    {
        Task AnimateTransitionIn(bool realTime = false);

        Task AnimateTransitionOut(bool realTime = false);

        void SetInTransition(TransitionIdentifier transitionId);

        void SetOutTransition(TransitionIdentifier transitionId);
    }
}
