﻿
using Microsoft.Win32;
using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.Eventing.Reader;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Test.ImageExtend.Extension;
using Test.ImageExtend.ImageEx.ShapeEx;
using Test.ImageExtend.Self;
using Point = System.Windows.Point;
using Rect = System.Windows.Rect;
using Size = System.Windows.Size;

namespace Test.ImageExtend.ImageEx;

#region begavior

/// <summary>
/// 关于浏览相关的行为
/// 图像缩放&移动
/// </summary>
public class ImageExViewerBehavior : Behavior<ImageEx>
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
public class ImageExDrawBehavior : Behavior<ImageEx>
{
    private bool _flagLine = false;
    private bool _flagLineDown = false;
    private bool _flagRect = false;
    private bool _flagPoint = false;
    private bool _flagPolygon = false;
    private bool _flagPolygonReset = false;
    private int clickCount = 0;
    private const int DoubleClickTime = 400;
    private System.Timers.Timer? clickTimer;
    private List<Point> _imageEXpolygonPoints = new List<Point>();

    private void ClickTimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
    {
        clickTimer?.Stop();
        clickCount = 0;
    }

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

        clickTimer = new System.Timers.Timer(DoubleClickTime);
        clickTimer.Elapsed += ClickTimerElapsed;
    }

    /// <summary>
    /// 处理在画布上的鼠标事件以绘制形状
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasMouseLeave(object sender, MouseEventArgs e)
    {
        Debug.WriteLine($"{AssociatedObject.ShapePreviewer?.ToString()}__OnCanvasMouseLeave");
    }

    /// <summary>
    /// 处理在画布上的鼠标事件以绘制形状
    /// 松开时确定最终形状
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasPreviewMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (AssociatedObject.ShapePreviewer == null
            || AssociatedObject.ShapeMarker == null) return;

        Debug.WriteLine($"{AssociatedObject.ShapePreviewer.ToString()}__OnCanvasPreviewMouseUp");

        if (AssociatedObject.NamePartShapeMarkder.Contains("RECT"))
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

            Debug.WriteLine($"Up_p X_{AssociatedObject.ShapePreviewer!.PointStart.X.ToString("0.00")}  " + $"Y_{AssociatedObject.ShapePreviewer!.PointStart.Y.ToString("0.00")}");
        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("LINE"))
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

            Debug.WriteLine($"Up_p X_{AssociatedObject.ShapePreviewer!.PointStart.X.ToString("0.00")}  " + $"Y_{AssociatedObject.ShapePreviewer!.PointStart.Y.ToString("0.00")}");

        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("POINT"))
        {
            if (!_flagPoint) return;
            _flagPoint = false;

            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);
            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Collapsed;

            if (!ValidLocation(e)) return;

            AssociatedObject.ShapeMarker!.PointEnd = AssociatedObject.ShapePreviewer!.PointEnd;
            AssociatedObject.ShapeMarker!.Visibility = Visibility.Visible;
            AssociatedObject.ShapeMarker!.Refresh();
        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("POLYGON"))
        {
            clickCount++;
            if (clickCount == 1)
            {
                clickTimer?.Start();
            }
            else if (clickCount == 2)
            {
                clickTimer?.Stop();
                clickCount = 0;

                AssociatedObject.ShapePreviewer!.Visibility = Visibility.Collapsed;
                AssociatedObject.ShapeMarker!.Visibility = Visibility.Visible;

                var polygonMarker = AssociatedObject.ShapeMarker as PolygonShape;
                if (polygonMarker == null) return;

                if (_imageEXpolygonPoints.Count >= 3)
                {
                    _imageEXpolygonPoints.Add(_imageEXpolygonPoints.First());
                    polygonMarker.ShowPoint(_imageEXpolygonPoints);
                    _flagPolygonReset = true;
                }
                else
                {
                    _imageEXpolygonPoints.Clear();
                    AssociatedObject.ShapeMarker!.Visibility = Visibility.Collapsed;
                    _flagPolygonReset = false;
                }
            }

            Debug.WriteLine($"UP {_imageEXpolygonPoints.Count}");
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
        if (AssociatedObject.ShapePreviewer == null
            || AssociatedObject.ShapeMarker == null) return;

        if (AssociatedObject.NamePartShapeMarkder.Contains("RECT"))
        {
            if (e.LeftButton == MouseButtonState.Pressed
                || e.RightButton != MouseButtonState.Pressed) return;

            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);

            _flagRect = true;
            AssociatedObject.Canvas!.Cursor = Cursors.Cross;

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;
            AssociatedObject.ShapePreviewer!.Refresh();
        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("LINE"))
        {
            if (e.LeftButton != MouseButtonState.Pressed
                 || e.RightButton == MouseButtonState.Pressed) return;

            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);

            _flagLine = true;
            AssociatedObject.Canvas!.Cursor = Cursors.Cross;

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;
            AssociatedObject.ShapePreviewer!.Refresh();

        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("POINT"))
        {
            if (!ValidLocation(e)) return;
            AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);
            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;
            AssociatedObject.ShapePreviewer!.Refresh();
        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("POLYGON"))
        {
            if (_flagPolygonReset) return;

            var polygonShape = AssociatedObject.ShapePreviewer as PolygonShape;
            if (polygonShape == null) return;
            if (polygonShape.Points.Count == 0) return;

            var point = e.GetPosition(AssociatedObject.Canvas);

            if (_flagPolygon)
            {
                if (ValidPoint(point, _imageEXpolygonPoints))
                    _imageEXpolygonPoints.Add(point);
                else
                {
                    Debug.WriteLine("*********************************************ValidFalse");
                }

                _flagPolygon = false;
            }
            else
            {
                _imageEXpolygonPoints[_imageEXpolygonPoints.Count - 1] = point;
            }
            polygonShape.ShowPoint(_imageEXpolygonPoints);

            Debug.WriteLine($"Move {_imageEXpolygonPoints.Count}");
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
        Debug.WriteLine($"{AssociatedObject.ShapePreviewer.ToString()}__OnCanvasPreviewMouseDown");

        if (AssociatedObject.NamePartShapeMarkder.Contains("RECT"))
        {
            if (e.LeftButton == MouseButtonState.Pressed
                || e.RightButton != MouseButtonState.Pressed) return;

            AssociatedObject.ShapePreviewer!.PointStart = e.GetPosition(AssociatedObject.Canvas);

            Debug.WriteLine($"DOWN_p X_{AssociatedObject.ShapePreviewer!.PointStart.X.ToString("0.00")}  " + $"Y_{AssociatedObject.ShapePreviewer!.PointStart.Y.ToString("0.00")}");
        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("LINE"))
        {
            if (e.LeftButton != MouseButtonState.Pressed
                || e.RightButton == MouseButtonState.Pressed) return;//改成左键操作

            AssociatedObject.ShapePreviewer!.PointStart = e.GetPosition(AssociatedObject.Canvas);

        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("POINT"))
        {
            if (e.LeftButton == MouseButtonState.Pressed || e.RightButton != MouseButtonState.Pressed)
            {
                _flagPoint = true;
                AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);
                AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;
                AssociatedObject.ShapePreviewer!.Refresh();
            }
        }
        else if (AssociatedObject.NamePartShapeMarkder.Contains("POLYGON"))
        {
            var polygonShape = AssociatedObject.ShapePreviewer as PolygonShape;
            if (polygonShape == null) return;
            if (_flagPolygonReset)
            {
                _imageEXpolygonPoints.Clear();
                AssociatedObject.ShapeMarker!.Visibility = Visibility.Collapsed;
            }

            AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;

            var point = e.GetPosition(AssociatedObject.Canvas);
            if (ValidPoint(point, _imageEXpolygonPoints)) _imageEXpolygonPoints.Add(point);
            polygonShape.ShowPoint(_imageEXpolygonPoints);

            _flagPolygon = true;
            _flagPolygonReset = false;

            Debug.WriteLine($"DOWN {_imageEXpolygonPoints.Count}");
        }
    }

    /// <summary>
    /// 验证鼠标位置是否在画布范围内
    /// </summary>
    /// <param name="e"></param>
    /// <returns>
    /// true - 合法
    /// </returns>
    bool ValidLocation(MouseEventArgs e)
    {
        // valid location
        var pos = e.GetPosition(AssociatedObject.Canvas);
        var width = AssociatedObject.Canvas!.ActualWidth;
        var height = AssociatedObject.Canvas!.ActualHeight;

        //todo，point的InkCanvas.Set不偏移，ActualHeight会变化
        return !(pos.X < 0 || pos.Y < 0 || pos.X > width || pos.Y > height);
    }

    /// <summary>
    /// 点过于相近不添加
    /// </summary>
    /// <param name="point"></param>
    /// <returns></returns>
    bool ValidPoint(Point point, List<Point> points)
    {
        double threshold = 0.01;
        if (points.Count <= 1) return true;
        foreach (var existingPoint in points)
        {
            var value = Distance(point, existingPoint);
            if (value < threshold)
            {
                Debug.WriteLine($"value_{value}  point_x_{point.X}_y{point.Y} existingPoint_x_{existingPoint.X} y_{existingPoint.Y}");
                return false;
            }
        }
        return true;
    }

    double Distance(Point p1, Point p2)
    {
        return Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
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
    //定义控件模板中的各种部分，包括主面板、滚动视图、视图框、画布和形状预览器、标记器。
    #region Name

    public const string NamePartMainPanel = "PART_MAIN_PANEL";

    public const string NamePartScrollView = "PART_SCROLL";

    public const string NamePartViewBox = "PART_BOX";

    public const string NamePartCanvas = "PART_CANVAS";

    public string NamePartShapePreviewer = string.Empty;

    public string NamePartShapeMarkder = string.Empty;

    #endregion

    #region Part

    internal Panel? MainPanel;

    internal ScrollViewer? Scroll;

    internal Viewbox? Box;

    internal InkCanvas? Canvas;

    internal ShapeBase? ShapePreviewer;

    internal ShapeBase? ShapeMarker;

    #endregion

    //定义删除标记和缩放到当前标记的命令。
    #region Commands

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
    /// 标记命令
    /// Image上显示弹窗
    /// </summary>
    public static readonly RoutedUICommand MarkeronImageCommand = new();
    #endregion

    //定义图像处理命令
    #region ImageProcessCommands

    /// <summary>
    /// 图像处理
    /// </summary>
    public const string ImageProcess = "ImageProcess";

    /// <summary>
    /// 还原图像处理
    /// </summary>
    public const string ResetImageProcess = "ResetImageProcess";

    /// <summary>
    /// 还原图像处理
    /// </summary>
    public const string SaveDisplay = "SaveDisplay";

    /// <summary>
    /// 图像处理命令
    /// </summary>
    public static readonly RoutedUICommand ImageProcessCommand = new();

    private static ImageSource? _originalImageSource;
    private static double _brightness = 1;
    private static double _contrast = 0;
    private static double _gamma = 1;

    public static void SaveBitmapImage(BitmapImage bitmapImage, string filePath)
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

        PngBitmapEncoder encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
        using (MemoryStream memoryStream = new MemoryStream())
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

    private void ApplyImageProcess()
    {
        if (ImageSource == null || _originalImageSource == null) return;
        var processedImage = ImageProcessModel.Gamma(_originalImageSource, _gamma);
        processedImage = ImageProcessModel.Brightness(processedImage, _brightness);
        processedImage = ImageProcessModel.Contrast(processedImage, _contrast);

        if (processedImage != null)
        {
            processedImage.Freeze();
            ImageSource = processedImage;
        }
        //processedImage = null;
    }
    #endregion

    //定义标记变化和光标变化的事件
    #region Events

    /// <summary>
    /// 返回相对Image的坐标
    /// </summary>
    public Action<Rect>? OnRectMarkderChanged;
    public Action<Line>? OnLineMarkderChanged;//未订阅
    public Action<(int X, int Y, Color C)>? OnCursorChanged;
    public Func<int, int, Color>? GetImageColorFromPosition;

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

        //缩放拖拽
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

        //mark标记
        CommandBindings.Add(new CommandBinding(MarkeronImageCommand, (obj, args) =>
        {
            if (args.Parameter is not string command) return;

            if (ShapePreviewer != null)
                ShapePreviewer.Visibility = Visibility.Collapsed;

            if (ShapeMarker != null)
                ShapeMarker.Visibility = Visibility.Collapsed;

            NamePartShapePreviewer = GetShapeNameandCurrentType(true, command);
            NamePartShapeMarkder = GetShapeNameandCurrentType(false, command);

            ShapePreviewer = Template.FindName(NamePartShapePreviewer, this) as ShapeBase;
            ShapeMarker = Template.FindName(NamePartShapeMarkder, this) as ShapeBase;


        }));

        //图像处理
        CommandBindings.Add(new CommandBinding(ImageProcessCommand, (obj, args) =>
        {
            if (args.Parameter is not string command) return;

            if (ImageSource != null && _originalImageSource == null) _originalImageSource = ImageSource;

            if (command == ImageProcess)
            {
                if (ImageSource != null)
                {
                    var dialog = new ImageAdjust(_brightness, _contrast, _gamma);
                    dialog.GammaUpdated += (sender, args) => { _gamma = args; ApplyImageProcess(); };
                    dialog.ContrastUpdated += (sender, args) => { _contrast = args; ApplyImageProcess(); };
                    dialog.BrightnessUpdated += (sender, args) => { _brightness = args; ApplyImageProcess(); };
                    dialog.ShowDialog();
                }

                Debug.WriteLine($"_gamma_{_gamma}  _contrast_{_contrast}  _brightness_{_brightness}");
            }
            else if (command == ResetImageProcess)
            {
                ImageSource = _originalImageSource;
            }
            else if (command == SaveDisplay)
            {
                if (ImageSource == null) return;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                saveFileDialog.Filter = "TIFF files (*.tif)|*.tif|TIFF files (*.tiff)|*.tiff|All files (*.*)|*.*";
                saveFileDialog.DefaultExt = "tif";
                saveFileDialog.AddExtension = true;
                saveFileDialog.FileName = "Display"; // 设置默认文件名称

                bool? result = saveFileDialog.ShowDialog();
                if (result == true)
                {
                    var bitmapImage = ImageSourcetoBitmapImage(ImageSource);
                    SaveBitmapImage(bitmapImage, saveFileDialog.FileName);
                }
            }
        }));
    }

    private string GetShapeNameandCurrentType(bool isPreviewer, string command)
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
    /// 找到模板中的各部分并将行为附加到控件上
    /// </summary>
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();

        MainPanel = Template.FindName(NamePartMainPanel, this) as Panel;
        Scroll = Template.FindName(NamePartScrollView, this) as ScrollViewer;
        Box = Template.FindName(NamePartViewBox, this) as Viewbox;
        Canvas = Template.FindName(NamePartCanvas, this) as InkCanvas;
        ShapePreviewer = Template.FindName(NamePartShapePreviewer, this) as ShapeBase;
        ShapeMarker = Template.FindName(NamePartShapeMarkder, this) as ShapeBase;

        _behaviors.ForEach(b => b.Attach(this));

        Canvas!.PreviewMouseMove += OnCanvasCursorChanged;
    }

    private void OnCanvasCursorChanged(object sender, MouseEventArgs e)
    {
        base.OnPreviewMouseMove(e);

        if (ImageSource is null || GetImageColorFromPosition is null) return;

        var pos = e.GetPosition(Canvas);
        var x = (int)Math.Floor(pos.X);
        var y = (int)Math.Floor(pos.Y);

        var c = GetImageColorFromPosition(x, y);
        OnCursorChanged?.Invoke((x, y, c));

        Debug.WriteLine("OnCanvasCursorChanged");
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

    #region 依赖属性

    // 定义图像源（ImageSource）、形状集合（ShapeCollection）、标记菜单（MarkerMenu）和图像面板缩放（ImagePanelScale）

    /// <summary>
    /// 设置和获取控件的图像源，并在图像源变化时更新图像信息和平铺图像
    /// </summary>
    public static readonly DependencyProperty ImageSourceProperty = DependencyProperty.Register(
        nameof(ImageSource), typeof(ImageSource), typeof(ImageEx), new PropertyMetadata(null, (o, p) =>
        {
            Debug.WriteLine($"ImageSource changed__{DateTime.Now.ToString("HH-mm-ss-fff")}");

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

    /// <summary>
    /// 设置和获取形状集合，并在形状集合变化时更新画布上的形状
    /// </summary>
    /// <param name="d"></param>
    /// <param name="e"></param>
    private static void OnShapeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not ImageEx ex || ex.Canvas is null) return;

        // do some func for update the render
        if (e.OldValue is not ObservableCollection<ShapeBase> oVal ||
            e.NewValue is not ObservableCollection<ShapeBase> nVal) return;

        // update
        oVal.ForEach(item => item?.Clear(ex.Canvas));
        nVal.ForEach(item => item?.Draw(ex.Canvas));
    }

    public ObservableCollection<ShapeBase> ShapeCollection
    {
        get => (ObservableCollection<ShapeBase>)GetValue(ShapeCollectionProperty);
        set => SetValue(ShapeCollectionProperty, value);
    }

    public static readonly DependencyProperty ShapeCollectionProperty = DependencyProperty.Register(
    nameof(ShapeCollection), typeof(ObservableCollection<ShapeBase>), typeof(ImageEx), new PropertyMetadata(new ObservableCollection<ShapeBase>(), OnShapeChanged));

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

    #region Render Size Info

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
        ex.Box.Height = ex.DefaultImageSize.Height * ex.ImagePanelScale;
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

    #endregion

}
