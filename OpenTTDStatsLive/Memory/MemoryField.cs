﻿using System;

namespace SimTelemetry.Domain.Memory
{
    public class MemoryField<T> : IMemoryObject
    {
        public string Name { get; protected set; }
        public MemoryProvider Memory { get; protected set; }
        public MemoryAddress AddressType { get; protected set; }

        public bool IsDynamic { get { return (AddressType == MemoryAddress.Dynamic); } }
        public bool IsStatic { get { return (AddressType == MemoryAddress.Static || AddressType == MemoryAddress.StaticAbsolute); } }
        public bool IsConstant { get { return false; } }

        public MemoryPool Pool { get; protected set; }
        public int Offset { get; protected set; }
        public int Address { get; protected set; }
        public int Size { get; protected set; }

        public Type FieldType { get; protected set; }

        public Func<T, T> Conversion { get; protected set; }

        public virtual T Value { get { return _Value; } }
        protected T _Value;

        public virtual T ReadAs()
        {
            return Value;
        }

        public virtual TOut ReadAs<TOut>()
        {
            return MemoryDataConverter.Cast<T, TOut>(Value);
        }

        public virtual void Refresh()
        {
            if (IsStatic)
                RefreshStatic();
            else
                RefreshDynamic();

            if (Value != null && Conversion != null)
                _Value = Conversion(_Value);
        }

        protected virtual void RefreshDynamic()
        {
            if (Pool == null || Pool.Value == null) return;
            _Value = MemoryDataConverter.Read<T>(Pool.Value, Offset);
        }

        protected virtual void RefreshStatic()
        {
            if (Memory == null)
                return;

            var computedAddress = 0;
            if (Address != 0 && Offset != 0)
                computedAddress = Memory.Reader.ReadInt32(Memory.BaseAddress + Address) + Offset;
            else
            {
                computedAddress = AddressType == MemoryAddress.Static
                                      ? Memory.BaseAddress + Address
                                      : Address;
            }

            var data = Memory.Reader.ReadBytes(computedAddress, (uint)Size);
            _Value = MemoryDataConverter.Read<T>(data, 0);
        }

        public void SetProvider(MemoryProvider provider)
        {
            if (Memory != null) throw new Exception("Can only set 1 memory provider");
            Memory = provider;
        }

        public void SetPool(MemoryPool pool)
        {
            //if (Pool != null) throw new Exception("Can only set 1 pool");
            Pool = pool;
        }
        #region Without conversion
        public MemoryField(string name,  MemoryAddress type, int address, int size)
        {
            Name = name;
            FieldType = typeof(T);
            Address = address;
            Size = size;
            Offset = 0;
            AddressType = type;
        }

        public MemoryField(string name,  MemoryAddress type, int address, int offset, int size)
        {
            Name = name;
            FieldType = typeof(T);
            Address = address;
            Size = size;
            Offset = offset;
            AddressType = type;
        }

        public MemoryField(string name,  MemoryAddress type, MemoryPool pool, int offset, int size)
        {
            Name = name;
            FieldType = typeof(T);
            Size = size;
            Offset = offset;
            Pool = pool;
            AddressType = type;
        }
        #endregion


        public MemoryField(string name,  MemoryAddress type, int address, int size, Func<T,T> conversion)
        {
            Name = name;
            FieldType = typeof(T);
            Address = address;
            Size = size;
            Offset = 0;
            Conversion = conversion;
            AddressType = type;
        }

        public MemoryField(string name,  MemoryAddress type, int address, int offset, int size, Func<T, T> conversion)
        {
            Name = name;
            FieldType = typeof(T);
            Address = address;
            Size = size;
            Offset = offset;
            Conversion = conversion;
            AddressType = type;
        }

        public MemoryField(string name,  MemoryAddress type, MemoryPool pool, int offset, int size, Func<T, T> conversion)
        {
            Name = name;
            FieldType = typeof(T);
            Size = size;
            Offset = offset;
            Pool = pool;
            Conversion = conversion;
            AddressType = type;
        }

    }
}