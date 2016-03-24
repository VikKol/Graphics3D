using System;
using System.Runtime.InteropServices;

namespace Graphics3D
{
    public static unsafe class ArrayExtensions
    {
        [StructLayout(LayoutKind.Explicit)]
        private struct ArrayUnion
        {
            [FieldOffset(0)]
            public byte[] ByteArray;
            [FieldOffset(0)]
            public int[] IntArray;
        }

        public static void AsIntArray(this byte[] array, Action<int[], int> action)
        {
            if (array == null || array.Length == 0)
                return;

            var union = new ArrayUnion { ByteArray = array };
            action(union.IntArray, array.Length * sizeof(int));
        }
    }
}
