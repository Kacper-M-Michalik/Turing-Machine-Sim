using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Actions
{
    public class MoveHeadAction : Action
    {
        public int MoveValue;
        public override void Execute(TuringMachine Machine)
        {
            Machine.HeadPosition += MoveValue;
        }
    }
}
