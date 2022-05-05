using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend.Actions
{
    public class WriteAction : Action
    {
        public string WriteValue;
        public override void Execute(TuringMachine Machine)
        {
            Machine.ActiveTape[Machine.HeadPosition] = WriteValue;
        }
    }
}
