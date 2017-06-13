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
        long readBytes;
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
            var bufferLength = output.Length - readBytes;
            if (bufferLength == 0)
            {
                return;
            }

            var buffer = new byte[bufferLength];

            output.Seek(readBytes, SeekOrigin.Begin);

            output.Read(buffer, (int)readBytes, (int)bufferLength);

            var str = Encoding.UTF8.GetString(buffer);

            textReceivedSubject.OnNext(str);
            readBytes += bufferLength;
        }

        public IObservable<string> TextReceived => textReceivedSubject.AsObservable();
        public void Dispose()
        {
            updater?.Dispose();
        }
    }
}