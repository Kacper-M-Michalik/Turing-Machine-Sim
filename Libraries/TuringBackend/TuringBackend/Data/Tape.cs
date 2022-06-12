using System;
using System.Collections.Generic;

namespace TuringBackend
{
    public class Tape
    {
        public string ID;
        public string DefenitionAlphabetID;
        Dictionary<int, string> Data = new Dictionary<int, string>();
        public int HighestIndex { get; private set; }
        public int LowestIndex { get; private set; }

        public Tape() {}

        public string this[int Position]
        {
            get
            {
                if (Data.TryGetValue(Position, out string Value))
                {
                    return Value;
                }
                else
                {
                    //Data.Add(Position, Project.ProjectAlphabets[DefenitionAlphabetID].EmptyCharacter);
                    return Data[Position];
                }
            }
            set
            {
                if (Data.ContainsKey(Position))
                {
                    Data[Position] = value;
                }
                else
                {
                    if (Position < LowestIndex) LowestIndex = Position;
                    if (Position > HighestIndex) HighestIndex = Position;
                    Data.Add(Position, value);
                }
            }
        }

        public void SetData(string[] Input)
        {
            Data.Clear();
            for (int i = 0;  i < Input.Length; i++)
            {
                Data.Add(i, Input[i]);
            }
            LowestIndex = 0;
            HighestIndex = Input.Length - 1;
        }

        public int Count()
        {
            return Data.Count;
        }

        public Tape Clone()
        {
            Tape CloneTape = new Tape();
            CloneTape.ID = ID;
            CloneTape.DefenitionAlphabetID = DefenitionAlphabetID;
            CloneTape.Data = new Dictionary<int, string>(Data);
            CloneTape.HighestIndex = HighestIndex;
            CloneTape.LowestIndex = LowestIndex;
            return CloneTape;
        }
    }
}
