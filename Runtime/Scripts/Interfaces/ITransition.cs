using System.Threading.Tasks;

namespace WorldShaper
{
    public interface ITransition 
    {
        Task AnimateTransitionIn(bool realTime = false);

        Task AnimateTransitionOut(bool realTime = false);
    }
}
