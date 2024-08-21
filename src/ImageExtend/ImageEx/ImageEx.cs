using ImageExtend.Extension;
using ImageExtend.ShapeEx;
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;

namespace ImageExtend;

#region begavior

/// <summary>
/// 关于浏览相关的行为
/// 图像缩放&移动
/// </summary>
internal class ImageExViewerBehavior : Behavior<ImageEx>
{
    /// <summary>
    /// 将鼠标滚轮、鼠标移动、鼠标按下和松开的事件绑定到 AssociatedObject 的相关控件
    /// </summary>
    protected override void OnAttached()
    {
        AssociatedObject.MainPanel!.MouseWheel += OnZoomChanged;
        AssociatedObject.Scroll!.MouseMove += OnMoveChanged;
        AssociatedObject.Scroll!.PreviewMouseDown += OnMoveStart;
        AssociatedObject.Scroll!.PreviewMouseUp += OnMoveStop;
    }

    /// <summary>
    /// 解除事件绑定
    /// </summary>
    protected override void OnDetaching()
    {
        AssociatedObject.MainPanel!.MouseWheel -= OnZoomChanged;
        AssociatedObject.Scroll!.MouseMove -= OnMoveChanged;
        AssociatedObject.Scroll!.PreviewMouseDown -= OnMoveStart;
        AssociatedObject.Scroll!.PreviewMouseUp -= OnMoveStop;
    }

    /// <summary>
    /// 处理鼠标滚轮事件来实现图像的缩放。缩放比例根据鼠标滚轮的滚动方向和当前缩放级别来调整
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnZoomChanged(object sender, MouseWheelEventArgs e)
    {
        var oldScale = AssociatedObject.ImagePanelScale;

        if (oldScale <= 0) return;

        // bug 放缩在default级别

        // 放大倍数使用放大十倍以内为0.1，当大100倍以内为1，以此内推
        var scale = Math.Log10(AssociatedObject.ImagePanelScale / AssociatedObject.DefaultImagePanelScale);

        scale = (scale <= 0 ? 0.1 : Math.Pow(10, Math.Floor(scale))) * AssociatedObject.DefaultImagePanelScale;

        if (e.Delta <= 0)
        {
            AssociatedObject.ImagePanelScale -= AssociatedObject.DefaultImagePanelScale * scale;

            // 最小为缩小10倍
            if (AssociatedObject.ImagePanelScale <= AssociatedObject.DefaultImagePanelScale * 0.1)
                AssociatedObject.ImagePanelScale = AssociatedObject.DefaultImagePanelScale * 0.1;
        }
        else
            AssociatedObject.ImagePanelScale += AssociatedObject.DefaultImagePanelScale * scale;

        // update the offset
        if (AssociatedObject.ImagePanelScale <= AssociatedObject.DefaultImagePanelScale) return;

        var transform = AssociatedObject.ImagePanelScale / oldScale;
        var pos = e.GetPosition(AssociatedObject.Box);
        var target = new Point(pos.X * transform, pos.Y * transform);
        var offset = target - pos;

        AssociatedObject.Scroll!.ScrollToHorizontalOffset(AssociatedObject.Scroll.HorizontalOffset + offset.X);
        AssociatedObject.Scroll!.ScrollToVerticalOffset(AssociatedObject.Scroll.VerticalOffset + offset.Y);
    }

    private Point _cursor = new(-1, -1);

    /// <summary>
    /// 拖拽开始，获取鼠标起始位置
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMoveStart(object sender, MouseButtonEventArgs e)
    {
        _cursor = e.GetPosition(AssociatedObject);
    }

    /// <summary>
    /// 拖拽结束，获取鼠标起始位置
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMoveStop(object sender, MouseButtonEventArgs e)
    {
        _cursor = new(-1, -1);
    }

    /// <summary>
    /// 鼠标拖动事件
    /// 计算偏移量并调整滚动视图的位置
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnMoveChanged(object sender, MouseEventArgs e)
    {
        if (e.LeftButton != MouseButtonState.Pressed) return;
        if (_cursor.X < 0 || _cursor.Y < 0) return;

        var pos = e.GetPosition(AssociatedObject);
        var offset = pos - _cursor;
        _cursor = pos;

        AssociatedObject.Scroll!.ScrollToHorizontalOffset(AssociatedObject.Scroll.HorizontalOffset - offset.X);
        AssociatedObject.Scroll.ScrollToVerticalOffset(AssociatedObject.Scroll.VerticalOffset - offset.Y);
    }
}

/// <summary>
/// 处理在图像上绘制形状的功能
/// </summary>
internal class ImageExDrawBehavior : Behavior<ImageEx>
{
    /// <summary>
    /// 将 Loaded 事件绑定到 AssociatedObject
    /// </summary>
    protected override void OnAttached()
    {
        AssociatedObject.Loaded += OnLoaded;
    }

    /// <summary>
    /// 解除事件绑定
    /// </summary>
    protected override void OnDetaching()
    {
        AssociatedObject.Canvas!.PreviewMouseMove -= OnCanvasPreviewMouseMove;
        AssociatedObject.Canvas!.PreviewMouseDown -= OnCanvasPreviewMouseDown;
        AssociatedObject.Canvas!.MouseLeave -= OnCanvasMouseLeave;
        AssociatedObject.MainPanel!.PreviewMouseUp -= OnCanvasPreviewMouseUp;
        AssociatedObject.Loaded -= OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        AssociatedObject.Canvas!.PreviewMouseMove += OnCanvasPreviewMouseMove;
        AssociatedObject.Canvas!.PreviewMouseDown += OnCanvasPreviewMouseDown;
        AssociatedObject.Canvas!.MouseLeave += OnCanvasMouseLeave;    
        AssociatedObject.MainPanel!.PreviewMouseUp += OnCanvasPreviewMouseUp;
    }

    /// <summary>
    /// 处理在画布上的鼠标事件以绘制形状
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasMouseLeave(object sender, MouseEventArgs e) { }

    /// <summary>
    /// 处理在画布上的鼠标事件以绘制形状
    /// 松开时确定最终形状
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.ShapePreviewer == null || AssociatedObject.ShapeMarker == null) return;

        if (AssociatedObject.ShapeMarker is RectangleShape)
        {
            if (!_flagRect) return;
            _flagRect = false;

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Collapsed;
            AssociatedObject.Canvas!.Cursor = Cursors.Arrow;

            if (!ValidLocation(e)) return;

            var min = Math.Min(AssociatedObject.ShapePreviewer!.Height, AssociatedObject.ShapePreviewer!.Width);
            var threshold = Math.Min(AssociatedObject.ImageSource!.Height, AssociatedObject.ImageSource!.Width) * 0.02;
            if (threshold > min) return;

            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);

            AssociatedObject.ShapeMarker!.PointStart = AssociatedObject.ShapePreviewer!.PointStart;
            AssociatedObject.ShapeMarker!.PointEnd = AssociatedObject.ShapePreviewer!.PointEnd;
            AssociatedObject.ShapeMarker!.Visibility = Visibility.Visible;
            AssociatedObject.ShapeMarker!.Refresh();

            if (AssociatedObject.GridRow != 0 || AssociatedObject.GridCol != 0)
            {
                AssociatedObject.ShapeMarker!.Fill =
                    GridFillBrush(AssociatedObject.ShapeMarker.Width, AssociatedObject.ShapeMarker.Height);
            }

        }
        else if (AssociatedObject.ShapeMarker is LineShape)
        {
            if (!_flagLine) return;
            _flagLine = false;

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Collapsed;
            AssociatedObject.Canvas!.Cursor = Cursors.Arrow;

            if (!ValidLocation(e)) return;

            var min = Math.Min(AssociatedObject.ShapePreviewer!.Height, AssociatedObject.ShapePreviewer!.Width);
            var threshold = Math.Min(AssociatedObject.ImageSource!.Height, AssociatedObject.ImageSource!.Width) * 0.02;
            if (threshold > min) return;

            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);

            AssociatedObject.ShapeMarker!.PointStart = AssociatedObject.ShapePreviewer!.PointStart;
            AssociatedObject.ShapeMarker!.PointEnd = AssociatedObject.ShapePreviewer!.PointEnd;
            AssociatedObject.ShapeMarker!.Visibility = Visibility.Visible;
            AssociatedObject.ShapeMarker!.Refresh();
        }
        else if (AssociatedObject.ShapeMarker is PointShape)
        {
            if (!_flagPoint) return;
            _flagPoint = false;

            if (!ValidLocation(e)) return;

            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);
            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Collapsed;

            AssociatedObject.ShapeMarker!.PointEnd = AssociatedObject.ShapePreviewer!.PointEnd;
            AssociatedObject.ShapeMarker!.Visibility = Visibility.Visible;
            AssociatedObject.ShapeMarker!.Refresh();
        }
        else if (AssociatedObject.ShapeMarker is PolygonShape)
        {

        }
    }

    /// <summary>
    /// 处理在画布上的鼠标事件以绘制形状
    /// 移动时更新预览形状
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (AssociatedObject.ShapePreviewer == null || AssociatedObject.ShapeMarker == null) return;

        if (AssociatedObject.ShapeMarker is RectangleShape)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton != MouseButtonState.Pressed) return;

            _flagRect = true;

            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);

            AssociatedObject.Canvas!.Cursor = Cursors.Cross;

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;
            AssociatedObject.ShapePreviewer!.Refresh();
        }
        else if (AssociatedObject.ShapeMarker is LineShape)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton != MouseButtonState.Pressed) return;

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Collapsed;

            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);
            if (!ValidLine(AssociatedObject.ShapePreviewer!.PointStart, AssociatedObject.ShapePreviewer!.PointEnd)) return;

            _flagLine = true;
            AssociatedObject.Canvas!.Cursor = Cursors.Cross;

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;
            AssociatedObject.ShapePreviewer!.Refresh();
        }
        else if (AssociatedObject.ShapeMarker is PointShape)
        {
            if (!ValidLocation(e)) return;
            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);
            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;
            AssociatedObject.ShapePreviewer!.Refresh();
        }
        else if (AssociatedObject.ShapeMarker is PolygonShape)
        {
            if (!AssociatedObject.isMsgExist) return;
            if (_flagPlgReset) return;

            var polygonShape = AssociatedObject.ShapePreviewer as PolygonShape;
            if (polygonShape?.Points?.Count == 0) return;

            var point = e.GetPosition(AssociatedObject.Canvas);
            if (_flagPlg)
            {
                if (ValidPoint(point, _plgPoints)) _plgPoints.Add(point);
                _flagPlg = false;
            }
            else
            {
                _plgPoints[_plgPoints.Count - 1] = point;
            }
            polygonShape?.RefreshPolygonPoints(_plgPoints);
        }
    }

    /// <summary>
    /// 处理在画布上的鼠标事件以绘制形状
    /// 按下时记录起始点
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.ShapePreviewer == null || AssociatedObject.ShapeMarker == null) return;

        if (AssociatedObject.ShapeMarker is RectangleShape)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton != MouseButtonState.Pressed) return;
            AssociatedObject.ShapePreviewer!.PointStart = e.GetPosition(AssociatedObject.Canvas);
        }
        else if (AssociatedObject.ShapeMarker is LineShape)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton != MouseButtonState.Pressed) return;
            AssociatedObject.ShapePreviewer!.PointStart = e.GetPosition(AssociatedObject.Canvas);
        }
        else if (AssociatedObject.ShapeMarker is PointShape)
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton != MouseButtonState.Pressed)
            {
                _flagPoint = true;
            }
        }
        else if (AssociatedObject.ShapeMarker is PolygonShape)
        {
            if (!AssociatedObject.isMsgExist) return;

            if (e.LeftButton != MouseButtonState.Pressed || e.RightButton == MouseButtonState.Pressed) return;

            if (_flagPlgReset)
            {
                _plgPoints.Clear();
                AssociatedObject.ShapeMarker!.Visibility = Visibility.Collapsed;
            }

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;

            var point = e.GetPosition(AssociatedObject.Canvas);
            if (ValidPoint(point, _plgPoints)) _plgPoints.Add(point);

            var polygonShape = AssociatedObject.ShapePreviewer as PolygonShape;
            polygonShape?.RefreshPolygonPoints(_plgPoints);

            _flagPlg = true;
            _flagPlgReset = false;

            //双击实现多边形闭环
            if (e.ClickCount == 2)
            {
                AssociatedObject.ShapePreviewer!.Visibility = Visibility.Collapsed;
                AssociatedObject.ShapeMarker!.Visibility = Visibility.Visible;

                if (_plgPoints.Count >= 3)
                {
                    var sortsPoints = SortPointsToFormPolygon(_plgPoints);
                    var polygonMarker = AssociatedObject.ShapeMarker as PolygonShape;
                    polygonMarker?.RefreshPolygonPoints(sortsPoints);

                    if (AssociatedObject.GridRow != 0 || AssociatedObject.GridCol != 0)
                    {
                        double width = sortsPoints.Max(p => p.X) - sortsPoints.Min(p => p.X);
                        double height = sortsPoints.Max(p => p.Y) - sortsPoints.Min(p => p.Y);
                        AssociatedObject.ShapeMarker!.Fill = GridFillBrush(width, height);

                    }

                    _flagPlgReset = true;
                    AssociatedObject.isMsgExist = false;
                }
                else
                {
                    _plgPoints.Clear();
                    AssociatedObject.ShapeMarker!.Visibility = Visibility.Collapsed;
                    _flagPlgReset = false;
                }
            }
        }
    }

    //marker标记
    private bool _flagLine = false;
    private bool _flagRect = false;
    private bool _flagPoint = false;
    private bool _flagPlg = false;
    private bool _flagPlgReset = false;
    private readonly List<Point> _plgPoints= new();

    /// <summary>
    /// 验证鼠标位置是否在画布范围内
    /// </summary>
    /// <param name="e"></param>
    /// <returns>
    /// true - 合法
    /// </returns>
    private bool ValidLocation(MouseEventArgs e)
    {
        // valid location
        var pos = e.GetPosition(AssociatedObject.Canvas);
        var width = AssociatedObject.Canvas!.ActualWidth;
        var height = AssociatedObject.Canvas!.ActualHeight;

        //todo，point的InkCanvas.Set不偏移，ActualHeight会变化
        return !(pos.X < 0 || pos.Y < 0 || pos.X > width || pos.Y > height);
    }

    /// <summary>
    /// 多边形，点过于相近不添加
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    private bool ValidPoint(Point point, List<Point> points)
    {
        double threshold = 0.01;
        if (points.Count <= 1) return true;
        foreach (var existingPoint in points)
        {
            var value = Distance(point, existingPoint);
            if (value < threshold) return false;
        }
        return true;
    }

    /// <summary>
    /// 线段，点过于相近不添加
    /// </summary>
    /// <param name="start"></param>
    /// <param name="end"></param>
    /// <returns></returns>
    private bool ValidLine(Point start, Point end)
    {
        double threshold = 5;
        return Distance(start, end) > threshold;
    }

    /// <summary>
    /// 计算线的两点间距离
    /// </summary>
    /// <param name="p1"></param>
    /// <param name="p2"></param>
    /// <returns></returns>
    private double Distance(Point p1, Point p2)
    {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
    }

    /// <summary>
    /// 多边形点集合排序，避免交叉
    /// </summary>
    /// <param name="points"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    private List<Point> SortPointsToFormPolygon(List<Point> points)
    {
        if (points == null || points.Count < 3)
        {
            //return sortedPoints;
            throw new ArgumentException("多边形必须至少有三个点。");
        }

        // 找到最低且最左的点作为参考点
        Point referencePoint = points.OrderBy(p => p.Y).ThenBy(p => p.X).First();

        // 按照与参考点的极角排序
        List<Point> sortedPoints = points.OrderBy(p => Math.Atan2(p.Y - referencePoint.Y, p.X - referencePoint.X)).ToList();

        return sortedPoints;
    }

    /// <summary>
    /// 绘制网格&填充
    /// </summary>
    /// <param name="width"></param>
    /// <param name="height"></param>
    /// <param name="fillEndPoint"></param>
    /// <param name="fillColor"></param>
    /// <returns></returns>
    private Brush GridFillBrush(double width, double height)
    {
        int verGridCount = AssociatedObject.GridCol;
        int horGridCount = AssociatedObject.GridRow;
        var gridColor = AssociatedObject.GridColor;
        var gridThickness = AssociatedObject.GridThickness;
        var gridFillColor = AssociatedObject.GridFillColor;
        var fillEndPoint = AssociatedObject.FillEndPoint;

        VisualBrush gridBrush = new();
        double gridSpaceX = width / verGridCount;
        double gridSpaceY = height / horGridCount;
        if (gridSpaceX > 0 && gridSpaceY > 0)
        {
            DrawingVisual gridVisual = new();
            using (DrawingContext dc = gridVisual.RenderOpen())
            {
                Pen gridPen = new(gridColor, gridThickness) { DashStyle = new DashStyle(new double[] { 2, 2 }, 0) };

                for (int x = 0; x <= verGridCount; x++)
                {
                    for (int y = 0; y <= horGridCount; y++)
                    {
                        double cellX = x * gridSpaceX;
                        double cellY = y * gridSpaceY;

                        // 绘制填充矩形
                        if (y < fillEndPoint.Item2 - 1 || (y == fillEndPoint.Item2 - 1 && x < fillEndPoint.Item1))
                            dc.DrawRectangle(gridFillColor, null, new Rect(cellX, cellY, gridSpaceX, gridSpaceY));

                        // 绘制垂直线，首尾不绘制
                        if (x > 0 && cellX != width)
                            dc.DrawLine(gridPen, new Point(cellX, 0), new Point(cellX, height));

                        // 绘制水平线，首尾不绘制
                        if (y > 0 && cellY != height)
                            dc.DrawLine(gridPen, new Point(0, cellY), new Point(width, cellY));
                    }
                }
            }
            gridBrush = new VisualBrush(gridVisual)
            {
                Stretch = Stretch.None,
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                TileMode = TileMode.None
            };
        }
        return gridBrush;
    }

    /// <summary>
    /// 判断是否在roi内——射线投射法
    /// </summary>
    /// <param name="polygonPoints"></param>
    /// <param name="testPoint"></param>
    /// <returns></returns>
    private bool IsPointInPolygon(List<Point> polygonPoints, Point testPoint)
    {
        int numPoints = polygonPoints.Count;
        bool isInside = false;

        // 遍历多边形的所有边
        for (int i = 0, j = numPoints - 1; i < numPoints; j = i++)
        {
            var pointI = polygonPoints[i];
            var pointJ = polygonPoints[j];

            // 判断测试点是否在多边形边的两端点的Y坐标之间
            bool isYInRange = ((pointI.Y > testPoint.Y) != (pointJ.Y > testPoint.Y));

            // 计算射线是否穿过多边形的边
            bool isXOnRay = (testPoint.X < (pointJ.X - pointI.X) * (testPoint.Y - pointI.Y) / (pointJ.Y - pointI.Y) + pointI.X);

            if (isYInRange && isXOnRay)
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }
}

#endregion

/// <summary>
/// 自定义的 WPF 控件，集成了图像显示、缩放、移动和标记功能
/// </summary>
[TemplatePart(Name = NamePartMainPanel, Type = typeof(Panel))]
[TemplatePart(Name = NamePartScrollView, Type = typeof(ScrollViewer))]
[TemplatePart(Name = NamePartViewBox, Type = typeof(Viewbox))]
[TemplatePart(Name = NamePartCanvas, Type = typeof(InkCanvas))]
public class ImageEx : ContentControl
{
    #region Name

    //定义控件模板中的各种部分，包括主面板、滚动视图、视图框、画布和形状预览器、标记器。

    public const string NamePartMainPanel = "PART_MAIN_PANEL";

    public const string NamePartScrollView = "PART_SCROLL";

    public const string NamePartViewBox = "PART_BOX";

    public const string NamePartCanvas = "PART_CANVAS";

    //public string NamePartShapePreviewer = string.Empty;
    //public string NamePartShapeMarkder = string.Empty;

    #endregion

    #region Part

    internal Panel? MainPanel;

    internal ScrollViewer? Scroll;

    internal Viewbox? Box;

    internal InkCanvas? Canvas;

    internal ShapeBase? ShapePreviewer;

    internal ShapeBase? ShapeMarker;

    #endregion

    #region MarkerCommand

    //定义删除标记和缩放到当前标记的命令。

    /// <summary>
    /// 删除当前标记
    /// </summary>
    public const string Delete = "Delete";

    /// <summary>
    /// 放缩到当前标记
    /// </summary>
    public const string ZoomScale = "ZoomScale";

    /// <summary>
    /// 标记命令
    /// marker上显示弹窗
    /// </summary>
    public static readonly RoutedUICommand MarkerCommand = new();

    #endregion

    #region MarkeronImageCommand

    /// <summary>
    /// 点标记
    /// </summary>
    public const string PointMarker = "PointMarker";

    /// <summary>
    /// 线标记
    /// </summary>
    public const string LineMarker = "LineMarker";

    /// <summary>
    /// 矩形标记
    /// </summary>
    public const string RectMarker = "RectMarker";

    /// <summary>
    /// 多边形标记
    /// </summary>
    public const string PolygonMarker = "PolygonMarker";

    /// <summary>
    /// 多边形限制
    /// 单次只能绘制一个多边形
    /// </summary>
    public bool isMsgExist=false;

    /// <summary>
    /// 标记命令
    /// Image上显示弹窗
    /// </summary>
    public static readonly RoutedUICommand MarkeronImageCommand = new();
    #endregion

    #region ImageProcessCommands

    /// <summary>
    /// 图像处理
    /// </summary>
    public const string ImageProcess = "ImageProcess";

    /// <summary>
    /// 保存显示原图
    /// </summary>
    public const string SaveDisplay = "SaveDisplay";

    /// <summary>
    /// 保存截图
    /// </summary>
    public const string SaveDump = "SaveDump";

    /// <summary>
    /// 图像处理命令
    /// </summary>
    public static readonly RoutedUICommand ImageProcessCommand = new();

    private void SaveBitmapImage(BitmapImage bitmapImage, string filePath)
    {
        BitmapEncoder encoder;
        string extension = System.IO.Path.GetExtension(filePath).ToLower();

        switch (extension)
        {
            case ".png":
                encoder = new PngBitmapEncoder();
                break;
            case ".jpeg":
            case ".jpg":
                encoder = new JpegBitmapEncoder();
                break;
            case ".bmp":
                encoder = new BmpBitmapEncoder();
                break;
            case ".gif":
                encoder = new GifBitmapEncoder();
                break;
            case ".tiff":
            case ".tif":
                encoder = new TiffBitmapEncoder();
                break;
            default:
                throw new NotSupportedException($"Unsupported file extension: {extension}");
        }

        encoder.Frames.Add(BitmapFrame.Create(bitmapImage));

        using (FileStream fileStream = new FileStream(filePath, FileMode.Create))
        {
            encoder.Save(fileStream);
        }
    }

    private BitmapImage ImageSourcetoBitmapImage(ImageSource _imageSource)
    {
        BitmapImage bitmapImage = new BitmapImage();

        RenderTargetBitmap renderTargetBitmap = new RenderTargetBitmap((int)_imageSource.Width, (int)_imageSource.Height, 96, 96, PixelFormats.Pbgra32);
        DrawingVisual drawingVisual = new DrawingVisual();
        using (DrawingContext drawingContext = drawingVisual.RenderOpen())
        {
            drawingContext.DrawImage(_imageSource, new Rect(new Size((int)_imageSource.Width, (int)_imageSource.Height)));
        }
        renderTargetBitmap.Render(drawingVisual);

        var encoder = new TiffBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
        using (MemoryStream memoryStream = new())
        {
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
        }
        return bitmapImage;
    }

    private BitmapImage InkCanvastoBitmapImage(InkCanvas inkCanvas)
    {
        BitmapImage bitmapImage = new BitmapImage();
        int width = (int)inkCanvas.ActualWidth;
        int height = (int)inkCanvas.ActualHeight;

        RenderTargetBitmap rtb = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
        DrawingVisual dv = new DrawingVisual();
        using (DrawingContext dc = dv.RenderOpen())
        {
            VisualBrush vb = new VisualBrush(inkCanvas);
            dc.DrawRectangle(vb, null, new Rect(new Point(), new Size(width, height)));
        }
        rtb.Render(dv);

        var encoder = new TiffBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(rtb));
        using (MemoryStream memoryStream = new())
        {
            encoder.Save(memoryStream);
            memoryStream.Position = 0;

            bitmapImage.BeginInit();
            bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
            bitmapImage.StreamSource = memoryStream;
            bitmapImage.EndInit();
        }
        return bitmapImage;
    }

    private void OpenFolderAndSelectFile(string fileFullName)
    {
        ProcessStartInfo psi = new ProcessStartInfo("Explorer.exe");
        psi.Arguments = "/e,/select," + fileFullName;
        Process.Start(psi);
    }

    #endregion

    #region Events-定义标记变化和光标变化的事件

    public Action<Rect>? OnRectMarkderChanged;//todo，保存信息、处理对应数据

    public Action<(int X, int Y, Color C)>? OnCursorChanged;
    public Func<int, int, Color>? GetImageColorFromPosition;//todo，获取当前点击位置颜色

    private void OnCanvasCursorChanged(object sender, MouseEventArgs e)
    {
        base.OnPreviewMouseMove(e);

        if (ImageSource is null || GetImageColorFromPosition is null) return;

        var pos = e.GetPosition(Canvas);
        var x = (int)Math.Floor(pos.X);
        var y = (int)Math.Floor(pos.Y);

        var c = GetImageColorFromPosition(x, y);
        OnCursorChanged?.Invoke((x, y, c));
    }

    #endregion

    private readonly List<Behavior> _behaviors = new();

    /// <summary>
    /// 初始化行为类并添加到 _behaviors 列表中。为命令绑定处理程序
    /// </summary>
    static ImageEx()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageEx), new FrameworkPropertyMetadata(typeof(ImageEx)));
    }

    /// <summary>
    /// 初始化行为类并添加到 _behaviors 列表中。为命令绑定处理程序
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public ImageEx()
    {
        _behaviors.Add(new ImageExViewerBehavior());
        _behaviors.Add(new ImageExDrawBehavior());

        //标记上蒙版右键菜单-delete/zoom/acquire
        CommandBindings.Add(new CommandBinding(MarkerCommand, (obj, args) =>
        {
            if (args.Parameter is not string command || ShapeMarker is null) return;
            if (command == Delete) ShapeMarker.Visibility = Visibility.Collapsed;
            else if (command == ZoomScale)
            {
                // zoom the 90% width or height
                var marker = ShapeMarker;
                var start = marker.PointStart;
                var end = marker.PointEnd;

                var location = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));
                var center = (location.X + ShapeMarker.Width / 2, location.Y + ShapeMarker.Height / 2);

                var width = ShapeMarker.Width * DefaultImagePanelScale;
                var height = ShapeMarker.Height * DefaultImagePanelScale;

                var scaleX = ActualWidth * 0.9 / width;
                var scaleY = ActualHeight * 0.9 / height;

                var scale = Math.Min(scaleX, scaleY);
                ImagePanelScale = scale * DefaultImagePanelScale;

                var offsetX = center.Item1 * ImagePanelScale - ActualWidth / 2;
                var offsetY = center.Item2 * ImagePanelScale - ActualHeight / 2;

                Scroll?.ScrollToHorizontalOffset(offsetX);
                Scroll?.ScrollToVerticalOffset(offsetY);
            }
            else throw new NotImplementedException();
        }));

        //Mark标记
        CommandBindings.Add(new CommandBinding(MarkeronImageCommand, (obj, args) =>
        {
            if (args.Parameter is not string command) return;

            if (ShapePreviewer != null)
                ShapePreviewer.Visibility = Visibility.Collapsed;

            if (ShapeMarker != null)
                ShapeMarker.Visibility = Visibility.Collapsed;

            ShapePreviewer = Template.FindName(GetCurrentShapeType(true, command), this) as ShapeBase;
            ShapeMarker = Template.FindName(GetCurrentShapeType(false, command), this) as ShapeBase;

            if (ShapeMarker != null)
                ShapeCollection = new() { ShapeMarker };

            isMsgExist = true;

        }));

        //图像处理
        CommandBindings.Add(new CommandBinding(ImageProcessCommand, (obj, args) =>
        {
            if (args.Parameter is not string command) return;

            if (command == ImageProcess)
            {
                if (ImageSource != null)
                {
                    var dialog = new ImageDisposeView();
                    dialog.ShowDialog();
                }
            }
            else if (command == SaveDisplay)
            {
                if (ImageSource == null) return;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog.Filter = "TIFF files (*.tif)|*.tif|TIFF files (*.tiff)|*.tiff|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "tif";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = "Display";

                bool? result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    var bitmapImage = ImageSourcetoBitmapImage(ImageSource);
                    SaveBitmapImage(bitmapImage, saveFileDialog.FileName);
                    OpenFolderAndSelectFile(saveFileDialog.FileName);
                }
            }
            else if (command == SaveDump)
            {
                if (Canvas == null) return;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog.Filter = "TIFF files (*.tif)|*.tif|TIFF files (*.tiff)|*.tiff|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "tif";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = "DumpImage";

                bool? result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    var bitmapImage = InkCanvastoBitmapImage(Canvas);
                    SaveBitmapImage(bitmapImage, saveFileDialog.FileName);
                    OpenFolderAndSelectFile(saveFileDialog.FileName);
                }
            }
        }));

    }

    /// <summary>
    /// 找到模板中的各部分并将行为附加到控件上
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        MainPanel = Template.FindName(NamePartMainPanel, this) as Panel;
        Scroll = Template.FindName(NamePartScrollView, this) as ScrollViewer;
        Box = Template.FindName(NamePartViewBox, this) as Viewbox;
        Canvas = Template.FindName(NamePartCanvas, this) as InkCanvas;

        _behaviors.ForEach(b => b.Attach(this));

        Canvas!.PreviewMouseMove += OnCanvasCursorChanged;
    }
  
    /// <summary>
    /// 在控件渲染时更新图像信息和平铺图像
    /// </summary>
    /// <param name="drawingContext"></param>
    protected override void OnRender(DrawingContext drawingContext)
    {
        base.OnRender(drawingContext);

        UpdateImageInfo();
    }

    /// <summary>
    /// 在控件大小变化时更新图像信息和平铺图像
    /// </summary>
    /// <param name="sizeInfo"></param>
    protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
    {
        base.OnRenderSizeChanged(sizeInfo);

        UpdateImageInfo();
        TileImage();
    }

    /// <summary>
    /// 获取当前标记模式
    /// </summary>
    /// <param name="isPreviewer"></param>
    /// <param name="command"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    private string GetCurrentShapeType(bool isPreviewer, string command)
    {
        string name = string.Empty;

        switch (command)
        {
            case "PointMarker":
                name = isPreviewer ? "PART_SHAPE_POINT_PREVIEWER" : "PART_SHAPE_POINT_MARKER";
                break;
            case "LineMarker":
                name = isPreviewer ? "PART_SHAPE_LINE_PREVIEWER" : "PART_SHAPE_LINE_MARKER";
                break;
            case "RectMarker":
                name = isPreviewer ? "PART_SHAPE_RECT_PREVIEWER" : "PART_SHAPE_RECT_MARKER";
                break;
            case "PolygonMarker":
                name = isPreviewer ? "PART_SHAPE_POLYGON_PREVIEWER" : "PART_SHAPE_POLYGON_MARKER";
                break;
            default:
                throw new NotImplementedException("Not Valid Command!");
        }
        return name;
    }

    /// <summary>
    /// 图像像素位置
    /// </summary>
    public (int x, int y) ImageCurrentPosition
    {
        get
        {
            var pos = Mouse.GetPosition(Canvas);
            return ((int)pos.X, (int)pos.Y);
        }
    }

    /// <summary>
    /// 网格行列
    /// </summary>
    public (int x, int y) GridCurrentPos
    {
        get
        {
            if (GridRow == 0 || GridCol == 0) return (-1, -1);

            List<Point> sortsPoints = new();
            Point _constrastPoint = new();
            double width = 0;
            double height = 0;
            if (ShapeMarker is PolygonShape)
            {
                var polygonMarker = ShapeMarker as PolygonShape;
                sortsPoints = SortPointsToFormPolygon(polygonMarker?.Points!);
                double minX = sortsPoints.Min(p => p.X);
                double maxX = sortsPoints.Max(p => p.X);
                double minY = sortsPoints.Min(p => p.Y);
                double maxY = sortsPoints.Max(p => p.Y);
                _constrastPoint = new Point(minX, minY);
                width = maxX - minX;
                height = maxY - minY;
            }
            else if (ShapeMarker is RectangleShape)
            {
                width = ShapeMarker!.ActualWidth;
                height = ShapeMarker!.ActualHeight;
                _constrastPoint = ShapeMarker!.PointStart;
            }
            else
            {
                return (-1, -1);
            }

            double _gridSpacingX = width / GridCol;
            double _gridSpacingY = height / GridRow;
            if (_gridSpacingX == 0 || _gridSpacingY == 0) return (-1, -1);

            var currentPos = Mouse.GetPosition(Canvas);
            int row = (int)Math.Floor((currentPos.Y - _constrastPoint.Y) / _gridSpacingY) + 1;
            int column = (int)Math.Floor((currentPos.X - _constrastPoint.X) / _gridSpacingX) + 1;

            bool isPointInPolygon = true;
            if (ShapeMarker is PolygonShape)
                isPointInPolygon = IsPointInPolygon(sortsPoints, currentPos);

            if ((row > GridRow || row <= 0) || (column > GridCol || column <= 0) || !isPointInPolygon)
                return (-1, -1);

            return (column, row);
        }
    }

    private List<Point> SortPointsToFormPolygon(List<Point> points)
    {
        if (points == null || points.Count < 3)
        {
            throw new ArgumentException("多边形必须至少有三个点。");
        }

        // 找到最低且最左的点作为参考点
        Point referencePoint = points.OrderBy(p => p.Y).ThenBy(p => p.X).First();

        // 按照与参考点的极角排序
        List<Point> sortedPoints = points.OrderBy(p => Math.Atan2(p.Y - referencePoint.Y, p.X - referencePoint.X)).ToList();

        return sortedPoints;
    }

    private bool IsPointInPolygon(List<Point> polygonPoints, Point testPoint)
    {
        int numPoints = polygonPoints.Count;
        bool isInside = false;

        // 遍历多边形的所有边
        for (int i = 0, j = numPoints - 1; i < numPoints; j = i++)
        {
            var pointI = polygonPoints[i];
            var pointJ = polygonPoints[j];

            // 判断测试点是否在多边形边的两端点的Y坐标之间
            bool isYInRange = ((pointI.Y > testPoint.Y) != (pointJ.Y > testPoint.Y));

            // 计算射线是否穿过多边形的边
            bool isXOnRay = (testPoint.X < (pointJ.X - pointI.X) * (testPoint.Y - pointI.Y) / (pointJ.Y - pointI.Y) + pointI.X);

            if (isYInRange && isXOnRay)
            {
                isInside = !isInside;
            }
        }

        return isInside;
    }

    #region 依赖属性

    // 定义图像源（ImageSource）、形状集合（ShapeCollection）、标记菜单（MarkerMenu）和图像面板缩放（ImagePanelScale）

    #region ImageSource
    /// <summary>
    /// 设置和获取控件的图像源，并在图像源变化时更新图像信息和平铺图像
    /// </summary>
    public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
        nameof(ImageSource), typeof(ImageSource), typeof(ImageEx), new PropertyMetadata(null, (o, p) =>
        {
            if (o is not ImageEx ex) return;
            ex.UpdateImageInfo();

            if (p.OldValue is not BitmapSource s1)
            {
                ex.TileImage();
                return;
            }

            if (p.NewValue is not BitmapSource s2) return;

            if (Math.Abs(s1.Width - s2.Width) > 0.001
                || Math.Abs(s1.Height - s2.Height) > 0.001) ex.TileImage();

        }));

    public ImageSource? ImageSource
    {
        get => (ImageSource?)GetValue(ImageSourceProperty);
        set => SetValue(ImageSourceProperty, value);
    }

    #endregion

    #region ShapeCollection
    /// <summary>
    /// 设置和获取形状集合，并在形状集合变化时更新画布上的形状
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ImageEx ex || ex.Canvas is null) return;

        if (e.OldValue is not ObservableCollection<ShapeBase> oVal ||
            e.NewValue is not ObservableCollection<ShapeBase> nVal) return;

        oVal.ForEach(item =>
        {
            item.Clear(ex.Canvas);
            //var clone = item.Clone();
            //clone.Draw(ex.Canvas);

        });

        nVal.ForEach(item => item?.Draw(ex.Canvas));
    }

    public ObservableCollection<ShapeBase> ShapeCollection
    {
        get => (ObservableCollection<ShapeBase>)GetValue(ShapeCollectionProperty);
        set => SetValue(ShapeCollectionProperty, value);
    }

    public static readonly DependencyProperty ShapeCollectionProperty = DependencyProperty.Register(
        nameof(ShapeCollection), typeof(ObservableCollection<ShapeBase>), typeof(ImageEx), new PropertyMetadata(new ObservableCollection<ShapeBase>(), OnShapeChanged));

    #endregion

    #region MarkerMenu
    public static readonly DependencyProperty MarkerMenuProperty = DependencyProperty.Register(
        nameof(MarkerMenu), typeof(ContextMenu), typeof(ImageEx), new PropertyMetadata(default(ContextMenu)));

    /// <summary>
    /// 设置和获取标记菜单
    /// </summary>
    public ContextMenu MarkerMenu
    {
        get => (ContextMenu)GetValue(MarkerMenuProperty);
        set => SetValue(MarkerMenuProperty, value);
    }

    #endregion

    #region Grid Appearance

    public static readonly DependencyProperty GridColProperty = DependencyProperty.RegisterAttached(
        nameof(GridCol), typeof(int), typeof(ImageEx), new PropertyMetadata(0));

    public int GridCol
    {
        get => (int)GetValue(GridColProperty);
        set => SetValue(GridColProperty, value);
    }

    public static readonly DependencyProperty GridRowProperty = DependencyProperty.RegisterAttached(
        nameof(GridRow), typeof(int), typeof(ImageEx), new PropertyMetadata(0));

    public int GridRow
    {
        get => (int)GetValue(GridRowProperty);
        set => SetValue(GridRowProperty, value);
    }

    public static readonly DependencyProperty GridColorProperty = DependencyProperty.RegisterAttached(
        nameof(GridColor), typeof(Brush), typeof(ImageEx), new PropertyMetadata(Brushes.Red));

    public Brush GridColor
    {
        get => (Brush)GetValue(GridColorProperty);
        set => SetValue(GridColorProperty, value);
    }

    public static readonly DependencyProperty GridThicknessProperty = DependencyProperty.RegisterAttached(
        nameof(GridThickness), typeof(int), typeof(ImageEx), new PropertyMetadata(1));

    public int GridThickness
    {
        get => (int)GetValue(GridThicknessProperty);
        set => SetValue(GridThicknessProperty, value);
    }

    public static readonly DependencyProperty GridFillColorProperty = DependencyProperty.RegisterAttached(
        nameof(GridFillColor), typeof(Brush), typeof(ImageEx), new PropertyMetadata(Brushes.Transparent));

    public Brush GridFillColor
    {
        get => (Brush)GetValue(GridFillColorProperty);
        set => SetValue(GridFillColorProperty, value);
    }

    public static readonly DependencyProperty FillEndPointProperty = DependencyProperty.RegisterAttached(
    nameof(FillEndPoint), typeof((int, int)), typeof(ImageEx), new PropertyMetadata((0, 0), OnFillEndPointChanged));

    private static void OnFillEndPointChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        //todo，重绘界面，显示新的图形填充
    }

    public (int, int) FillEndPoint
    {
        get => ((int, int))GetValue(FillEndPointProperty);
        set => SetValue(FillEndPointProperty, value);
    }



    #endregion

    #region ImagePanelScale

    internal double DefaultImagePanelScale = 0;

    internal (double Width, double Height) DefaultImageSize;

    public static readonly DependencyProperty ImagePanelScaleProperty = DependencyProperty.Register(
        nameof(ImagePanelScale), typeof(double), typeof(ImageEx), new PropertyMetadata((double)-1, OnImagePanelScale));

    /// <summary>
    /// 设置和获取图像面板的缩放比例，并在缩放比例变化时更新视图框的大小
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnImagePanelScale(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ImageEx ex || e.NewValue is not double n || n <= 0) return;
        if (ex.Box is null) return;

        ex.Box.Height = ex.DefaultImageSize.Height * ex.ImagePanelScale;
        ex.Box.Width = ex.DefaultImageSize.Width * ex.ImagePanelScale;
    }

    [Browsable(true)]
    [Category("SizeInfo")]
    [ReadOnly(true)]
    public double ImagePanelScale
    {
        get => (double)GetValue(ImagePanelScaleProperty);
        set => SetValue(ImagePanelScaleProperty, value);
    }

    #endregion

    /// <summary>
    /// 平铺图案
    /// </summary>
    private void TileImage()
    {
        ImagePanelScale = DefaultImagePanelScale;

        if (Scroll is null) return;

        Scroll.ScrollToVerticalOffset(0);
        Scroll.ScrollToHorizontalOffset(0);
    }

    /// <summary>
    /// 更新图像信息
    /// </summary>
    private void UpdateImageInfo()
    {
        if (ImageSource is null) return;

        DefaultImagePanelScale = Math.Min(ActualWidth / ImageSource.Width,
            ActualHeight / ImageSource.Height);

        DefaultImageSize = (ImageSource.Width, ImageSource.Height);
    }

    #endregion

}
