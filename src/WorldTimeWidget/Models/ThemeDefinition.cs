using System.Windows.Media;

namespace WorldTimeWidget.Models;

/// <summary>
/// Полный набор визуальных токенов одной темы оформления виджета (см. spec.md, «Доработка v1.3»,
/// раздел «Токены тем» — значения зафиксированы и утверждены в Penpot, не менять без сверки с дизайном).
///
/// Цвета с альфа-каналом (<see cref="Border"/>, <see cref="HomeHighlight"/>, <see cref="Divider"/>)
/// хранятся уже с «вшитой» альфой. Исключение — <see cref="Surface"/>: это базовый RGB без альфы,
/// т.к. итоговая прозрачность карточки регулируется отдельно (слайдер «Прозрачность», своё значение
/// на каждую тему — см. <c>AppSettings.BackgroundOpacityByTheme</c>), а прозрачность попапов —
/// через <see cref="PopupOpacityPercent"/>.
/// </summary>
public sealed class ThemeDefinition
{
    public required string Id { get; init; }

    public required string DisplayName { get; init; }

    /// <summary>Базовый цвет поверхности карточки/попапов (без альфа-канала).</summary>
    public required Color Surface { get; init; }

    /// <summary>
    /// Прозрачность карточки по умолчанию, % — применяется, пока пользователь не задал своё
    /// значение слайдером для этой конкретной темы (см. <c>AppSettings.BackgroundOpacityByTheme</c>).
    /// </summary>
    public required int DefaultOpacityPercent { get; init; }

    /// <summary>
    /// false только для Minimal Flat: прозрачность там принципиально зафиксирована на 100%
    /// (плоский дизайн непрозрачен), слайдер «Прозрачность» в меню задизейблен.
    /// </summary>
    public bool AllowsOpacityAdjustment { get; init; } = true;

    /// <summary>Прозрачность попапов (добавление города, меню «⋯»), %. Слайдером не регулируется — фиксированное значение темы.</summary>
    public required int PopupOpacityPercent { get; init; }

    /// <summary>Цвет обводки карточки/попапов (уже с альфа-каналом).</summary>
    public required Color Border { get; init; }

    /// <summary>Толщина обводки, px (для верхней и боковых граней — см. ThemeViewModel.Apply).</summary>
    public required double BorderThickness { get; init; }

    /// <summary>Радиус скругления карточки/попапов, px.</summary>
    public required double CornerRadius { get; init; }

    /// <summary>Заливка подсветки домашней строки (уже с альфа-каналом).</summary>
    public required Color HomeHighlight { get; init; }

    public required Color TextPrimary { get; init; }

    public required Color TextSecondary { get; init; }

    /// <summary>Цвет разделителя между пунктами меню (уже с альфа-каналом).</summary>
    public required Color Divider { get; init; }

    /// <summary>
    /// null — тени нет вообще (Minimal Flat: <c>shadow: null</c> в токенах, не просто прозрачная тень).
    /// Иначе — цвет тени/свечения: не всегда чёрный, см. AMOLED Dark (<c>#00E5FF</c>).
    /// </summary>
    public Color? ShadowColor { get; init; }

    public double ShadowBlurRadius { get; init; }

    public double ShadowOffsetY { get; init; }

    public double ShadowOpacity { get; init; }

    /// <summary>true — только Rainmeter Enigma-style: название города рисуется начертанием Regular вместо Medium.</summary>
    public bool IsCityNameRegular { get; init; }

    /// <summary>true — только Liquid Glass: показывать глянцевый блик поверх карточки.</summary>
    public bool HasGlossHighlight { get; init; }
}
