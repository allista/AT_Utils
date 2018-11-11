using UnityEngine;
namespace AT_Utils.UI
{
    public interface IColored
    {
        Color color { get; set; }
    }

    public interface IColorProvider
    {
        IColored GetColored(string name);
    }
}
