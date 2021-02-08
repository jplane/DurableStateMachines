using DSM.FunctionClient;
using DSM.Metadata.Actions;
using DSM.Metadata.States;

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
                                    To = d => d.X,
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
                                    To = d => d.X,
                                    ValueFunction = data => data.X + 1
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<TestState>
                            {
                                Target = "alldone"
                            }
                        }
                    },
                    new FinalState<TestState>
                    {
                        Id = "alldone"
                    }
                }
            };

        [StateMachineDefinition("tupletest")]
        public StateMachine<(int x, int y)> TupleTest =>
            new StateMachine<(int x, int y)>
            {
                Id = "test",
                States =
                {
                    new AtomicState<(int x, int y)>
                    {
                        Id = "state1",
                        OnEntry = new OnEntryExit<(int x, int y)>
                        {
                            Actions =
                            {
                                new Assign<(int x, int y)>
                                {
                                    To = d => d.x,
                                    ValueFunction = data => data.x + 1
                                }
}
                        },
                        OnExit = new OnEntryExit<(int x, int y)>
                        {
                            Actions =
                            {
                                new Assign<(int x, int y)>
                                {
                                    To = d => d.x,
                                    ValueFunction = data => data.x + 1
                                }
                            }
                        },
                        Transitions =
                        {
                            new Transition<(int x, int y)>
                            {
                                Target = "alldone"
                            }
                        }
                    },
                    new FinalState<(int x, int y)>
                    {
                        Id = "alldone"
                    }
                }
            };
    }
}
