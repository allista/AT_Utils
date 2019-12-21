using System.Linq;

namespace AT_Utils
{
    public enum AnimatorState
    {
        Closed,
        Closing,
        Opened,
        Opening,
    }

    public interface IAnimator
    {
        string GetAnimatorID();
        AnimatorState GetAnimatorState();
        void Open();
        void Close();
    }

    public static class AnimatorExtensions
    {
        static bool animator_matches(IAnimator a, string ID)
        {
            var a_ID = a.GetAnimatorID();
            return !string.IsNullOrEmpty(a_ID) && a_ID.Equals(ID);
        }

        public static IAnimator GetAnimator(this Part p, string ID) =>
            p.Modules.GetModules<IAnimator>().FirstOrDefault(m => animator_matches(m, ID));

        public static T GetAnimator<T>(this Part p, string ID) where T : PartModule, IAnimator =>
            p.Modules.GetModules<T>().FirstOrDefault(m => animator_matches(m, ID));
    }
}
