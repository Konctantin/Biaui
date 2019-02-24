﻿using System;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Biaui.Internals;

namespace Biaui.Controls
{
    public class BiaColorPicker : Control
    {
        #region Red

        public double Red
        {
            get => _Red;
            set
            {
                if (NumberHelper.AreClose(value, _Red) == false)
                    SetValue(RedProperty, value);
            }
        }

        private double _Red;

        public static readonly DependencyProperty RedProperty =
            DependencyProperty.Register(nameof(Red), typeof(double), typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.Double0,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._Red = (double) e.NewValue;
                        self.RgbToHsv();
                    }));

        #endregion

        #region Green

        public double Green
        {
            get => _Green;
            set
            {
                if (NumberHelper.AreClose(value, _Green) == false)
                    SetValue(GreenProperty, value);
            }
        }

        private double _Green;

        public static readonly DependencyProperty GreenProperty =
            DependencyProperty.Register(nameof(Green), typeof(double), typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.Double0,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._Green = (double) e.NewValue;
                        self.RgbToHsv();
                    }));

        #endregion

        #region Blue

        public double Blue
        {
            get => _Blue;
            set
            {
                if (NumberHelper.AreClose(value, _Blue) == false)
                    SetValue(BlueProperty, value);
            }
        }

        private double _Blue;

        public static readonly DependencyProperty BlueProperty =
            DependencyProperty.Register(nameof(Blue), typeof(double), typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.Double0,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._Blue = (double) e.NewValue;
                        self.RgbToHsv();
                    }));

        #endregion

        #region Alpha

        public double Alpha
        {
            get => _Alpha;
            set
            {
                if (NumberHelper.AreClose(value, _Alpha) == false)
                    SetValue(AlphaProperty, value);
            }
        }

        private double _Alpha;

        public static readonly DependencyProperty AlphaProperty =
            DependencyProperty.Register(nameof(Alpha), typeof(double), typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.Double0,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._Alpha = (double) e.NewValue;
                    }));

        #endregion

        #region IsVisibleAlphaEditor

        public bool IsVisibleAlphaEditor
        {
            get => _IsVisibleAlphaEditor;
            set
            {
                if (value != _IsVisibleAlphaEditor)
                    SetValue(IsVisibleAlphaEditorProperty, value);
            }
        }

        private bool _IsVisibleAlphaEditor = true;

        public static readonly DependencyProperty IsVisibleAlphaEditorProperty =
            DependencyProperty.Register(
                nameof(IsVisibleAlphaEditor),
                typeof(bool),
                typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.BoolTrue,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._IsVisibleAlphaEditor = (bool) e.NewValue;
                    }));

        #endregion

        #region Hue

        public double Hue
        {
            get => _Hue;
            set
            {
                if (NumberHelper.AreClose(value, _Hue) == false)
                    SetValue(HueProperty, value);
            }
        }

        private double _Hue;

        public static readonly DependencyProperty HueProperty =
            DependencyProperty.Register(nameof(Hue), typeof(double), typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.Double0,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._Hue = (double) e.NewValue;
                        self.HsvToRgb();
                    }));

        #endregion

        #region Saturation

        public double Saturation
        {
            get => _Saturation;
            set
            {
                if (NumberHelper.AreClose(value, _Saturation) == false)
                    SetValue(SaturationProperty, value);
            }
        }

        private double _Saturation;

        public static readonly DependencyProperty SaturationProperty =
            DependencyProperty.Register(nameof(Saturation), typeof(double), typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.Double0,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._Saturation = (double) e.NewValue;
                        self.HsvToRgb();
                    }));

        #endregion

        #region Value

        public double Value
        {
            get => _Value;
            set
            {
                if (NumberHelper.AreClose(value, _Value) == false)
                    SetValue(ValueProperty, value);
            }
        }

        private double _Value;

        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register(nameof(Value), typeof(double), typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.Double0,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._Value = (double) e.NewValue;
                        self.HsvToRgb();
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
            DependencyProperty.Register(nameof(IsReadOnly), typeof(bool), typeof(BiaColorPicker),
                new PropertyMetadata(
                    Boxes.BoolFalse,
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._IsReadOnly = (bool) e.NewValue;
                    }));

        #endregion

        #region StartedContinuousEditingCommand

        public ICommand StartedContinuousEditingCommand
        {
            get => _StartedContinuousEditingCommand;
            set
            {
                if (value != _StartedContinuousEditingCommand)
                    SetValue(StartedContinuousEditingCommandProperty, value);
            }
        }

        private ICommand _StartedContinuousEditingCommand;

        public static readonly DependencyProperty StartedContinuousEditingCommandProperty =
            DependencyProperty.Register(
                nameof(StartedContinuousEditingCommand),
                typeof(ICommand),
                typeof(BiaColorPicker),
                new PropertyMetadata(
                    default(ICommand),
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._StartedContinuousEditingCommand = (ICommand) e.NewValue;
                    }));

        #endregion

        #region EndContinuousEditingCommand

        public ICommand EndContinuousEditingCommand
        {
            get => _EndContinuousEditingCommand;
            set
            {
                if (value != _EndContinuousEditingCommand)
                    SetValue(EndContinuousEditingCommandProperty, value);
            }
        }

        private ICommand _EndContinuousEditingCommand;

        public static readonly DependencyProperty EndContinuousEditingCommandProperty =
            DependencyProperty.Register(
                nameof(EndContinuousEditingCommand),
                typeof(ICommand),
                typeof(BiaColorPicker),
                new PropertyMetadata(
                    default(ICommand),
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._EndContinuousEditingCommand = (ICommand) e.NewValue;
                    }));

        #endregion

        #region StartedBatchEditingCommand

        public ICommand StartedBatchEditingCommand
        {
            get => _StartedBatchEditingCommand;
            set
            {
                if (value != _StartedBatchEditingCommand)
                    SetValue(StartedBatchEditingCommandProperty, value);
            }
        }

        private ICommand _StartedBatchEditingCommand;

        public static readonly DependencyProperty StartedBatchEditingCommandProperty =
            DependencyProperty.Register(
                nameof(StartedBatchEditingCommand),
                typeof(ICommand),
                typeof(BiaColorPicker),
                new PropertyMetadata(
                    default(ICommand),
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._StartedBatchEditingCommand = (ICommand) e.NewValue;
                    }));

        #endregion

        #region EndBatchEditingCommand

        public ICommand EndBatchEditingCommand
        {
            get => _EndBatchEditingCommand;
            set
            {
                if (value != _EndBatchEditingCommand)
                    SetValue(EndBatchEditingCommandProperty, value);
            }
        }

        private ICommand _EndBatchEditingCommand;

        public static readonly DependencyProperty EndBatchEditingCommandProperty =
            DependencyProperty.Register(
                nameof(EndBatchEditingCommand),
                typeof(ICommand),
                typeof(BiaColorPicker),
                new PropertyMetadata(
                    default(ICommand),
                    (s, e) =>
                    {
                        var self = (BiaColorPicker) s;
                        self._EndBatchEditingCommand = (ICommand) e.NewValue;
                    }));

        #endregion

        static BiaColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(BiaColorPicker),
                new FrameworkPropertyMetadata(typeof(BiaColorPicker)));
        }

        public BiaColorPicker()
        {
            SizeChanged += (_, e) =>
            {
                if (_redEditor == null)
                    return;

                if (e.NewSize.Width > 300)
                {
                    _redEditor.Caption = "Red";
                    _greenEditor.Caption = "Green";
                    _blueEditor.Caption = "Blue";
                    _alphaEditor.Caption = "Alpha";
                    _hueEditor.Caption = "Hue";
                    _saturationEditor.Caption = "Saturation";
                    _valueEditor.Caption = "Value";
                }
                else
                {
                    _redEditor.Caption = "R";
                    _greenEditor.Caption = "G";
                    _blueEditor.Caption = "B";
                    _alphaEditor.Caption = "A";
                    _hueEditor.Caption = "H";
                    _saturationEditor.Caption = "S";
                    _valueEditor.Caption = "V";
                }
            };
        }

        public override void OnApplyTemplate()
        {
            foreach (var e in this.Descendants<BiaNumberEditor>())
            {
                switch (e.Name)
                {
                    case "RedEditor":
                        _redEditor = e;
                        break;

                    case "GreenEditor":
                        _greenEditor = e;
                        break;

                    case "BlueEditor":
                        _blueEditor = e;
                        break;

                    case "AlphaEditor":
                        _alphaEditor = e;
                        break;

                    case "HueEditor":
                        _hueEditor = e;
                        break;

                    case "SaturationEditor":
                        _saturationEditor = e;
                        break;

                    case "ValueEditor":
                        _valueEditor = e;
                        break;

                    default:
                        throw new Exception();
                }
            }

            _valueBar = this.Descendants<BiaColorBar>().First();

            SetContinuousEditingCommand();
        }

        private BiaNumberEditor _redEditor;
        private BiaNumberEditor _greenEditor;
        private BiaNumberEditor _blueEditor;
        private BiaNumberEditor _alphaEditor;

        private BiaNumberEditor _hueEditor;
        private BiaNumberEditor _saturationEditor;
        private BiaNumberEditor _valueEditor;

        private BiaColorBar _valueBar;

        private bool _isConverting;

        private void RgbToHsv()
        {
            if (_isConverting)
                return;

            _isConverting = true;

            if (StartedBatchEditingCommand != null &&
                StartedBatchEditingCommand.CanExecute(null))
                StartedBatchEditingCommand.Execute(null);

            (Hue, Saturation, Value) = ColorSpaceHelper.RgbToHsv(Red, Green, Blue);

            if (EndBatchEditingCommand != null &&
                EndBatchEditingCommand.CanExecute(null))
                EndBatchEditingCommand.Execute(null);

            _isConverting = false;
        }

        private void HsvToRgb()
        {
            if (_isConverting)
                return;

            _isConverting = true;

            if (StartedBatchEditingCommand != null &&
                StartedBatchEditingCommand.CanExecute(null))
                StartedBatchEditingCommand.Execute(null);

            (Red, Green, Blue) = ColorSpaceHelper.HsvToRgb(Hue, Saturation, Value);

            if (EndBatchEditingCommand != null &&
                EndBatchEditingCommand.CanExecute(null))
                EndBatchEditingCommand.Execute(null);

            _isConverting = false;
        }

        private void SetContinuousEditingCommand()
        {
            int editingDepth = 0;

            (double, double, double) continuousEditingStartValue = default;

            var started = new DelegateCommand(() =>
            {
                if (editingDepth == 0)
                {
                    continuousEditingStartValue = (Red, Green, Blue);

                    if (StartedContinuousEditingCommand != null)
                        if (StartedContinuousEditingCommand.CanExecute(null))
                            StartedContinuousEditingCommand.Execute(null);
                }

                ++editingDepth;
            });

            var end = new DelegateCommand(() =>
            {
                Debug.Assert(editingDepth > 0);

                --editingDepth;
                if (editingDepth == 0)
                {
                    if (EndContinuousEditingCommand != null)
                    {
                        if (EndContinuousEditingCommand.CanExecute(null))
                        {
                            var changedValue = (Red, Green, Blue);

                            (Red, Green, Blue) = continuousEditingStartValue;

                            EndContinuousEditingCommand.Execute(null);

                            (Red, Green, Blue) = changedValue;
                        }
                    }
                }
            });

            _redEditor.StartedContinuousEditingCommand = started;
            _greenEditor.StartedContinuousEditingCommand = started;
            _blueEditor.StartedContinuousEditingCommand = started;
            _alphaEditor.StartedContinuousEditingCommand = started;
            _hueEditor.StartedContinuousEditingCommand = started;
            _saturationEditor.StartedContinuousEditingCommand = started;
            _valueEditor.StartedContinuousEditingCommand = started;
            _valueBar.StartedContinuousEditingCommand = started;

            _redEditor.EndContinuousEditingCommand = end;
            _greenEditor.EndContinuousEditingCommand = end;
            _blueEditor.EndContinuousEditingCommand = end;
            _alphaEditor.EndContinuousEditingCommand = end;
            _hueEditor.EndContinuousEditingCommand = end;
            _saturationEditor.EndContinuousEditingCommand = end;
            _valueEditor.EndContinuousEditingCommand = end;
            _valueBar.EndContinuousEditingCommand = end;
        }


        private class DelegateCommand : ICommand
        {
            private readonly Action _execute;

            public DelegateCommand(Action execute)
            {
                _execute = execute;
            }

            public event EventHandler CanExecuteChanged
            {
                add => CommandManager.RequerySuggested += value;
                remove => CommandManager.RequerySuggested -= value;
            }

            bool ICommand.CanExecute(object parameter) => true;
            void ICommand.Execute(object parameter) => _execute?.Invoke();
        }
    }
}