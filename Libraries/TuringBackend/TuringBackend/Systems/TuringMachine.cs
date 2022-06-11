using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace TuringBackend
{
    public class TuringMachine
    {
        const string HaltError = "HALT-ERROR";

        public bool IsActive { get; private set; } = false;
        public string CurrentState { get; set; } = "Null";
        public int HeadPosition { get; set; } = 0;
        public InstructionVariant NextInstruction { get; private set; } = null;

        public StateTable ActiveStateTable { get; private set; }
        public Tape ActiveTape { get; private set; }
        Dictionary<string, StateTable> StateTableMemory = new Dictionary<string, StateTable>();
        Dictionary<string, Tape> TapeMemory = new Dictionary<string, Tape>();

        public void Start(string NewTable, string StartState, string NewTape, int StartHeadPosition)
        {
            if (IsActive) throw new Exception("Machine already on");
            ShallowClear();

            IsActive = true;
            CurrentState = StartState;
            HeadPosition = StartHeadPosition;

            SetActiveStateTable(NewTable);
            SetActiveTape(NewTape);

            if (ActiveStateTable.DefenitionAlphabetID != ActiveTape.DefenitionAlphabetID)
            {
                CurrentState = HaltError;
                NextInstruction = null;
            }
            else
            {
                NextInstruction = ActiveStateTable[CurrentState][ActiveTape[HeadPosition]];
            }

        }
        public void Start(string StartState, int StartHeadPosition)
        {
            if (IsActive) throw new Exception("Machine already on");
            string LoadedTableID = ActiveStateTable.ID;
            string LoadedTapeID = ActiveTape.ID;
            ShallowClear();

            IsActive = true;
            CurrentState = StartState;
            HeadPosition = StartHeadPosition;

            SetActiveStateTable(LoadedTableID);
            SetActiveTape(LoadedTapeID);

            if (ActiveStateTable.DefenitionAlphabetID != ActiveTape.DefenitionAlphabetID)
            {
                CurrentState = HaltError;
                NextInstruction = null;
            }
            else
            {
                NextInstruction = ActiveStateTable[CurrentState][ActiveTape[HeadPosition]];
            }

        }

        public void StepProgram()
        {
            if (!IsActive) return;

            if (CurrentState == HaltError || ActiveStateTable.IsHaltState(CurrentState))
            {
                IsActive = false;
                return;
            }

            for (int i = 0; i < NextInstruction.Actions.Count; i++)
            {
                NextInstruction.Actions[i].Execute(this);
            }

            if (ActiveStateTable.IsHaltState(CurrentState))
            {
                NextInstruction = null;
            }
            else if (ActiveStateTable.DefenitionAlphabetID == ActiveTape.DefenitionAlphabetID && ActiveStateTable.ContainsInstructionForState(CurrentState))
            {
                if (ActiveStateTable[CurrentState].ContainsVariant(ActiveTape[HeadPosition]))
                {
                    NextInstruction = ActiveStateTable[CurrentState][ActiveTape[HeadPosition]];
                }
               // else if (ActiveStateTable[CurrentState].ContainsVariant(Project.ProjectAlphabets[ActiveStateTable.DefenitionAlphabetID].WildcardCharacter))
               // {
               //     NextInstruction = ActiveStateTable[CurrentState][Project.ProjectAlphabets[ActiveStateTable.DefenitionAlphabetID].WildcardCharacter];
               // }
                else
                {
                    CurrentState = HaltError;
                }
            }
            else
            {
                CurrentState = HaltError;
            }
        }

        public void AddStateTableToMemory(StateTable NewStateTable)
        {
            StateTableMemory.Add(NewStateTable.ID, NewStateTable);
        }

        public void AddTapeToMemory(Tape NewTape)
        {
            TapeMemory.Add(NewTape.ID, NewTape);
        }

        public void SetActiveTape(string ID)
        {
            ActiveTape = TapeMemory[ID].Clone();
        }

        public void SetActiveStateTable(string ID)
        {
            ActiveStateTable = StateTableMemory[ID];
        }

        public void ShallowClear()
        {
            IsActive = false;
            CurrentState = "Null";
            HeadPosition = 0;
            NextInstruction  = null;
            ActiveStateTable = null;
            ActiveTape = null;
        }
    }
}
