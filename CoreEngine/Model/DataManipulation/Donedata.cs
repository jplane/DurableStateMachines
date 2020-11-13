using System.Linq;
using Nito.AsyncEx;
using StateChartsDotNet.CoreEngine.Abstractions.Model.DataManipulation;

namespace StateChartsDotNet.CoreEngine.Model.DataManipulation
{
    internal class Donedata
    {
        private readonly AsyncLazy<Content> _content;
        private readonly AsyncLazy<Param[]> _params;

        public Donedata(IDonedataMetadata metadata)
        {
            metadata.CheckArgNull(nameof(metadata));

            _content = new AsyncLazy<Content>(async () =>
            {
                var meta = await metadata.GetContent();

                if (meta != null)
                    return new Content(meta);
                else
                    return null;
            });

            _params = new AsyncLazy<Param[]>(async () =>
            {
                return (await metadata.GetParams()).Select(pm => new Param(pm)).ToArray();
            });
        }
    }
}
