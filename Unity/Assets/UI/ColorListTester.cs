using UnityEngine;
using UnityEngine.Events;

namespace AT_Utils.UI
{
    public class ColorListTester : MonoBehaviour
    {
        public ColorList colorList;

        public class ColoredTest : IColored
        {
            public Color color { get; set; }

            public ColoredTest(int i)
            {
                color = Color.white;
            }

            public void addOnColorChangeListner(UnityAction<Color> action)
            {
            }

            public void removeOnColorChangeListner(UnityAction<Color> action)
            {
            }
        }

        void Start()
        {
            for(int i = 0; i < 15; i++)
                colorList.AddColored(new ColoredTest(i), "Colored Test: " + i);
        }
    }
}
