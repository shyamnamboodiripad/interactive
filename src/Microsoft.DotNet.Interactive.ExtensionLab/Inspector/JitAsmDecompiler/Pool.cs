﻿/*
Copyright (c) 2016-2017, Andrey Shchekin
All rights reserved.

Redistribution and use in source and binary forms, with or without
modification, are permitted provided that the following conditions are met:

* Redistributions of source code must retain the above copyright notice, this
  list of conditions and the following disclaimer.

* Redistributions in binary form must reproduce the above copyright notice,
  this list of conditions and the following disclaimer in the documentation
  and/or other materials provided with the distribution.

THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS"
AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE
IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE
FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL
DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER
CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY,
OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE
OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
*/
using System;
using System.Collections.Concurrent;

namespace Microsoft.DotNet.Interactive.ExtensionLab.Inspector.JitAsmDecompiler;

public class Pool<T>
{
    private readonly Func<T> _factory;
    private readonly ConcurrentBag<T> _pool = new ConcurrentBag<T>();

    public Pool(Func<T> factory) => _factory = factory;

    public Lease GetOrCreate()
    {
        if (!_pool.TryTake(out var value))
            value = _factory();

        return new Lease(this, value);
    }

    private void Return(T value) => _pool.Add(value);

    public readonly struct Lease : IDisposable
    {
        private readonly Pool<T> _pool;

        internal Lease(Pool<T> pool, T value)
        {
            _pool = pool;
            Object = value;
        }

        public T Object { get; }

        public void Dispose() => _pool.Return(Object);
    }
}