using Microsoft.UI.Xaml.Data;
using VantageFlow.Core.Modules.TaskManager.Models;

namespace VantageFlow.Modules.TaskManager.Views.Converters;

/// <summary>Formats an optional Project for display — absent for standalone tasks.</summary>
public sealed class ProjectToSummaryConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, string language) =>
        value is Project project ? $"[{project.Name}]" : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, string language) =>
        throw new NotSupportedException();
}
