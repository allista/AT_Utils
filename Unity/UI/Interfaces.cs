using UnityEngine;
using UnityEngine.Events;

namespace AT_Utils.UI
{
    public interface IColored
    {
        Color color { get; set; }

        void addOnColorChangeListner(UnityAction<Color> action);
        void removeOnColorChangeListner(UnityAction<Color> action);
    }

    public interface IColorProvider
    {
        IColored GetColored(string name);
    }
}
