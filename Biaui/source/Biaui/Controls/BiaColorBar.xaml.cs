﻿using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using Biaui.Internals;

namespace Biaui.Controls
{
    public class BiaColorBar : FrameworkElement
    {
        #region Value

        public double Value
        {
            get => _Value;
            set
            {
                if (NumberHelper.AreClose(value, _Value) == false)
                    SetValue(ValueProperty, Boxes.Double(value));
            }
        }

        private double _Value;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(BiaColorBar),
                new FrameworkPropertyMetadata(
                    Boxes.Double0,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (BiaColorBar) s;
                        self._Value = (double) e.NewValue;
                    }));

        #endregion

        #region Color0

        public ByteColor Color0
        {
            get => _Color0;
            set
            {
                if (value != _Color0)
                    SetValue(Color0Property, value);
            }
        }

        private ByteColor _Color0 = ByteColor.Black;

        public static readonly DependencyProperty Color0Property =
            DependencyProperty.Register(nameof(Color0), typeof(ByteColor), typeof(BiaColorBar),
                new FrameworkPropertyMetadata(
                    Boxes.ByteColorBlack,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (BiaColorBar) s;
                        self._Color0 = (ByteColor) e.NewValue;
                        self._isRequestUpdateBackgroundBrush = true;
                    }));

        #endregion

        #region Color1

        public ByteColor Color1
        {
            get => _Color1;
            set
            {
                if (value != _Color1)
                    SetValue(Color1Property, value);
            }
        }

        private ByteColor _Color1 = ByteColor.White;

        public static readonly DependencyProperty Color1Property =
            DependencyProperty.Register(nameof(Color1), typeof(ByteColor), typeof(BiaColorBar),
                new FrameworkPropertyMetadata(
                    Boxes.ByteColorWhite,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (BiaColorBar) s;
                        self._Color1 = (ByteColor) e.NewValue;
                        self._isRequestUpdateBackgroundBrush = true;
                    }));

        #endregion

        #region BorderColor

        public ByteColor BorderColor
        {
            get => _BorderColor;
            set
            {
                if (value != _BorderColor)
                    SetValue(BorderColorProperty, value);
            }
        }

        private ByteColor _BorderColor = ByteColor.Red;

        public static readonly DependencyProperty BorderColorProperty =
            DependencyProperty.Register(nameof(BorderColor), typeof(ByteColor), typeof(BiaColorBar),
                new FrameworkPropertyMetadata(
                    Boxes.ByteColorRed,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (BiaColorBar) s;
                        self._BorderColor = (ByteColor) e.NewValue;
                    }));

        #endregion

        #region IsInverseValue

        public bool IsInverseValue
        {
            get => _IsInverseValue;
            set
            {
                if (value != _IsInverseValue)
                    SetValue(IsInverseValueProperty, Boxes.Bool(value));
            }
        }

        private bool _IsInverseValue;

        public static readonly DependencyProperty IsInverseValueProperty =
            DependencyProperty.Register(nameof(IsInverseValue), typeof(bool), typeof(BiaColorBar),
                new FrameworkPropertyMetadata(
                    Boxes.BoolFalse,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (BiaColorBar) s;
                        self._IsInverseValue = (bool) e.NewValue;
                    }));

        #endregion

        #region IsReadOnly

        public bool IsReadOnly
        {
            get => _IsReadOnly;
            set
            {
                if (value != _IsReadOnly)
                    SetValue(IsReadOnlyProperty, Boxes.Bool(value));
            }
        }

        private bool _IsReadOnly;

        public static readonly DependencyProperty IsReadOnlyProperty =
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(BiaColorBar),
                new FrameworkPropertyMetadata(
                    Boxes.BoolFalse,
                    FrameworkPropertyMetadataOptions.AffectsRender |
                    FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender,
                    (s, e) =>
                    {
                        var self = (BiaColorBar) s;
                        self._IsReadOnly = (bool) e.NewValue;
                    }));

        #endregion

        #region StartedContinuousEditingCommand

        public ICommand? StartedContinuousEditingCommand
        {
            get => _StartedContinuousEditingCommand;
            set
            {
                if (value != _StartedContinuousEditingCommand)
                    SetValue(StartedContinuousEditingCommandProperty, value);
            }
        }

        private ICommand? _StartedContinuousEditingCommand;

        public static readonly DependencyProperty StartedContinuousEditingCommandProperty =
            DependencyProperty.Register(
                nameof(StartedContinuousEditingCommand),
                typeof(ICommand),
                typeof(BiaColorBar),
                new PropertyMetadata(
                    default(ICommand),
                    (s, e) =>
                    {
                        var self = (BiaColorBar) s;
                        self._StartedContinuousEditingCommand = (ICommand) e.NewValue;
                    }));

        #endregion

        #region EndContinuousEditingCommand

        public ICommand? EndContinuousEditingCommand
        {
            get => _EndContinuousEditingCommand;
            set
            {
                if (value != _EndContinuousEditingCommand)
                    SetValue(EndContinuousEditingCommandProperty, value);
            }
        }

        private ICommand? _EndContinuousEditingCommand;

        public static readonly DependencyProperty EndContinuousEditingCommandProperty =
            DependencyProperty.Register(
                nameof(EndContinuousEditingCommand),
                typeof(ICommand),
                typeof(BiaColorBar),
                new PropertyMetadata(
                    default(ICommand),
                    (s, e) =>
                    {
                        var self = (BiaColorBar) s;
                        self._EndContinuousEditingCommand = (ICommand) e.NewValue;
                    }));

        #endregion

        private Brush? _backgroundBrush;
        private bool _isRequestUpdateBackgroundBrush = true;

        // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
        private readonly PropertyChangeNotifier _isEnabledChangeNotifier;

        static BiaColorBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BiaColorBar),
                new FrameworkPropertyMetadata(typeof(BiaColorBar)));
        }

        public BiaColorBar()
        {
            RenderOptions.SetEdgeMode(this, EdgeMode.Aliased);

            _isEnabledChangeNotifier = new PropertyChangeNotifier(this, IsEnabledProperty);
            _isEnabledChangeNotifier.ValueChanged += (_, __) =>
            {
                _isRequestUpdateBackgroundBrush = true;
                InvalidateVisual();
            };
        }

        protected override void OnRender(DrawingContext dc)
        {
            if (ActualWidth <= 1 ||
                ActualHeight <= 1)
                return;

            VisualEdgeMode = EdgeMode.Unspecified;

            if (_isRequestUpdateBackgroundBrush)
            {
                _isRequestUpdateBackgroundBrush = false;
                UpdateBackgroundBrush();
            }

            var rect = this.RoundLayoutRenderRectangle(true);
            dc.DrawRectangle(_backgroundBrush, this.GetBorderPen(BorderColor), rect);

            // Cursor
            this.DrawPointCursor(dc, CursorRenderPos, IsEnabled, IsReadOnly);
        }

        private ImmutableVec2_double CursorRenderPos
        {
            get
            {
                var bw = this.RoundLayoutValue(FrameworkElementExtensions.BorderWidth);

                var h = this.RoundLayoutValue(ActualHeight - bw * 2);
                var y = NumberHelper.Clamp01(Value) * h;

                if (IsInverseValue)
                    y = h - y;

                y += bw;

                return new ImmutableVec2_double(this.RoundLayoutValue(ActualWidth / 2), this.RoundLayoutValue(y));
            }
        }

        private static Brush? _disabledBackgroundBrush;

        private void UpdateBackgroundBrush()
        {
            if (IsEnabled)
            {
                _backgroundBrush = new LinearGradientBrush(Color1.ToColor(), Color0.ToColor(), 90);
                _backgroundBrush.Freeze();
            }
            else
            {
                if (_disabledBackgroundBrush == null)
                    _disabledBackgroundBrush = (Brush) TryFindResource("InactiveBackgroundBrushKey");

                _backgroundBrush = _disabledBackgroundBrush;
            }
        }

        private void UpdateParams(MouseEventArgs e)
        {
            var pos = e.GetPosition(this);

            var s = this.RoundLayoutValue(1);
            var y = (pos.Y - s) / (ActualHeight - s * 2);
            y = NumberHelper.Clamp01(y);

            Value = IsInverseValue ? 1 - y : y;
        }

        private bool _isMouseDown;
        private bool _isContinuousEdited;
        private double _ContinuousEditingStartValue;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            if (IsReadOnly)
                return;

            _isMouseDown = true;
            GuiHelper.HideCursor();

            _ContinuousEditingStartValue = Value;
            _isContinuousEdited = true;
            StartedContinuousEditingCommand?.ExecuteIfCan(null);

            UpdateParams(e);

            CaptureMouse();

            this.SetMouseClipping();

            e.Handled = true;
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsReadOnly)
                return;

            if (_isMouseDown == false)
                return;

            UpdateParams(e);

            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (IsReadOnly)
                return;

            if (_isMouseDown == false)
                return;

            // マウス位置を補正する
            {
                var cp = CursorRenderPos;
                var p = PointToScreen(Unsafe.As<ImmutableVec2_double, Point>(ref cp));
                Win32Helper.SetCursorPos((int) p.X, (int) p.Y);
            }

            _isMouseDown = false;
            this.ResetMouseClipping();
            GuiHelper.ShowCursor();
            ReleaseMouseCapture();

            if (_isContinuousEdited)
            {
                if (EndContinuousEditingCommand != null)
                {
                    if (EndContinuousEditingCommand.CanExecute(null))
                    {
                        var changedValue = Value;
                        Value = _ContinuousEditingStartValue;

                        EndContinuousEditingCommand.Execute(null);

                        Value = changedValue;
                    }
                }

                _isContinuousEdited = false;
            }

            e.Handled = true;
        }

        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);

            if (_isMouseDown)
            {
                _isMouseDown = false;
                ReleaseMouseCapture();
                GuiHelper.ShowCursor();
                this.ResetMouseClipping();
            }

            e.Handled = true;
        }
    }
}