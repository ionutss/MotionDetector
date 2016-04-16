using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

namespace MotionDetector
{
    struct CircularData
    {
        public double X;
        public double Y;
        public double Z;
    }

    class CircularBuffer
    {
        public int Size;
        public int ReadOffset, WriteOffset;
        public CircularData[] buf;

        public CircularBuffer(int size)
        {
            this.Size = size;
            this.buf = new CircularData[size];
            WriteOffset = 0;
            ReadOffset = 0;
        }

        public void Write(double valueX, double valueY, double valueZ)
        {
            buf[WriteOffset].X = valueX;
            buf[WriteOffset].Y = valueY;
            buf[WriteOffset].Z = valueZ;
        }

        public CircularData Read()
        {
            return buf[ReadOffset];
        }

        public CircularData[] Read(int numberOfReads)
        {
            var ret = new CircularData[numberOfReads];
            for (int i = ReadOffset, j = 0; j < numberOfReads; i++, j++)
            {
                ret[j] = buf[i % Size];
            }
            return ret;
        }

        public bool IsWriteOffsetBlocked()
        {
            if ((WriteOffset - ReadOffset == -1) || (WriteOffset - ReadOffset == Size - 1))
            {
                return true;
            }
            return false;
        }

        public bool IsReadOffsetBlocked()
        {
            if ((ReadOffset - WriteOffset == -1) || (ReadOffset - WriteOffset == Size - 1))
            {
                return true;
            }
            return false;
        }

        public void IncrementWriteOffset()
        {
            WriteOffset = (WriteOffset + 1) % Size;
        }

        public void IncrementReadOffset(int offset)
        {
            ReadOffset = (ReadOffset + offset) % Size;
        }

        public bool IsReadyToProcess(int minimumDataAmount)
        {
            var ret = false;
            switch (ReadOffset < WriteOffset)
            {
                case true:
                    if ((WriteOffset - ReadOffset) >= minimumDataAmount)
                    {
                        ret = true;
                    }
                    break;
                case false:
                    if (Size - (ReadOffset - WriteOffset) >= minimumDataAmount)
                    {
                        ret = true;
                    }
                    break;
            }

            return ret;
        }

    }
}