using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Actions
{
    public class SwapStateTableAction : TuringAction
    {
        public string NewStateTableID;
        public override void Execute(TuringMachine Machine)
        {
            Machine.SetActiveStateTable(NewStateTableID);
        }
    }
}
