using Lift.UI.Tools.Extension;
using Microsoft.Xaml.Behaviors;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
//using Test.ImageExtend.Extension;
using Test.ImageExtend.ImageEx.ShapeEx;

namespace Test.ImageExtend.ImageEx;

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

    private DateTime _flag = DateTime.Now;

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

    private bool _flag = false;

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
        if (!_flag) return;
        _flag = false;

        AssociatedObject.ShapePreviewer!.Visibility = Visibility.Collapsed;
        AssociatedObject.Canvas!.Cursor = Cursors.Arrow;

        // valid location
        if (!ValidLocation(e)) return;

        // valid size
        var min = Math.Min(AssociatedObject.ShapePreviewer!.Height, AssociatedObject.ShapePreviewer!.Width);
        var threshold = Math.Min(AssociatedObject.ImageSource!.Height, AssociatedObject.ImageSource!.Width) * 0.02;

        if (threshold > min) return;

        // render
        AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);

        // add shape
        //var rec = AssociatedObject.ShapePreviewer!.Clone();
        //AssociatedObject.ShapeCollection.Add(rec);
        AssociatedObject.ShapeMarker!.PointStart = AssociatedObject.ShapePreviewer!.PointStart;
        AssociatedObject.ShapeMarker!.PointEnd = AssociatedObject.ShapePreviewer!.PointEnd;
        AssociatedObject.ShapeMarker!.Visibility = Visibility.Visible;

        AssociatedObject.ShapeMarker!.Refresh();

        var marker = AssociatedObject.ShapeMarker;
        var start = marker.PointStart;
        var end = marker.PointEnd;

        var location = new Point(Math.Min(start.X, end.X), Math.Min(start.Y, end.Y));

        AssociatedObject.OnMarkderChanged
            ?.Invoke(new Rect(location.X, location.Y, marker.Width, marker.Height));
    }

    /// <summary>
    /// 处理在画布上的鼠标事件以绘制形状
    /// 按下时记录起始点
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasPreviewMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed
            || e.RightButton != MouseButtonState.Pressed) return;

        AssociatedObject.ShapePreviewer!.PointStart = e.GetPosition(AssociatedObject.Canvas);
    }

    /// <summary>
    /// 处理在画布上的鼠标事件以绘制形状
    /// 移动时更新预览形状
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void OnCanvasPreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed
            || e.RightButton != MouseButtonState.Pressed) return;

        AssociatedObject.ShapePreviewer!.PointEnd = e.GetPosition(AssociatedObject.Canvas);

        _flag = true;
        AssociatedObject.Canvas!.Cursor = Cursors.Cross;

        AssociatedObject.ShapePreviewer!.Visibility = Visibility.Visible;
        AssociatedObject.ShapePreviewer!.Refresh();
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

        return !(pos.X < 0 || pos.Y < 0 || pos.X > width || pos.Y > height);
    }
}

/// <summary>
/// 自定义的 WPF 控件，集成了图像显示、缩放、移动和标记功能
/// </summary>
[TemplatePart(Name = NamePartMainPanel, Type = typeof(Panel))]
[TemplatePart(Name = NamePartScrollView, Type = typeof(ScrollViewer))]
[TemplatePart(Name = NamePartViewBox, Type = typeof(Viewbox))]
[TemplatePart(Name = NamePartCanvas, Type = typeof(InkCanvas))]
[TemplatePart(Name = NamePartShapePreviewer, Type = typeof(ShapeBase))]
[TemplatePart(Name = NamePartShapeMarkder, Type = typeof(ShapeBase))]
public class ImageEx : ContentControl
{
    //定义控件模板中的各种部分，包括主面板、滚动视图、视图框、画布和形状预览器、标记器。
    #region Name

    public const string NamePartMainPanel = "PART_MAIN_PANEL";

    public const string NamePartScrollView = "PART_SCROLL";

    public const string NamePartViewBox = "PART_BOX";

    public const string NamePartCanvas = "PART_CANVAS";

    public const string NamePartShapePreviewer = "PART_SHAPE_PREVIEWER";

    public const string NamePartShapeMarkder = "PART_SHAPE_MARKER";

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
    /// </summary>
    public static readonly RoutedUICommand MarkerCommand = new();

    #endregion

    //定义标记变化和光标变化的事件
    #region Events

    /// <summary>
    /// 返回相对Image的坐标
    /// </summary>
    public Action<Rect>? OnMarkderChanged;

    public Action<(int X, int Y, Color C)>? OnCursorChanged;

    public Func<int, int, Color>? GetImageColorFromPosition;

    #endregion

    private readonly List<Behavior> _behaviors = new();

    /// <summary>
    /// 初始化行为类并添加到 _behaviors 列表中。为命令绑定处理程序
    /// </summary>
    static ImageEx()
    {
        DefaultStyleKeyProperty.OverrideMetadata(typeof(ImageEx)
            , new FrameworkPropertyMetadata(typeof(ImageEx)));
    }

    /// <summary>
    /// 初始化行为类并添加到 _behaviors 列表中。为命令绑定处理程序
    /// </summary>
    /// <exception cref="NotImplementedException"></exception>
    public ImageEx()
    {
        _behaviors.Add(new ImageExViewerBehavior());
        _behaviors.Add(new ImageExDrawBehavior());

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

        Canvas!.PreviewMouseMove += OnCanvasCursorChagned;
    }

    private void OnCanvasCursorChagned(object sender, MouseEventArgs e)
    {
        base.OnPreviewMouseMove(e);

        if (ImageSource is null || GetImageColorFromPosition is null) return;

        var pos = e.GetPosition(Canvas);
        var x = (int)Math.Floor(pos.X);
        var y = (int)Math.Floor(pos.Y);

        var c = GetImageColorFromPosition(x, y);
        OnCursorChanged?.Invoke((x, y, c));
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
        nameof(ImageSource), typeof(ImageSource), typeof(ImageEx), new PropertyMetadata(null,
            (o, p) =>
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

    /// <summary>
    /// 双击事件
    /// todo
    /// </summary>
    /// <param name="e"></param>
    protected override void OnMouseDoubleClick(MouseButtonEventArgs e)
    {
        var pos = e.GetPosition(Canvas);
    }
}
