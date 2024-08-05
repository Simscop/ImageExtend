using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Shapes;
//using Lift.UI.Tools.Extension;
using Test.ImageExtend.Extension;
using InkCanvas = System.Windows.Controls.InkCanvas;

namespace Test.ImageExtend.ImageEx.ShapeEx;

// todo 线宽改成显示像素，而不是实际宽度
// todo，初始化多了一个点的绘制
//todo, clear&draw的重写

public abstract class ShapeBase : Shape
{
    /// <summary>
    /// 任意图像的起始点
    /// </summary>
    public Point PointStart { get; set; }

    /// <summary>
    /// 任意图形的终止点
    /// </summary>
    public Point PointEnd { get; set; }

    /// <summary>
    /// 在指定的 InkCanvas 上绘制形状
    /// </summary>
    public abstract void Draw(InkCanvas canvas);

    /// <summary>
    ///清除画板上的形状
    /// </summary>
    public virtual void Clear(InkCanvas canvas) => canvas.Children.Remove(this);

    /// <summary>
    /// 是否被选中
    /// </summary>
    public bool IsSelected { get; protected set; } = true;

    /// <summary>
    /// 切换选中状态
    /// 触发OnSelectedChanged事件
    /// </summary>
    public virtual void SetSelected(InkCanvas? canvas)
    {
        if (canvas is null) return;

        foreach (var obj in canvas.Children)
        {
            if (obj is not ShapeBase shape) continue;

            shape.IsSelected = !shape.IsSelected;
            RefreshStrokeThickness();

            OnSelectedChanged?.Invoke(this);
        }
    }

    /// <summary>
    /// 切换选中状态
    /// </summary>
    public virtual void SetSelected() => SetSelected(Parent as InkCanvas);

    /// <summary>
    /// 当选中后
    /// </summary>
    public event Action<ShapeBase>? OnSelectedChanged;

    /// <summary>
    /// 常规状态宽度
    /// </summary>
    public double ThicknessNormal { get; set; } = 1;

    /// <summary>
    /// 鼠标放上去后线宽
    /// </summary>
    public double ThicknessMouseOver { get; set; } = 1;

    /// <summary>
    /// 选择后线宽
    /// </summary>
    public double ThicknessSelected { get; set; } = 1;

    /// <summary>
    /// 鼠标放上去和选择后线宽
    /// </summary>
    public double ThicknessMouseOverAndSelected { get; set; } = 1;

    /// <summary>
    /// 构造函数
    /// </summary>
    protected ShapeBase() : base()
        => InitComponent();

    /// <summary>
    /// initialize the components
    /// </summary>
    public void InitComponent()
    {
        //ThicknessNormal = StrokeThickness;

        //MouseEnter += (_, _) => RefreshStrokeThickness();
        //MouseLeave += (_, _) => RefreshStrokeThickness();
        //MouseDown += (_, e) =>
        //{
        //if (e.LeftButton == MouseButtonState.Pressed)
        //    SetSelected();
        //};

    }

    /// <summary>
    /// 刷新形状的尺寸和位置
    /// </summary>
    public virtual void Refresh()
    {
        var start = PointStart;
        var end = PointEnd;
        Width = Math.Abs(start.X - end.X);
        Height = Math.Abs(start.Y - end.Y);
        var position = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
        InkCanvas.SetLeft(this, position.X);
        InkCanvas.SetTop(this, position.Y);
    }

    /// <summary>
    /// 刷新形状的线条宽度和颜色
    /// </summary>
    protected virtual void RefreshStrokeThickness()
    {
        Fill = IsSelected
            ? new SolidColorBrush() { Color = ((SolidColorBrush)Fill).Color, Opacity = 0.5 }
            : new SolidColorBrush() { Color = ((SolidColorBrush)Fill).Color, Opacity = 0.2 };

        Stroke = IsSelected
            ? new SolidColorBrush() { Color = ((SolidColorBrush)Stroke).Color, Opacity = 0.5 }
            : new SolidColorBrush() { Color = ((SolidColorBrush)Stroke).Color, Opacity = 0.2 };

        StrokeThickness = IsSelected
            ? IsMouseOver ? ThicknessMouseOverAndSelected : ThicknessSelected
            : IsMouseOver ? ThicknessMouseOver : ThicknessNormal;

    }

    /// <summary>
    /// 抽象方法—克隆形状
    /// </summary>
    /// <returns></returns>
    internal abstract ShapeBase Clone();

    /// <summary>
    /// 抽象属性—矩形的几何形状 
    /// </summary>
    protected override Geometry DefiningGeometry
        => throw new NotImplementedException();

}

public class RectangleShape : ShapeBase
{
    protected override Geometry DefiningGeometry => new RectangleGeometry { Rect = new Rect(new Point(0, 0), new Size(Width, Height)) };

    public override void Draw(InkCanvas canvas)
    {
        throw new NotImplementedException();
    }

    internal override RectangleShape Clone()
    {
        throw new NotImplementedException();

        //var clone = new RectangleShape();
        //typeof(RectangleShape).GetProperties()
        //    ?.Where(prop => prop.CanWrite)
        //    ?.Where(prop => new List<string>()
        //    {
        //        "Width","Height","PointStart","PointEnd","Fill","Strokex"
        //    }.Contains(prop.Name))
        //    ?.Do(prop => prop.SetValue(clone, prop.GetValue(this)));

        //return clone;
    }
}

public class LineShape : ShapeBase
{
    private LineGeometry lineGeometry;

    public LineShape()
    {
        lineGeometry = new LineGeometry();
    }

    protected override Geometry DefiningGeometry => lineGeometry;

    public override void Refresh()
    {
        var start = PointStart;
        var end = PointEnd;
        lineGeometry.StartPoint = start;
        lineGeometry.EndPoint = end;
        InkCanvas.SetLeft(this, (end.X - start.X) * 0.01);
        InkCanvas.SetTop(this, (end.Y - start.Y) * 0.01);
    }

    public override void Draw(InkCanvas canvas)
    {
        throw new NotImplementedException();
    }

    internal override ShapeBase Clone()
    {
        throw new NotImplementedException();
    }
}

public class PointShape : ShapeBase
{
    private EllipseGeometry ellipseGeometry;

    public PointShape()
    {
        ellipseGeometry = new EllipseGeometry();
    }

    protected override Geometry DefiningGeometry => ellipseGeometry;

    public override void Refresh()
    {
        double radius = 5;
        ellipseGeometry.Center = PointStart;
        ellipseGeometry.RadiusX = radius;
        ellipseGeometry.RadiusY = radius;
        InkCanvas.SetLeft(this, PointEnd.X - radius );
        InkCanvas.SetTop(this, PointEnd.Y - radius );
    }

    public override void Draw(InkCanvas canvas)
    {
        throw new NotImplementedException();
    }

    internal override ShapeBase Clone()
    {
        throw new NotImplementedException();
    }
}

public class PolygonShape : ShapeBase
{
    public List<Point> Points { get; set; } = new List<Point>();

    protected override Geometry DefiningGeometry
    {
        get
        {
            var geometry = new StreamGeometry();
            using (var context = geometry.Open())
            {
                if (Points.Count > 0)
                {
                    context.BeginFigure(Points.First(), true, true);
                    context.PolyLineTo(Points.Skip(1).ToList(), true, true);
                }
            }
            geometry.Freeze();
            return geometry;
        }
    }

    public override void Refresh() => InvalidateVisual();

    public void RefreshPolygonPoints(List<Point> points)
    {
        Points = points;
        Refresh();
    }

    public override void Draw(InkCanvas canvas)
    {
        throw new NotImplementedException();
    }

    internal override ShapeBase Clone()
    {
        throw new NotImplementedException();
    }
}
