using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
namespace MockFunctionalityTests
{
    internal class OutputCaptureTextWriter:StringWriter
    {
        public override void Write(bool value)
        {
            Debug.Write(value);
            base.Write(value);
        }
        public override void Write(char value)
        {
            Debug.Write(value);
            base.Write(value);
        }
        public override void Write(char[] buffer, int index, int count)
        {
            Debug.Write(new Span<char>(buffer, index, count).ToString());
            base.Write(buffer, index, count);
        }
        public override void Write(ReadOnlySpan<char> buffer)
        {
            Debug.Write(buffer.ToString());
            base.Write(buffer);
        }
        public override void Write(string? value)
        {
            Debug.Write(value);
            base.Write(value);
        }
        public override void Write(StringBuilder? value)
        {
            Debug.Write(value?.ToString());
            base.Write(value);
        }
        public override Task WriteAsync(char value)
        {
            Debug.Write(value);
            return base.WriteAsync(value);
        }
        public override Task WriteAsync(char[] buffer, int index, int count)
        {
            Debug.Write(new Span<char>(buffer, index, count).ToString());
            return base.WriteAsync(buffer, index, count);
        }
        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            Debug.Write(buffer.Span.ToString());
            return base.WriteAsync(buffer, cancellationToken);
        }
        public override Task WriteAsync(string? value)
        {
            Debug.Write(value);
            return base.WriteAsync(value);
        }                                                        
        public override Task WriteAsync(StringBuilder? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Debug.Write(value?.ToString());
            return base.WriteAsync(value,cancellationToken);
        }                                        
        public override void WriteLine(ReadOnlySpan<char> buffer)
        {
            Debug.WriteLine(buffer.ToString());
            base.WriteLine(buffer);
        }                                        
        public override void WriteLine(StringBuilder? value)
        {
            Debug.WriteLine(value?.ToString());
            base.WriteLine(value);
        }                                                                        
        public override Task WriteLineAsync(char value)
        {
            Debug.WriteLine(value);
            return base.WriteLineAsync(value);
        }                                                                                                                                        
        public override Task WriteLineAsync(char[] buffer, int index, int count)
        {
            Debug.WriteLine(new Span<char>(buffer,index,count).ToString());
            return base.WriteLineAsync(buffer, index, count);
        }                                                                
        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default(CancellationToken))
        {
            Debug.WriteLine(buffer.ToString());
            return base.WriteLineAsync(buffer, cancellationToken);
        }                                                                        
        public override Task WriteLineAsync(string? value)
        {
            Debug.WriteLine(value);
            return base.WriteLineAsync(value);
        }                                                                
        public override Task WriteLineAsync(StringBuilder? value, CancellationToken cancellationToken = default(CancellationToken))
        {
            Debug.WriteLine(value?.ToString());
            return base.WriteLineAsync(value, cancellationToken);
        }
    }
}
