﻿using System;
using System.Collections.Generic;
using System.Text;

namespace IOTesting
{
    public static class Extentions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }
    }
}
