using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rage;

namespace PerfectDay
{
    class BlackoutScenario
    {
        public void Start()
        {
            GameFiber.StartNew(() => {
                GameFiber.Sleep(5000);
                Rage.Native.NativeFunction.CallByHash<uint>(0x1268615ACE24D504, true);
                GameFiber.Sleep(150);
                Rage.Native.NativeFunction.CallByHash<uint>(0x1268615ACE24D504, false);
                GameFiber.Sleep(150);
                Rage.Native.NativeFunction.CallByHash<uint>(0x1268615ACE24D504, true);
            });
        }
    }
}
