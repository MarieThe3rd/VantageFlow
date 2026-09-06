using Microsoft.UI.Xaml.Data;

namespace VantageFlow.Modules.TaskManager.Views.Converters;

/// <summary>Formats an optional due date for display — absent when there's no deadline.</summary>
public sealed class DueDateToSummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is DateOnly date ? $"due {date:d}" : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
