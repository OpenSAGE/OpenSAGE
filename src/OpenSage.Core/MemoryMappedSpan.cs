#region License
/*
 * Copyright (C) 2019 Stefano Moioli <smxdev4@gmail.com>
 * This Source Code Form is subject to the terms of the Mozilla Public
 * License, v. 2.0. If a copy of the MPL was not distributed with this
 * file, You can obtain one at http://mozilla.org/MPL/2.0/.
 */
#endregion
using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.MemoryMappedFiles;
using System.Text;

namespace OpenSage.Core
{
	public unsafe class MemoryMappedSpan<T> : MemoryManager<T>, IDisposable where T : unmanaged
	{
		public readonly int Length;

		private readonly MemoryMappedViewAccessor acc;
		private readonly byte* dptr = null;

		public MemoryMappedSpan(MemoryMappedFile mf, int length, MemoryMappedFileAccess mmapFlags) {
			this.Length = length;
			this.acc = mf.CreateViewAccessor(0, length, mmapFlags);
			this.acc.SafeMemoryMappedViewHandle.AcquirePointer(ref dptr);
		}

		public override Span<T> GetSpan() {
			return new Span<T>((void*)dptr, Length);
		}

		public override MemoryHandle Pin(int elementIndex = 0) {
			if (elementIndex < 0 || elementIndex >= Length) {
				throw new ArgumentOutOfRangeException(nameof(elementIndex));
			}

			return new MemoryHandle(dptr + elementIndex);
		}

		public override void Unpin() { }

		public void Dispose() {
			Dispose(true);
		}

		protected override void Dispose(bool disposing) {
			acc.Dispose();
		}
	}
}
