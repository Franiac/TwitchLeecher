using System;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using Xceed.Wpf.Toolkit;

namespace TwitchLeecher.Gui.Controls
{
    public class IntegerLoopUpDown : IntegerUpDown
    {
        #region Fields

        private int maxLength;

        #endregion Fields

        #region Methods

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

                if (TextBox.SelectionLength == 0 && textLength == maxLength && caretIndex < textLength)
                {
                    e.Handled = true;
                    sb[caretIndex] = inputChar;
                    Text = sb.ToString();
                    TextBox.CaretIndex = caretIndex + 1;
                }
            }

            base.OnPreviewTextInput(e);
        }

        protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
        {
            int value = Value.GetValueOrDefault();
            value = Maximum.HasValue && value > Maximum.Value ? Maximum.Value : value;
            value = Minimum.HasValue && value < Minimum.Value ? Minimum.Value : value;
            Value = value;

            base.OnLostKeyboardFocus(e);
        }

        protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
        {
            if (TextBox.IsFocused)
            {
                e.Handled = true;

                int value = Value.GetValueOrDefault();
                value += e.Delta > 0 ? 1 : -1;
                value = Maximum.HasValue && Minimum.HasValue && value > Maximum.Value ? Minimum.Value : value;
                value = Minimum.HasValue && Maximum.HasValue && value < Minimum.Value ? Maximum.Value : value;
                Value = value;
            }

            base.OnMouseWheel(e);
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

            return Math.Abs(value).ToString().PadLeft(maxLength, '0');
        }

        #endregion Methods
    }
}