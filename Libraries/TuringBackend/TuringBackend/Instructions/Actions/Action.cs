using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend
{
    public abstract class Action
    {
        public abstract void Execute(TuringMachine Machine);
    }
}
