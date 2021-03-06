//
// Pop3Command.cs
//
// Author: Jeffrey Stedfast <jeff@xamarin.com>
//
// Copyright (c) 2013 Jeffrey Stedfast
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//

using System;
using System.Threading;

namespace MailKit.Net.Pop3 {
	public delegate void Pop3CommandHandler (Pop3Engine engine, Pop3Command pc, string text);

	public enum Pop3CommandStatus {
		Queued         = -5,
		Active         = -4,
		Continue       = -3,
		ProtocolError  = -2,
		Error          = -1,
		Ok             =  0
	}

	public class Pop3Command
	{
		public CancellationToken CancelToken { get; private set; }
		public Pop3CommandHandler Handler { get; set; }
		public string Command { get; private set; }
		public int Id { get; internal set; }

		// output
		public Pop3CommandStatus Status { get; internal set; }
		public Exception Exception { get; set; }

		public Pop3Command (CancellationToken token, string format, params object[] args)
		{
			Command = string.Format (format, args);
			CancelToken = token;
		}
	}
}
