using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend
{
    public class Instruction
    {
        Dictionary<string, InstructionVariant> InstructionVariants = new Dictionary<string, InstructionVariant>();

        public InstructionVariant this[string ReadAlphabetCharacter]
        {
            get
            {
                if (InstructionVariants.TryGetValue(ReadAlphabetCharacter, out InstructionVariant Variant))
                {
                    return Variant;
                }
                else
                {
                    throw new Exception("Exception! No variant implentation for current character! Index: " + ReadAlphabetCharacter);
                }
            }
        }

        public void AddVariant(string TriggerState, InstructionVariant NewVariant)
        {
            InstructionVariants.Add(TriggerState, NewVariant);
        }

        public bool ContainsVariant(string ReadAlphabetCharacter)
        {
            return InstructionVariants.ContainsKey(ReadAlphabetCharacter);
        }
    }
}
