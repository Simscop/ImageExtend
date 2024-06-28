using System;

namespace Lift.UI.Expression.Media;

[Flags]
internal enum DrawingPropertyMetadataOptions
{
    AffectsMeasure = 1,
    AffectsRender = 0x10,
    None = 0
}
