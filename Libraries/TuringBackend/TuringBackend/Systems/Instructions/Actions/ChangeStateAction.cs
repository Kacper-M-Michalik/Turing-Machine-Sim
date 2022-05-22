using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Actions
{
    public class ChangeStateAction : TuringAction
    {
        public string NewState;
        public override void Execute(TuringMachine Machine)
        {
            Machine.CurrentState = NewState;
        }
    }
}
