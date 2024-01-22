using System.Windows.Input;
using TwitchLeecher.Gui.Types;
using TwitchLeecher.Shared.Commands;

namespace TwitchLeecher.Gui.ViewModels;

public class MessageBoxViewModel : ViewModelBase
{
    private readonly Action<MessageBoxResult> _close;
    private string _message;
    private string _okButtonText;
    private string _cancelButtonText;
    private string _yesButtonText;
    private string _noButtonText;
    private string _mainIcon;
    private string _caption;
    private bool _yesButtonVisible;
    private bool _noButtonVisible;
    private bool _okButtonVisible;
    private bool _cancelButtonVisible;

    public ICommand YesCommand { get; set; }
    public ICommand NoCommand { get; set; }
    public ICommand OkCommand { get; set; }
    public ICommand CancelCommand { get; set; }

    public MessageBoxViewModel(Action<MessageBoxResult> close)
    {
        _close = close;
        YesCommand = new DelegateCommand(() => { Close(MessageBoxResult.Yes); });
        NoCommand = new DelegateCommand(() => { Close(MessageBoxResult.No); });
        OkCommand = new DelegateCommand(() => { Close(MessageBoxResult.Ok); });
        CancelCommand = new DelegateCommand(() => { Close(MessageBoxResult.Cancel); });
    }

    public MessageBoxViewModel()
    {
        YesCommand = new DelegateCommand(() => { Close(MessageBoxResult.Yes); });
        NoCommand = new DelegateCommand(() => { Close(MessageBoxResult.No); });
        OkCommand = new DelegateCommand(() => { Close(MessageBoxResult.Ok); });
        CancelCommand = new DelegateCommand(() => { Close(MessageBoxResult.Cancel); });
    }

    private void Close(MessageBoxResult result)
    {
        _close(result);
    }

    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public string OkButtonText
    {
        get => _okButtonText;
        set => SetProperty(ref _okButtonText, value);
    }

    public string CancelButtonText
    {
        get => _cancelButtonText;
        set => SetProperty(ref _cancelButtonText, value);
    }

    public string YesButtonText
    {
        get => _yesButtonText;
        set => SetProperty(ref _yesButtonText, value);
    }

    public string NoButtonText
    {
        get => _noButtonText;
        set => SetProperty(ref _noButtonText, value);
    }

    public string MainIcon
    {
        get => _mainIcon;
        set => SetProperty(ref _mainIcon, value);
    }

    public string Caption
    {
        get => _caption;
        set => SetProperty(ref _caption, value);
    }

    public bool YesButtonVisible
    {
        get => _yesButtonVisible;
        set => SetProperty(ref _yesButtonVisible, value);
    }

    public bool NoButtonVisible
    {
        get => _noButtonVisible;
        set => SetProperty(ref _noButtonVisible, value);
    }

    public bool OkButtonVisible
    {
        get => _okButtonVisible;
        set => SetProperty(ref _okButtonVisible, value);
    }

    public bool CancelButtonVisible
    {
        get => _cancelButtonVisible;
        set => SetProperty(ref _cancelButtonVisible, value);
    }

    public void SetIcon(MessageBoxImage image)
    {
        switch (image)
        {
            case MessageBoxImage.Warning:
                MainIcon = "fa-solid fa-triangle-exclamation";
                break;

            case MessageBoxImage.Error:
                MainIcon = "fa-solid fa-ban";
                break;

            case MessageBoxImage.Information:
                MainIcon = "fa-solid fa-circle-info";
                break;

            case MessageBoxImage.Question:
                MainIcon = "fa-solid fa-circle-question";
                break;

            default:
                MainIcon = "fa-solid fa-circle-info";
                break;
        }
    }

    public void SetButtons(MessageBoxButton buttons)
    {
        YesButtonVisible = false;
        NoButtonVisible = false;
        OkButtonVisible = false;
        CancelButtonVisible = false;
        switch (buttons)
        {
            case MessageBoxButton.OKCancel:
                OkButtonVisible = true;
                CancelButtonVisible = true;
                break;

            case MessageBoxButton.YesNo:
                YesButtonVisible = true;
                NoButtonVisible = true;
                break;

            case MessageBoxButton.YesNoCancel:
                YesButtonVisible = true;
                NoButtonVisible = true;
                CancelButtonVisible = true;
                break;

            default:
                OkButtonVisible = true;
                break;
        }
    }
}