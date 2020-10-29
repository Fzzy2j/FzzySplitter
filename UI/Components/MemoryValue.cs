using System;
using System.Text;
using LiveSplit.ComponentUtil;
using LiveSplit.UI.Components;

namespace FzzyTools.UI.Components
{
    class MemoryValue
    {

        private DeepPointer pointer;
        private string type;
        private bool fromThisTick;

        private bool _firstTick = true;

        public MemoryValue(string type, DeepPointer pointer)
        {
            this.pointer = pointer;
            this.type = type;
            SetDefault();
        }

        private dynamic current;
        private dynamic old;

        private void SetValue(dynamic value)
        {
            pointer.DerefOffsets(FzzyComponent.process, out var ptr);
            if (value is float f)
            {
                FzzyComponent.process.WriteValue(ptr, f);
            }
            if (value is int i)
            {
                FzzyComponent.process.WriteValue(ptr, i);
            }
            if (value is byte[] b)
            {
                FzzyComponent.process.WriteBytes(ptr, b);
            }
            if (value is string s)
            {
                FzzyComponent.process.WriteBytes(ptr, Encoding.ASCII.GetBytes(s));
            }
        }

        public dynamic Current {
            get
            {
                if (!fromThisTick) NextTick();
                return current;
            } 
            set
            {
                SetValue(value);
            }
        }

        public dynamic Old
        {
            get
            {
                if (!fromThisTick) NextTick();
                return old;
            }
        }

        public void EndTick()
        {
            fromThisTick = false;
        }

        private void SetDefault()
        {
            switch (type)
            {
                case "int":
                    current = (int)0;
                    old = (int)0;
                    break;
                case "uint":
                    current = (uint)0;
                    old = (uint)0;
                    break;
                case "long":
                    current = (long)0;
                    old = (long)0;
                    break;
                case "ulong":
                    current = (ulong)0;
                    old = (ulong)0;
                    break;
                case "float":
                    current = (float)0;
                    old = (float)0;
                    break;
                case "double":
                    current = (double)0;
                    old = (double)0;
                    break;
                case "byte":
                    current = (byte)0;
                    old = (byte)0;
                    break;
                case "sbyte":
                    current = (sbyte)0;
                    old = (sbyte)0;
                    break;
                case "short":
                    current = (short)0;
                    old = (short)0;
                    break;
                case "ushort":
                    current = (ushort)0;
                    old = (ushort)0;
                    break;
                case "bool":
                    current = false;
                    old = false;
                    break;
                default:
                    if (type.StartsWith("string"))
                    {
                        current = "";
                        old = "";
                    }
                    else if (type.StartsWith("byte"))
                    {
                        current = (byte)0;
                        old = (byte)0;
                    }
                    break;
            }
        }

        private void NextTick()
        {
            fromThisTick = true;
            old = current;
            switch (type)
            {
                case "int":
                    current = pointer.Deref<int>(FzzyComponent.process);
                    break;
                case "uint":
                    current = pointer.Deref<uint>(FzzyComponent.process);
                    break;
                case "long":
                    current = pointer.Deref<long>(FzzyComponent.process);
                    break;
                case "ulong":
                    current = pointer.Deref<ulong>(FzzyComponent.process);
                    break;
                case "float":
                    current = pointer.Deref<float>(FzzyComponent.process);
                    break;
                case "double":
                    current = pointer.Deref<double>(FzzyComponent.process);
                    break;
                case "byte":
                    current = pointer.Deref<byte>(FzzyComponent.process);
                    break;
                case "sbyte":
                    current = pointer.Deref<sbyte>(FzzyComponent.process);
                    break;
                case "short":
                    current = pointer.Deref<short>(FzzyComponent.process);
                    break;
                case "ushort":
                    current = pointer.Deref<ushort>(FzzyComponent.process);
                    break;
                case "bool":
                    current = pointer.Deref<bool>(FzzyComponent.process);
                    break;
                default:
                    if (type.StartsWith("string"))
                    {
                        var length = int.Parse(type.Substring("string".Length));
                        current = pointer.DerefString(FzzyComponent.process, length);
                    }
                    else if (type.StartsWith("byte"))
                    {
                        var length = int.Parse(type.Substring("byte".Length));
                        current = pointer.DerefBytes(FzzyComponent.process, length);
                    }
                    break;
            }
            if (_firstTick) old = current;
            _firstTick = false;
        }

    }
}
