using System;
using System.Collections;
using System.Text;
using System.Threading;

namespace ECC
{
    internal class IntArray
        : ICloneable
    {
        // TODO make m fixed for the IntArray, and hence compute T once and for all

        // TODO Use uint's internally?
        private int[] m_ints;

        public IntArray(int intLen)
        {
            m_ints = new int[intLen];
        }

        private IntArray(int[] ints)
        {
            m_ints = ints;
        }

        public IntArray(BigInteger bigInt)
            : this(bigInt, 0)
        {
        }

        public IntArray(BigInteger bigInt, int minIntLen)
        {
            if (bigInt.SignValue == -1)
                throw new ArgumentException("Only positive Integers allowed", "bigint");

            if (bigInt.SignValue == 0)
            {
                m_ints = new int[] { 0 };
                return;
            }

            byte[] barr = bigInt.ToByteArrayUnsigned();
            int barrLen = barr.Length;

            int intLen = (barrLen + 3) / 4;
            m_ints = new int[System.Math.Max(intLen, minIntLen)];

            Array.Reverse(barr);
            Buffer.BlockCopy(barr, 0, m_ints, 0, barrLen);
        }

        public int GetUsedLength()
        {
            int highestIntPos = m_ints.Length;

            if (highestIntPos < 1)
                return 0;

            do
            {
                if (m_ints[--highestIntPos] != 0)
                {
                    return highestIntPos + 1;
                }
            }
            while (highestIntPos > 0);

            return 0;
        }

        public int BitLength
        {
            get
            {
                int intLen = GetUsedLength();

                if (intLen == 0)
                    return 0;

                int last = intLen - 1;
                uint highest = (uint)m_ints[last];
                int bits = (last << 5) + 1; // * 32 + 1

                while (highest > 1)
                {
                    ++bits;
                    highest >>= 1;
                }

                return bits;
            }
        }

        private int[] resizedInts(int newLen)
        {
            int[] newInts = new int[newLen];
            int oldLen = m_ints.Length;
            int copyLen = oldLen < newLen ? oldLen : newLen;

            Array.Copy(m_ints, 0, newInts, 0, copyLen);

            return newInts;
        }

        public BigInteger ToBigInteger()
        {
            var array = new byte[(BitLength + 7) / 8];

            Buffer.BlockCopy(m_ints, 0, array, 0, array.Length);
            Array.Reverse(array);

            return new BigInteger(1, array);
        }

        // Ñäâèã âëåâî íà 1 áèò
        public void ShiftLeft()
        {
            int usedLen = GetUsedLength();

            if (usedLen == 0)
                return;

            if (m_ints[usedLen - 1] < 0)
            {
                // highest bit of highest used byte is set, so shifting left will
                // make the IntArray one byte longer
                usedLen++;

                if (usedLen > m_ints.Length)
                {
                    // make the m_ints one byte longer, because we need one more
                    // byte which is not available in m_ints
                    m_ints = resizedInts(m_ints.Length + 1);
                }
            }

            bool carry = false;
            for (int i = 0; i < usedLen; i++)
            {
                // nextCarry is true if highest bit is set
                bool nextCarry = m_ints[i] < 0;
                m_ints[i] <<= 1;

                if (carry)
                    m_ints[i] |= 1;

                carry = nextCarry;
            }
        }

        public IntArray ShiftLeft(int n)
        {
            int usedLen = GetUsedLength();
            if (usedLen == 0)
            {
                return this;
            }

            if (n == 0)
            {
                return this;
            }

            if (n > 31)
            {
                throw new ArgumentException("shiftLeft() for max 31 bits "
                    + ", " + n + "bit shift is not possible", "n");
            }

            int[] newInts = new int[usedLen + 1];

            int nm32 = 32 - n;
            newInts[0] = m_ints[0] << n;

            for (int i = 1; i < usedLen; i++)
            {
                newInts[i] = (m_ints[i] << n) | (int)((uint)m_ints[i - 1] >> nm32);
            }

            newInts[usedLen] = (int)((uint)m_ints[usedLen - 1] >> nm32);

            return new IntArray(newInts);
        }

        // shift -- íà ñêîëüêî áàéò îòñòóïèòü îò íà÷àëà
        // äàëåå äåëàåì XOR
        public void AddShifted(IntArray other, int shift)
        {
            int usedLenOther = other.GetUsedLength();
            int newMinUsedLen = usedLenOther + shift;

            if (newMinUsedLen > m_ints.Length)
            {
                m_ints = resizedInts(newMinUsedLen);
                //Console.WriteLine("Resize required");
            }

            for (int i = 0; i < usedLenOther; i++)
            {
                m_ints[i + shift] ^= other.m_ints[i];
            }
        }

        public int Length
        {
            get { return m_ints.Length; }
        }

        // ïðîâåðÿåì áèò
        public bool TestBit(int n)
        {
            // theInt = n / 32
            int theInt = n >> 5;
            // theBit = n % 32
            int theBit = n & 0x1F;
            int tester = 1 << theBit;
            return ((m_ints[theInt] & tester) != 0);
        }

        // ïåðåâîðà÷èâàåì áèò
        public void FlipBit(int n)
        {
            // theInt = n / 32
            int theInt = n >> 5;
            // theBit = n % 32
            int theBit = n & 0x1F;
            int flipper = 1 << theBit;
            m_ints[theInt] ^= flipper;
        }

        // óñòàíîâêà áèòà â 1
        public void SetBit(int n)
        {
            // theInt = n / 32
            int theInt = n >> 5;
            // theBit = n % 32
            int theBit = n & 0x1F;
            int setter = 1 << theBit;
            m_ints[theInt] |= setter;
        }

        public IntArray Multiply(IntArray other, int m)
        {
            // Lenght of c is 2m bits rounded up to the next int (32 bit)
            int t = (m + 31) >> 5; // ðàçìåð â áàéòàõ

            if (m_ints.Length < t)
                m_ints = resizedInts(t);

            IntArray b = new IntArray(other.resizedInts(other.Length + 1));
            IntArray c = new IntArray((m + m + 31) >> 5);

            // IntArray c = new IntArray(t + t);
            int testBit = 1;

            for (int k = 0; k < 32; k++)
            {
                for (int j = 0; j < t; j++)
                {
                    if ((m_ints[j] & testBit) != 0)
                    {
                        // The kth bit of m_ints[j] is set
                        c.AddShifted(b, j);
                    }
                }

                testBit <<= 1;
                b.ShiftLeft();
            }

            return c;
        }


        // TODO note, redPol.Length must be 3 for TPB and 5 for PPB
        public void Reduce(int m, int[] redPol)
        {
            for (int i = m + m - 2; i >= m; i--)
            {
                if (TestBit(i))
                {
                    int bit = i - m;
                    FlipBit(bit);
                    FlipBit(i);

                    int l = redPol.Length;

                    while (--l >= 0)
                    {
                        FlipBit(redPol[l] + bit);
                    }
                }
            }

            m_ints = resizedInts((m + 31) >> 5);
        }

        public IntArray Square(int m)
        {
            // ÌÎÆÍÎ ÇÀÌÅÍÈÒÜ ÓÌÍÎÆÅÍÈÅÌ, íî ìåäëåííî!
            //IntArray other = (IntArray)this.Clone();
            //return Multiply(other, m);

            // TODO make the table static readonly
            int[] table = { 0x0, 0x1, 0x4, 0x5, 0x10, 0x11, 0x14, 0x15, 0x40,
                                    0x41, 0x44, 0x45, 0x50, 0x51, 0x54, 0x55 };

            int t = (m + 31) >> 5;
            if (m_ints.Length < t)
            {
                m_ints = resizedInts(t);
            }

            IntArray c = new IntArray(t + t);

            // TODO twice the same code, put in separate private method
            for (int i = 0; i < t; i++)
            {
                int v0 = 0;
                for (int j = 0; j < 4; j++)
                {
                    v0 = (int)((uint)v0 >> 8);
                    int u = (int)((uint)m_ints[i] >> (j * 4)) & 0xF;
                    int w = table[u] << 24;
                    v0 |= w;
                }
                c.m_ints[i + i] = v0;

                v0 = 0;
                int upper = (int)((uint)m_ints[i] >> 16);
                for (int j = 0; j < 4; j++)
                {
                    v0 = (int)((uint)v0 >> 8);
                    int u = (int)((uint)upper >> (j * 4)) & 0xF;
                    int w = table[u] << 24;
                    v0 |= w;
                }
                c.m_ints[i + i + 1] = v0;
            }
            return c;
        }

        public object Clone()
        {
            return new IntArray((int[])m_ints.Clone());
        }
    }
}
