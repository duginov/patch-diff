using Microsoft.Extensions.Options;
using Patch;
#pragma warning disable CA1860
#pragma warning disable IDE0290
namespace DiffToolApp
{
    internal class App
    {
        private readonly IPatchLoader _loader;
        private readonly IDiffDetector _diffDetector;
        private readonly ISummaryPresenter _presenter;

        public App(IPatchLoader loader, IDiffDetector diffDetector, ISummaryPresenter presenter)
        {
            _loader = loader;
            _diffDetector = diffDetector;
            _presenter = presenter;
        }
        public void Run(IOptions<AppOptions> inputs)
        {
            var oldFile = inputs.Value.AbsolutePathLeft;
            var newFile = inputs.Value.AbsolutePathRight;

            var oldPatch = _loader.Load(oldFile);
            var newPatch = _loader.Load(newFile);

            if (oldPatch.KeyName != newPatch.KeyName)
            {
                throw new ArgumentException($"Inconsistent key names:\r\n{oldPatch.KeyName} in {oldFile}\r\n{newPatch.KeyName} in {newFile}");
            }

            var removedFields = _diffDetector.DetectRemovedFields(oldPatch, newPatch);
            oldPatch.RemoveFields(removedFields);
            var diffSummary = new DiffSummary(newPatch.KeyName!, oldFile, newFile)
            {
                RemovedFields = removedFields
            };

            // for comparison purposes, we will add new fields to the old patch (with null values), so both patches are structurally the same
            var addedFieldNames = _diffDetector.DetectAddedColumns(oldPatch, newPatch);
            if (addedFieldNames.Any())
            {
                oldPatch.AddFields(addedFieldNames);
            }

            oldPatch.ConsolidateSameKeyLines();
            newPatch.ConsolidateSameKeyLines();

            // at this point all lines have unique keys and same set of fields

            diffSummary.RemovedLines = _diffDetector.DetectUniqueLines(oldPatch.Lines, newPatch.Lines);
            diffSummary.NewLines = _diffDetector.DetectUniqueLines(newPatch.Lines, oldPatch.Lines);
            diffSummary.ChangedLines = _diffDetector.DetectChangesInLines(oldPatch.Lines, newPatch.Lines);

            _presenter.Present(diffSummary);
        }
    }
}
