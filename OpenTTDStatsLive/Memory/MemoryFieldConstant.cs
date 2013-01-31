﻿namespace SimTelemetry.Domain.Memory
{
    public class MemoryFieldConstant<T> : IMemoryObject
    {

        public string Name { get; protected set; }
        public MemoryPool Pool { get; protected set; }

        public MemoryProvider Memory { get; protected set; }
        public MemoryAddress AddressType { get { return MemoryAddress.Constant; } }

        public bool IsDynamic { get { return false; } }
        public bool IsStatic { get { return false; } }
        public bool IsConstant { get { return true; } }

        public int Address { get { return 0; } }
        public int Offset { get { return 0; } }
        public int Size { get { return 0; } }

        public T StaticValue { get; protected set; }

        public MemoryFieldConstant(string name, T staticValue)
        {
            Name = name;
            StaticValue = staticValue;
        }

        public TOut ReadAs<TOut>()
        {
            return MemoryDataConverter.Cast<T, TOut>(StaticValue);
        }

        public void Refresh()
        {
            // Done!
        }

        public void SetProvider(MemoryProvider provider)
        {
            Memory = provider; // don't care
        }

        public void SetPool(MemoryPool pool)
        {
            Pool = pool; // don't care
        }
    }
}