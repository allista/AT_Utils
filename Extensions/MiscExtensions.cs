using UnityEngine;

namespace AT_Utils
{
    public static class MiscExtensions
    {
        public static Color Normalized(this Color c)
        {
            var max = c.r > c.g ? (c.r > c.b ? c.r : c.b) : (c.g > c.b ? c.g : c.b);
            return max.Equals(0) ? c : new Color(c.r / max, c.g / max, c.b / max);
        }

        #region From blizzy's Toolbar
        public static Vector2 clampToScreen(this Vector2 pos)
        {
            pos.x = Mathf.Clamp(pos.x, 0, Screen.width - 1);
            pos.y = Mathf.Clamp(pos.y, 0, Screen.height - 1);
            return pos;
        }

        public static Rect clampToScreen(this Rect rect)
        {
            rect.width = Mathf.Clamp(rect.width, 0, Screen.width);
            rect.height = Mathf.Clamp(rect.height, 0, Screen.height);
            rect.x = Mathf.Clamp(rect.x, 0, Screen.width - rect.width);
            rect.y = Mathf.Clamp(rect.y, 0, Screen.height - rect.height);
            return rect;
        }

        public static Rect clampToWindow(this Rect rect, Rect window)
        {
            rect.width = Mathf.Clamp(rect.width, 0, window.width);
            rect.height = Mathf.Clamp(rect.height, 0, window.height);
            rect.x = Mathf.Clamp(rect.x, 0, window.width - rect.width);
            rect.y = Mathf.Clamp(rect.y, 0, window.height - rect.height);
            return rect;
        }
        #endregion

        #region ConfigNode
        public static void AddRect(this ConfigNode n, string name, Rect r) =>
            n.AddValue(name,
                ConfigNode.WriteQuaternion(new Quaternion(r.x, r.y, r.width, r.height)));

        public static string ToConfigString(this IConfigNode inode)
        {
            if(inode == null)
                return "";
            var node = new ConfigNode(inode.GetID());
            inode.Save(node);
            return node.ToString();
        }

        public static Rect GetRect(this ConfigNode n, string name)
        {
            try
            {
                var q = ConfigNode.ParseQuaternion(n.GetValue(name));
                return new Rect(q.x, q.y, q.z, q.w);
            }
            catch
            {
                return default(Rect);
            }
        }
        #endregion
    }
}
