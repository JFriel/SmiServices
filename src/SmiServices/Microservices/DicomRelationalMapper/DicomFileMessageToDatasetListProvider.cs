using FellowOakDicom;
using Rdmp.Dicom.PipelineComponents.DicomSources.Worklists;
using System.Collections.Generic;
using System.Linq;

namespace SmiServices.Microservices.DicomRelationalMapper;

public class DicomFileMessageToDatasetListWorklist : IDicomDatasetWorklist,IDicomWorklist
{
    private readonly List<QueuedImage> _messages;
    private int _progress;

    public HashSet<QueuedImage> CorruptMessages = [];

    public DicomFileMessageToDatasetListWorklist(List<QueuedImage> messages)
    {
        _messages = messages;
    }

    /// <summary>
    /// Resets the progress through the work list e.g. if half the list is consumed and you want to
    /// start again.
    /// </summary>
    public void ResetProgress()
    {
        _progress = 0;
    }

    public DicomDataset? GetNextDatasetToProcess(out string? filename, out Dictionary<string, string> otherValuesToStoreInRow)
    {
        otherValuesToStoreInRow = [];

        if (_progress >= _messages.Count)
        {
            filename = null;
            return null;
        }

        QueuedImage toReturn = _messages[_progress];
        filename = toReturn.DicomFileMessage.DicomFilePath;

        otherValuesToStoreInRow.Add("MessageGuid", _messages[_progress].Header.MessageGuid.ToString());
        otherValuesToStoreInRow.Add("DicomFileSize", toReturn.DicomFileMessage.DicomFileSize.ToString()); //TN: It won't be a string when it hits the database but the API supports only string/string for this out Dictionary

        _progress++;

        return toReturn.DicomDataset;
    }

    public void MarkCorrupt(DicomDataset ds)
    {
        CorruptMessages.Add(_messages.Single(m => m.DicomDataset == ds));
    }
}
