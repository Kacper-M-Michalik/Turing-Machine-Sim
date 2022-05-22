using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend
{
    public abstract class TuringAction
    {
        public abstract void Execute(TuringMachine Machine);
    }
}
