using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DevTool.Module
{
    // Claim enum type
    public enum ModuleState
    {
        Idle,
        Inactive,
        Active,
        Terminate,
        Run,
        Error,
    }

    public enum StateCommand
    {
        Init,
        Activate,
        Deactivate,
        Begin,
        End,
        Exit,
        Alarm,
        Reset
    }

    public class StateMachineBase
    {
        // Class object for transition check
        class StateTransition
        {
            // Private Field
            readonly ModuleState CurrentState;
            readonly StateCommand Command;

            public StateTransition(ModuleState currentState, StateCommand command)
            {
                CurrentState = currentState;
                Command = command;
            }

            #region Public Override Method

            public override int GetHashCode()
            {
                return 17 + 31 * CurrentState.GetHashCode() + 31 * Command.GetHashCode();
            }

            public override bool Equals(object obj)
            {
                StateTransition other = obj as StateTransition;
                return other != null && this.CurrentState == other.CurrentState && this.Command == other.Command;
            }

            #endregion
        }

        // Private Field
        Dictionary<StateTransition, ModuleState> _transitions;

        // Public Property
        public ModuleState CurrentState { get; private set; }

        public StateMachineBase()
        {
            // Initialize the Property
            CurrentState = ModuleState.Idle;

            // Initialize the StateTransition and define the available state-flow
            _transitions = new Dictionary<StateTransition, ModuleState>
            {
                {new StateTransition(ModuleState.Idle, StateCommand.Init), ModuleState.Inactive },
                {new StateTransition(ModuleState.Inactive, StateCommand.Activate), ModuleState.Active },
                {new StateTransition(ModuleState.Inactive, StateCommand.Alarm), ModuleState.Error },
                {new StateTransition(ModuleState.Inactive, StateCommand.Exit), ModuleState.Terminate },
                {new StateTransition(ModuleState.Active, StateCommand.Deactivate), ModuleState.Inactive },
                {new StateTransition(ModuleState.Active, StateCommand.Begin), ModuleState.Run },
                {new StateTransition(ModuleState.Active, StateCommand.Alarm), ModuleState.Error },
                {new StateTransition(ModuleState.Run, StateCommand.End), ModuleState.Active },
                {new StateTransition(ModuleState.Run, StateCommand.Alarm), ModuleState.Error },
                {new StateTransition(ModuleState.Error, StateCommand.Reset), ModuleState.Inactive },
            };
        }

        #region Public Method

        public void CheckState(ModuleState expectedState)
        {
            if(CurrentState != expectedState)
                throw new Exception(string.Format("CheckState Exception!{0}ExpectedState = {1}, CurrentState = {2}", System.Environment.NewLine, expectedState, CurrentState));
        }

        public ModuleState MoveNext(StateCommand command)
        {
            CurrentState = GetNext(command);
            return CurrentState;
        }

        #endregion

        #region Private Method

        private ModuleState GetNext(StateCommand command)
        {
            StateTransition transition = new StateTransition(CurrentState, command);
            ModuleState nextState;
            if (!_transitions.TryGetValue(transition, out nextState))
                throw new Exception("Invalid transition: " + CurrentState + " -> " + command);
            return nextState;
        }

        #endregion
    }
}
