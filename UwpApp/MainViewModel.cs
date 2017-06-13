using System;
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

        public MainViewModel()
        {
            Output = string.Empty;
            var client = new SshClient("raspberrypi", "pi", "raspberry");
            client.Connect();
            var output = new MemoryStream();

            var input = new MemoryStream();
            var textInput = new StreamWriter(input);

            reader = new OutputPoller(output);
            reader.TextReceived
                .ObserveOnDispatcher()
                .Subscribe(AddText);
            
            
            shell = client.CreateShell(input, output, new MemoryStream());
            shell.ErrorOccurred += ShellOnErrorOccurred;
            shell.Start();
            textInput.Write("ls\r\n");
            textInput.Flush();

            var hasSomeText = this.WhenAnyValue(model => model.SendText, s => !string.IsNullOrEmpty(s));
            
            ExecuteCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                return await Task.Run(() => client.RunCommand(SendText));
            }, hasSomeText);

            ExecuteCommand.Subscribe(cmd =>
            {
                AddText(cmd.Result);
                SendText = string.Empty;
            });

            isBusy = ExecuteCommand.IsExecuting.ToProperty(this, model => model.IsBusy);
        }

        public bool IsBusy => isBusy.Value;

        private void ShellOnErrorOccurred(object sender, ExceptionEventArgs exceptionEventArgs)
        {
            
        }

        private void AddText(string text)
        {
            Output = Output + "\n" + text;
        }

        public string Output
        {
            get { return output1; }
            set { this.RaiseAndSetIfChanged(ref output1, value); }
        }
        
        public ReactiveCommand<Unit, SshCommand> ExecuteCommand { get; set; }

        public string SendText
        {
            get { return sendText; }
            set { this.RaiseAndSetIfChanged(ref sendText, value); }
        }
    }
}