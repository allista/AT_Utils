using UnityEngine;

namespace AT_Utils
{
    public interface ISpaceUser
    {
        bool IsSpaceUsed(string spaceName);
        bool IsSpaceUsed(GameObject go);
    }
}
