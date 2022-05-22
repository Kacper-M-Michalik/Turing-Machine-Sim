using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Actions
{
    public class SwapTapeAction : TuringAction
    {
        public string NewTapeID;
        public override void Execute(TuringMachine Machine)
        {
            Machine.SetActiveTape(NewTapeID);
        }
    }
}
