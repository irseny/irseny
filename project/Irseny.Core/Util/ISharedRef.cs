﻿using System;
namespace Irseny.Core.Util {
	public interface ISharedRef : IDisposable {
		bool LastReference { get; }
		bool Disposed { get; }
	}
	public interface ISharedRef<T> : ISharedRef {
		T Reference { get; }
		void Reset(T reference);

	}
}
