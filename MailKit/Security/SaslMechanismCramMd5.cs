//
// SaslMechanismCramMd5.cs
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
using System.Net;
using System.Text;
using System.Security.Cryptography;

namespace MailKit.Security {
	public class SaslMechanismCramMd5 : SaslMechanism
	{
		static readonly byte[] HexAlphabet = new byte[16] {
			0x30, 0x31, 0x32, 0x33, 0x34, 0x35, 0x36, 0x37, // '0' -> '7'
			0x38, 0x39, 0x41, 0x42, 0x43, 0x44, 0x45, 0x46, // '8' -> 'F'
		};

		public SaslMechanismCramMd5 (Uri uri, ICredentials credentials) : base (uri, credentials)
		{
		}

		public override string MechanismName {
			get { return "CRAM-MD5"; }
		}

		public override byte[] Challenge (byte[] token, int startIndex, int length)
		{
			if (IsAuthenticated)
				throw new InvalidOperationException ();

			if (token == null)
				return null;

			var cred = Credentials.GetCredential (Uri, MechanismName);
			var userName = Encoding.UTF8.GetBytes (cred.UserName);
			var password = Encoding.UTF8.GetBytes (cred.Password);

			using (var md5 = HashAlgorithm.Create ("MD5")) {
				var ipad = new byte[64];
				var opad = new byte[64];
				byte[] buffer;
				byte[] digest;
				int offset;

				if (password.Length > 64) {
					var checksum = md5.ComputeHash (password);
					Array.Copy (checksum, ipad, checksum.Length);
					Array.Copy (checksum, opad, checksum.Length);
				} else {
					Array.Copy (password, ipad, password.Length);
					Array.Copy (password, opad, password.Length);
				}

				for (int i = 0; i < 64; i++) {
					ipad[i] ^= 0x36;
					opad[i] ^= 0x5c;
				}

				buffer = new byte[ipad.Length + length];
				offset = 0;

				for (int i = 0; i < ipad.Length; i++)
					buffer[offset++] = ipad[i];
				for (int i = 0; i < length; i++)
					buffer[offset++] = token[startIndex + i];

				md5.Initialize ();
				digest = md5.ComputeHash (buffer);

				buffer = new byte[opad.Length + digest.Length];
				offset = 0;

				for (int i = 0; i < opad.Length; i++)
					buffer[offset++] = opad[i];
				for (int i = 0; i < digest.Length; i++)
					buffer[offset++] = digest[i];

				md5.Initialize ();
				digest = md5.ComputeHash (buffer);

				buffer = new byte[userName.Length + 1 + (digest.Length * 2)];
				offset = 0;

				for (int i = 0; i < userName.Length; i++)
					buffer[offset++] = userName[i];
				buffer[offset++] = 0x20;
				for (int i = 0; i < digest.Length; i++) {
					byte c = digest[i];

					buffer[offset++] = HexAlphabet[(c >> 4) & 0x0f];
					buffer[offset++] = HexAlphabet[c & 0x0f];
				}

				IsAuthenticated = true;

				return buffer;
			}
		}
	}
}
