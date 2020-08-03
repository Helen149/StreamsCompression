using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Streams.Compression
{
    public class CustomCompressionStream : Stream
    {
        private bool read;
        private Stream baseStream;
        private List<byte> readBuffer = new List<byte>();

        public CustomCompressionStream(Stream baseStream, bool read)
        {
            this.read = read;  
            this.baseStream = baseStream;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            CheckСonditionStream(u=>u);
            CheckLengthBaseSTream();
            ReadFromBaseStream(count);
            count = readBuffer.Count > count ? count : readBuffer.Count;
            readBuffer.CopyTo(0, buffer, offset, count);
            readBuffer.RemoveRange(0, count);
            return count;
        }

        private void ReadFromBaseStream(int count)
        {
            var sizeSteam = count % 2 == 0 ? count : count - 1;
            while (true)
            {
                if (sizeSteam == 0 || readBuffer.Count > count)
                    break;
                var data = new byte[sizeSteam];
                sizeSteam = baseStream.Read(data, 0, sizeSteam);
                if (sizeSteam % 2 != 0)
                    sizeSteam += baseStream.Read(data, sizeSteam, 1);

                for (int i = 0; i < sizeSteam - 1; i = i + 2)
                    if (data[i] != 0 || data[i + 1] != 0)
                        ReadCompressionByte(data[i], data[i + 1]);
            }
        }

        private void CheckLengthBaseSTream()
        {
            if (baseStream.Length % 2 != 0)
                throw new InvalidOperationException();
        }

        private void CheckСonditionStream(Func<bool, bool> isNeedCondition)
        {
            if(!isNeedCondition(read))
                throw new NotSupportedException();
        }

        private void ReadCompressionByte(byte countRepeat, byte dataRepeat)
        {
            for(;countRepeat!=0; countRepeat--)
                readBuffer.Add(dataRepeat);
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            CheckСonditionStream(u => !u);
            var resultBuffer = new List<byte>();
            byte dataRepeat = buffer.ElementAtOrDefault(offset);
            byte countRepeat = 0;
            for(int i=offset; i<count+offset;i++)
            {
                if (buffer[i] == dataRepeat)
                {
                    if (countRepeat == 255)
                        WriteCompressionByte(resultBuffer, ref countRepeat, dataRepeat);
                    else
                        countRepeat++;
                }
                else
                {
                    WriteCompressionByte(resultBuffer, ref countRepeat, dataRepeat);
                    dataRepeat = buffer[i];
                }
            }
            WriteCompressionByte(resultBuffer, ref countRepeat, dataRepeat);
            baseStream.Write(resultBuffer.ToArray(), 0, resultBuffer.Count);
        }

        private void WriteCompressionByte(List<byte> buffer, ref byte countRepeat, byte dataRepeat)
        {
            buffer.Add(countRepeat);
            buffer.Add(dataRepeat);
            countRepeat = 1;
        }

        public override bool CanRead { get { return read; } }

        public override bool CanSeek { get { return false; } }

        public override bool CanWrite { get { return !read; } }

        public override long Length => throw new NotSupportedException();

        public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }

        public override void Flush()
        {

        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(long value)
        {
            throw new NotSupportedException();
        }
    }
}
