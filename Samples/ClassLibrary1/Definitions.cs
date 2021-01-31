using StateChartsDotNet.DurableFunctionClient;
using StateChartsDotNet.Metadata.Execution;
using StateChartsDotNet.Metadata.States;

namespace ClassLibrary1
{
    public class TestState
    {
        public int X { get; set; }
    }

    public class Definitions
    {
        [StateMachineDefinition("test")]
        public StateMachine<TestState> Test =>
            new StateMachine<TestState>
            {
                Id = "test",
                States =
                {
                    new AtomicState<TestState>
                    {
                        Id = "state1",
                        OnEntry = new OnEntryExit<TestState>
                        {
                            Actions =
                            {
                                new Assign<TestState>
                                {
                                    Location = "X", 
                                    ValueFunction = data => data.X + 1
                                }
}
                        },
                        OnExit = new OnEntryExit<TestState>
                        {
                            Actions =
                            {
                                new Assign<TestState>
                                {
                                    Location = "X", 
                                    ValueFunction = data => data.X + 1
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<TestState>
                            {
                                Targets = { "alldone" }
                            }
                        }
                    },
                    new FinalState<TestState>
                    {
                        Id = "alldone"
                    }
                }
            };
    }
}
