//   ProgressIndicator.cs
//
//  Author:
//       Allis Tauri <allista@gmail.com>
//
//  Copyright (c) 2018 Allis Tauri
using System.Collections.Generic;

namespace AT_Utils
{
    public static class ProgressIndicator
    {
        static IEnumerator<char> indicator_seq()
        {
            const string clock = "◐◓◑◒";
            var clen = clock.Length;
            var timer = new RealTimer(0.125);
            var i = 0;
            while(true)
            {
                yield return clock[i];
                if(timer.TimePassed)
                {
                    i = (i + 1) % clen;
                    timer.Restart();
                }
            }
        }

        static IEnumerator<char> _indicator;
        public static char Get
        {
            get 
            { 
                if(_indicator == null)
                    _indicator = indicator_seq();
                _indicator.MoveNext();
                return _indicator.Current;
            }
        }
    }
}
