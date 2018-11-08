using UnityEngine;
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
        }

        void Start()
        {
            for(int i = 0; i < 15; i++)
                colorList.AddColored(new ColoredTest(i), "Colored Test: " + i);
        }
    }
}
