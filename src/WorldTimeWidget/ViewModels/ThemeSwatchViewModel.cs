using System.Windows.Media;
using WorldTimeWidget.Models;

namespace WorldTimeWidget.ViewModels;

/// <summary>
/// Один сваш в сетке пикера тем (меню «⋯», см. spec.md «UI переключения темы»): заливка —
/// непрозрачный surface-цвет темы (чтобы сваш был узнаваем сам по себе, независимо от текущей
/// прозрачности активной темы), тонкая обводка — её border-цвет (с альфой темы).
/// </summary>
public sealed class ThemeSwatchViewModel : ViewModelBase
{
    public ThemeSwatchViewModel(ThemeDefinition definition)
    {
        Id = definition.Id;
        DisplayName = definition.DisplayName;
        FillBrush = new SolidColorBrush(definition.Surface);
        BorderBrush = new SolidColorBrush(definition.Border);
    }

    public string Id { get; }

    public string DisplayName { get; }

    public Brush FillBrush { get; }

    public Brush BorderBrush { get; }

    private bool _isActive;

    /// <summary>true — эта тема сейчас активна: сваш получает акцентное кольцо-индикатор (2px) вокруг себя.</summary>
    public bool IsActive
    {
        get => _isActive;
        set => SetField(ref _isActive, value);
    }
}
