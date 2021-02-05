using Newtonsoft.Json;
using System.Linq;

namespace DSM.Common.Observability
{
    internal static class InstructionExtensions
    {
        public static bool IsMatch(this Instruction[] instructions, ObservableAction action, string element)
        {
            if (instructions == null)
            {
                return false;
            }
            else
            {
                return instructions.Any(di => di.IsMatch(action, element));
            }
        }
    }
}
