using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Renci.SshNet;
using Renci.SshNet.Common;

namespace SSH
{
    public class ShhClient : IDisposable
    {
        private readonly SshClient client;
        private Shell shell;
        private StreamPoller outputReader;
        private readonly ISubject<string> textReceivedSubject = new Subject<string>();
        private IDisposable textUpdater;
        private readonly StreamWriter outputStreamWriter;
        private readonly Stream extendedOuput = new PipeStream();
        private readonly Stream output = new PipeStream();
        private readonly Stream input = new PipeStream();

        public ShhClient(string host, string username, string password, int port = 22)
        {
            client = new SshClient(host, username, password);
            outputStreamWriter = new StreamWriter(input) { AutoFlush = true };
        }

        public void Connect()
        {
            client.Connect();
            shell = client.CreateShell(input, output, extendedOuput);
            shell.Start();
            outputReader = new StreamPoller(output);
            textUpdater = outputReader.TextReceived.Subscribe(textReceivedSubject);
        }

        public void SendText(string text)
        {
            outputStreamWriter.WriteLine(text);
        }

        public IObservable<string> TextReceived => textReceivedSubject.AsObservable();

        public void Dispose()
        {
            client?.Dispose();
            shell?.Dispose();
            outputReader?.Dispose();
            textUpdater?.Dispose();
            outputStreamWriter?.Dispose();
            extendedOuput?.Dispose();
            output?.Dispose();
            input?.Dispose();
        }
    }
}