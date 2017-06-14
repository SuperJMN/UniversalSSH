using System;
using System.Reactive;
using System.Reactive.Linq;
using ReactiveUI;

namespace SSH
{
    public class MainViewModel : ReactiveObject, IDisposable
    {
        private string sendText;
        private string output;
        private readonly ObservableAsPropertyHelper<bool> isBusy;
        private readonly ShhClient sshClient;

        public MainViewModel()
        {
            sshClient = new ShhClient("raspberrypi", "pi", "raspberry");
            sshClient.TextReceived.ObserveOnDispatcher().Subscribe(AddText);
            sshClient.Connect();

            var hasSomeText = this.WhenAnyValue(model => model.SendText, s => !string.IsNullOrEmpty(s));

            ExecuteCommand = ReactiveCommand.Create(() =>
            {
                sshClient.SendText(SendText);
                SendText = string.Empty;
            }, hasSomeText);

            isBusy = ExecuteCommand.IsExecuting.ToProperty(this, model => model.IsBusy);
        }

        public bool IsBusy => isBusy.Value;

        private void AddText(string text)
        {
            Output = Output + text;
        }

        public string Output
        {
            get { return output; }
            set { this.RaiseAndSetIfChanged(ref output, value); }
        }

        public ReactiveCommand<Unit, Unit> ExecuteCommand { get; set; }

        public string SendText
        {
            get { return sendText; }
            set { this.RaiseAndSetIfChanged(ref sendText, value); }
        }

        public void Dispose()
        {
            isBusy?.Dispose();
            sshClient?.Dispose();
            ExecuteCommand?.Dispose();
        }
    }
}