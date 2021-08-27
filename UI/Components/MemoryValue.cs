using System;
using System.Diagnostics;
using System.Linq;
using System.Text;
using LiveSplit.ComponentUtil;
using LiveSplit.Options;
using LiveSplit.UI.Components;

namespace FzzyTools.UI.Components
{
    class MemoryValue
    {
        public static IntPtr SigScan(string target)
        {
            var scantarget = new SigScanTarget(target);
            IntPtr scan = IntPtr.Zero;
            foreach (var page in FzzyComponent.process.MemoryPages(true).Reverse())
            {
                var scanner = new SignatureScanner(FzzyComponent.process, page.BaseAddress, (int) page.RegionSize);
                var s = scanner.Scan(scantarget);
                if (s != IntPtr.Zero)
                {
                    scan = s;
                    break;
                }
            }

            return scan;
        }

        public DeepPointer pointer;
        private string type;
        private bool fromThisTick;

        private bool firstTick = true;

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
            if (value is bool b)
            {
                FzzyComponent.process.WriteValue(ptr, b);
            }

            if (value is float f)
            {
                FzzyComponent.process.WriteValue(ptr, f);
            }

            if (value is int i)
            {
                FzzyComponent.process.WriteValue(ptr, i);
            }

            if (value is byte[] by)
            {
                FzzyComponent.process.WriteBytes(ptr, by);
            }

            if (value is string s)
            {
                FzzyComponent.process.WriteBytes(ptr, Encoding.ASCII.GetBytes(s));
            }
        }

        public void Update()
        {
            if (!fromThisTick) NextTick();
        }

        public dynamic Current
        {
            get
            {
                if (!fromThisTick) NextTick();
                return current;
            }
            set { SetValue(value); }
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
                    current = (int) 0;
                    old = (int) 0;
                    break;
                case "uint":
                    current = (uint) 0;
                    old = (uint) 0;
                    break;
                case "long":
                    current = (long) 0;
                    old = (long) 0;
                    break;
                case "ulong":
                    current = (ulong) 0;
                    old = (ulong) 0;
                    break;
                case "float":
                    current = (float) 0;
                    old = (float) 0;
                    break;
                case "double":
                    current = (double) 0;
                    old = (double) 0;
                    break;
                case "byte":
                    current = (byte) 0;
                    old = (byte) 0;
                    break;
                case "sbyte":
                    current = (sbyte) 0;
                    old = (sbyte) 0;
                    break;
                case "short":
                    current = (short) 0;
                    old = (short) 0;
                    break;
                case "ushort":
                    current = (ushort) 0;
                    old = (ushort) 0;
                    break;
                case "bool":
                    current = false;
                    old = false;
                    break;
                case "address":
                    current = IntPtr.Zero;
                    old = IntPtr.Zero;
                    break;
                default:
                    if (type.StartsWith("string"))
                    {
                        current = "";
                        old = "";
                    }
                    else if (type.StartsWith("byte"))
                    {
                        current = (byte) 0;
                        old = (byte) 0;
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
                case "address":
                    IntPtr ptr;
                    pointer.DerefOffsets(FzzyComponent.process, out ptr);
                    current = ptr;
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

            if (firstTick) old = current;
            firstTick = false;
        }
    }
}