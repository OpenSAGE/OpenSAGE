#region License
/*
 * Copyright (C) 2019 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.MemoryMappedFiles;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenSage.Core
{
	public class MFile : IDisposable
	{
		public readonly MemoryMappedSpan<byte> Span;

		public MFile(MemoryMappedSpan<byte> span) {
			this.Span = span;
		}

		public static MFile Open(string filePath, FileMode fileMode, FileAccess fileAccess, FileShare fileShare) {
			FileStream fs = new FileStream(filePath, fileMode, fileAccess, fileShare);

            MemoryMappedFileAccess mmapFlags = MemoryMappedFileAccess.Read;
            if (fileAccess.HasFlag(FileAccess.Write))
            {
                mmapFlags = MemoryMappedFileAccess.ReadWrite;
            }

			MemoryMappedFile mmf = MemoryMappedFile.CreateFromFile(
				fs, null, 0,
				mmapFlags, HandleInheritability.Inheritable, false
			);
			MemoryMappedSpan<byte> span = new MemoryMappedSpan<byte>(mmf, (int)fs.Length, mmapFlags);
			return new MFile(span);
		}

		public void Dispose() {
			Span.Dispose();
		}

        public SpanStream NewSpanStream()
        {
            return new SpanStream(Span.Memory);
        }
    }
}
