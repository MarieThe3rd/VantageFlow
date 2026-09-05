using Microsoft.UI.Xaml.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Converters;

/// <summary>Formats an optional Requester for display — absent for self-directed tasks.</summary>
public sealed class RequesterToSummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is Person person ? $"— asked by {person.Name}" : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
