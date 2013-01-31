﻿using System;

namespace SimTelemetry.Domain.Memory
{
    public class MemoryFieldFunc<T> : IMemoryObject
    {
        public string Name { get; protected set; }
        public MemoryPool Pool { get; protected set; }

        public MemoryProvider Memory { get; protected set; }
        public MemoryAddress AddressType { get { return MemoryAddress.Constant; } }

        public bool IsDynamic { get { return false; } }
        public bool IsStatic { get { return false; } }
        public bool IsConstant { get { return false; } }

        public int Address { get { return 0; } }
        public int Offset { get { return 0; } }
        public int Size { get { return 0; } }

        public Func<MemoryPool, T> ValidationFunc { get; protected set; }

        public MemoryFieldFunc(string name, Func<MemoryPool, T> validationFunc)
        {
            Name = name;
            ValidationFunc = validationFunc;
        }

        public TOut ReadAs<TOut>()
        {
            return MemoryDataConverter.Cast<T, TOut>(ValidationFunc(Pool));
        }

        public void Refresh()
        {
            // Done!
        }

        public void SetProvider(MemoryProvider provider)
        {
            Memory = provider;
        }

        public void SetPool(MemoryPool pool)
        {
            Pool = pool; 
        }
    }
}