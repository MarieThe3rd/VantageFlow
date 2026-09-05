using Microsoft.UI.Xaml.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Converters;

/// <summary>Displays "(idea)" only for Commitment.Idea — keeps this display concern out of the model.</summary>
public sealed class CommitmentToIdeaLabelConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is Commitment.Idea ? "(idea)" : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
