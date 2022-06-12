using System;
using System.Collections.Generic;

namespace TuringBackend
{
    public class StateTable
    {
        public string ID;
        public string DefenitionAlphabetID;
        Dictionary<string, Instruction> Instructions = new Dictionary<string, Instruction>();
        HashSet<string> HaltStates = new HashSet<string>();

        public Instruction this[string State]
        {
            get
            {
                if (Instructions.TryGetValue(State, out Instruction InstructionToReturn))
                {
                    return InstructionToReturn;
                }
                else
                {
                    throw new Exception("Exception! Statetable doesn't contain a instruction for this state" + State.ToString());
                }
            }
        }

        public void AddInstruction(string TriggerState, Instruction NewInstruction)
        {
            Instructions.Add(TriggerState, NewInstruction);
        }

        public void AddHaltState(string State)
        {
            HaltStates.Add(State);
        }

        public bool ContainsInstructionForState(string State)
        {
            return Instructions.ContainsKey(State);
        }

        public bool IsHaltState(string State)
        {
            return HaltStates.Contains(State);
        }
    }
}
