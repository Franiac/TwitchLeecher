using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace TwitchLeecher.Gui.Controls
{
    public class TlIntegerUpDown : IntegerUpDown
    {
        #region Fields

        private int maxLength;

        #endregion Fields

        #region Dependency Properties

        #region PadZeros

        public static readonly DependencyProperty PadZerosProperty = DependencyProperty.Register(
                nameof(PadZeros),
                typeof(bool),
                typeof(TlIntegerUpDown), new FrameworkPropertyMetadata(defaultValue: false));

        public bool PadZeros
        {
            get { return (bool)GetValue(PadZerosProperty); }
            set { SetValue(PadZerosProperty, value); }
        }

        #endregion PadZeros

        #region Loop

        public static readonly DependencyProperty LoopProperty = DependencyProperty.Register(
                nameof(Loop),
                typeof(bool),
                typeof(TlIntegerUpDown), new FrameworkPropertyMetadata(defaultValue: false));

        public bool Loop
        {
            get { return (bool)GetValue(LoopProperty); }
            set { SetValue(LoopProperty, value); }
        }

        #endregion Loop

        #endregion Dependency Properties

        #region Methods

        protected void FocusAndSelectAll()
        {
            if (!TextBox.IsFocused)
            {
                TextBox.Focus();
            }

            TextBox.SelectAll();
        }

        protected override void OnInitialized(EventArgs e)
        {
            maxLength = Maximum.HasValue ? Maximum.ToString().Length : int.MaxValue;

            base.OnInitialized(e);

            DataObject.AddPastingHandler(this, (s, args) => { args.CancelCommand(); });
        }

        protected override void OnPreviewTextInput(TextCompositionEventArgs e)
        {
            string input = e.Text;

            if (string.IsNullOrWhiteSpace(input) || input.Length < 1 || !Regex.Match(input, "[0-9]").Success)
            {
                e.Handled = true;
            }
            else
            {
                char inputChar = input[0];

                StringBuilder sb = new StringBuilder(Text);

                int caretIndex = TextBox.CaretIndex;
                int textLength = sb.Length;

                if (TextBox.SelectionLength == 0 && textLength == maxLength)
                {
                    e.Handled = true;

                    if (caretIndex < textLength)
                    {
                        sb[caretIndex] = inputChar;
                        Text = sb.ToString();
                        TextBox.CaretIndex = caretIndex + 1;
                    }
                }
            }

            base.OnPreviewTextInput(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            SetValue(Value.GetValueOrDefault());
            base.OnLostKeyboardFocus(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (TextBox.IsFocused)
            {
                SetValue(Value.GetValueOrDefault() + (e.Delta > 0 ? 1 : -1));
                e.Handled = true;
            }

            base.OnMouseWheel(e);
        }

        protected override void SetValidSpinDirection()
        {
            if (Loop)
            {
                if (Spinner != null)
                {
                    ValidSpinDirections validSpinDirections = ValidSpinDirections.None;

                    if (Increment.HasValue && !IsReadOnly)
                    {
                        validSpinDirections |= ValidSpinDirections.Increase;
                        validSpinDirections |= ValidSpinDirections.Decrease;
                    }

                    Spinner.ValidSpinDirection = validSpinDirections;
                }
            }
            else
            {
                base.SetValidSpinDirection();
            }
        }

        protected override void OnIncrement()
        {
            if (Loop && Increment.HasValue)
            {
                SetValue(Value.GetValueOrDefault() + Increment.Value);
            }
            else
            {
                base.OnIncrement();
            }

            FocusAndSelectAll();
        }

        protected override void OnDecrement()
        {
            if (Loop && Increment.HasValue)
            {
                SetValue(Value.GetValueOrDefault() - Increment.Value);
            }
            else
            {
                base.OnDecrement();
            }

            FocusAndSelectAll();
        }

        protected void SetValue(int value)
        {
            if (Loop)
            {
                value = Maximum.HasValue && Minimum.HasValue && value > Maximum.Value ? Minimum.Value : value;
                value = Minimum.HasValue && Maximum.HasValue && value < Minimum.Value ? Maximum.Value : value;
            }

            value = Maximum.HasValue && value > Maximum.Value ? Maximum.Value : value;
            value = Minimum.HasValue && value < Minimum.Value ? Minimum.Value : value;

            Value = value;

            FocusAndSelectAll();
        }

        protected override int? ConvertTextToValue(string text)
        {
            if (!string.IsNullOrWhiteSpace(text))
            {
                return int.TryParse(text.TrimStart('0'), out int result) ? result : 0; ;
            }
            else
            {
                return 0;
            }
        }

        protected override string ConvertValueToText()
        {
            int value = Value.GetValueOrDefault();

            if (Maximum.HasValue && PadZeros)
            {
                return Math.Abs(value).ToString().PadLeft(maxLength, '0');
            }
            else
            {
                return value.ToString();
            }
        }

        #endregion Methods
    }
}