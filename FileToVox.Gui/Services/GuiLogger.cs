using System;
using System.IO;
using System.Text;

namespace FileToVox.Gui.Services
{
	public class GuiLogger : TextWriter
	{
		private readonly Action<string> _onLine;
		private readonly StringBuilder _buffer = new();

		public GuiLogger(Action<string> onLine)
		{
			_onLine = onLine;
		}

		public override Encoding Encoding => Encoding.UTF8;

		public override void Write(char value)
		{
			if (value == '\n')
			{
				FlushBuffer();
			}
			else if (value != '\r')
			{
				_buffer.Append(value);
			}
		}

		public override void Write(string value)
		{
			if (value == null) return;

			foreach (char c in value)
			{
				Write(c);
			}
		}

		public override void WriteLine(string value)
		{
			_buffer.Append(value);
			FlushBuffer();
		}

		public override void WriteLine()
		{
			FlushBuffer();
		}

		private void FlushBuffer()
		{
			if (_buffer.Length > 0)
			{
				string line = _buffer.ToString();
				_buffer.Clear();
				_onLine?.Invoke(line);
			}
		}
	}
}
