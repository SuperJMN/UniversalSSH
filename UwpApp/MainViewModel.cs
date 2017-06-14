using System;
using System.Collections.Generic;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace SSH
{
    public class MainViewModel : ReactiveObject
    {
        private string sendText;
        private Shell shell;
        private string output1;
        private OutputPoller reader;
        private ObservableAsPropertyHelper<bool> isBusy;
        private OutputPoller extReader;

        public MainViewModel()
        {
            Output = string.Empty;
            var client = new SshClient("raspberrypi", "pi", "raspberry");
            client.Connect();
            var output = new PipeStream();

            var input = new PipeStream();
            var textInput = new StreamWriter(input) { AutoFlush = true };

            reader = new OutputPoller(output);
            reader.TextReceived
                .ObserveOnDispatcher()
                .Subscribe(AddText);

            var extendedOutput = new PipeStream();

            extReader = new OutputPoller(extendedOutput);
            extReader.TextReceived
                .ObserveOnDispatcher()
                .Subscribe(_ => { });

            var terminalModes = new Dictionary<TerminalModes, uint>() { {TerminalModes.IUCLC, 1}};
            
            shell = client.CreateShell(input, output, extendedOutput, "builtin_xterm", 80, 80, 100, 120, terminalModes);
            shell.ErrorOccurred += ShellOnErrorOccurred;
            shell.Start();

            var hasSomeText = this.WhenAnyValue(model => model.SendText, s => !string.IsNullOrEmpty(s));

            ExecuteCommand = ReactiveCommand.Create(() =>
            {
                textInput.WriteLine(SendText);
                SendText = string.Empty;
            }, hasSomeText);

            isBusy = ExecuteCommand.IsExecuting.ToProperty(this, model => model.IsBusy);
        }

        public bool IsBusy => isBusy.Value;

        private void ShellOnErrorOccurred(object sender, ExceptionEventArgs exceptionEventArgs)
        {

        }

        private void AddText(string text)
        {
            Output = Output + text;
        }

        public string Output
        {
            get { return output1; }
            set { this.RaiseAndSetIfChanged(ref output1, value); }
        }

        public ReactiveCommand<Unit, Unit> ExecuteCommand { get; set; }

        public string SendText
        {
            get { return sendText; }
            set { this.RaiseAndSetIfChanged(ref sendText, value); }
        }
    }
}