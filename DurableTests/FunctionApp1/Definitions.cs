using DSM.FunctionClient;
using DSM.Metadata.Actions;
using DSM.Metadata.States;

namespace FunctionApp1
{
    public class Definitions
    {
        [StateMachineDefinition("test1")]
        public StateMachine<(int x, int y)> Test1 =>
            new StateMachine<(int x, int y)>
            {
                Id = "test1",
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
