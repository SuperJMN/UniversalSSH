using System;
using System.IO;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;

namespace SSH
{
    public class OutputPoller : IDisposable
    {
        private readonly Stream output;
        private readonly ISubject<string> textReceivedSubject = new Subject<string>();
        private readonly IDisposable updater;

        public OutputPoller(Stream output)
        {
            this.output = output;

            updater = Observable
                .Timer(TimeSpan.FromSeconds(1))
                .Repeat()
                .Subscribe(_ => Read());
        }

        private void Read()
        {
            var bufflen = output.Length - output.Position;
            var buffer = new byte[bufflen];
            output.Read(buffer, (int) output.Position, (int)bufflen);

            var str = Encoding.UTF8.GetString(buffer);
            if (str != string.Empty)
            {
                textReceivedSubject.OnNext(str);
            }
        }

        public IObservable<string> TextReceived => textReceivedSubject.AsObservable();
        public void Dispose()
        {
            updater?.Dispose();
        }
    }
}