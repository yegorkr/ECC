using System;

namespace ECC
{
    public abstract class ECFieldElement
    {
        public abstract BigInteger ToBigInteger();
        public abstract string FieldName { get; }
        public abstract int FieldSize { get; }
        public abstract ECFieldElement Add(ECFieldElement b);
        public abstract ECFieldElement Subtract(ECFieldElement b);
        public abstract ECFieldElement Multiply(ECFieldElement b);
        public abstract ECFieldElement Divide(ECFieldElement b);
        public abstract ECFieldElement Negate();
        public abstract ECFieldElement Square();
        public abstract ECFieldElement Invert();

        public override bool Equals(object obj)
        {
            if (obj == this) return true;
            ECFieldElement other = obj as ECFieldElement;
            if (other == null)return false;
            return Equals(other);
        }

        protected bool Equals(ECFieldElement other)
        {
            return ToBigInteger().Equals(other.ToBigInteger());
        }

        public override int GetHashCode()
        {
            return ToBigInteger().GetHashCode();
        }

        public override string ToString()
        {
            return this.ToBigInteger().ToString(2);
        }
    }

    public class FieldElement
        : ECFieldElement
    {
        public const int Gnb = 1;
        public const int Tpb = 2;
        public const int Ppb = 3;
        private int representation;
        private int m;
        private int k1;
        private int k2;
        private int k3;

        private IntArray x;
        private readonly int t;

        public FieldElement(int m, int k1, int k2, int k3, BigInteger x)
        {
            this.t = (m + 31) >> 5;
            this.x = new IntArray(x, t);
            this.m = m;
            this.k1 = k1;
            this.k2 = k2;
            this.k3 = k3;
        }

        private FieldElement(int m, int k1, int k2, int k3, IntArray x)
        {
            t = (m + 31) >> 5;
            this.x = x;
            this.m = m;
            this.k1 = k1;
            this.k2 = k2;
            this.k3 = k3;
        }

        public override BigInteger ToBigInteger()
        {
            return x.ToBigInteger();
        }

        public override string FieldName
        {
            get { return "F2m"; }
        }

        public override int FieldSize
        {
            get { return m; }
        }

        public override ECFieldElement Add(ECFieldElement b)
        {
            IntArray arr = (IntArray)this.x.Clone();
            FieldElement bF2m = (FieldElement)b;
            arr.AddShifted(bF2m.x, 0);
            return new FieldElement(m, k1, k2, k3, arr);
        }

        public override ECFieldElement Subtract(ECFieldElement b)
        {
            return Add(b);
        }

        public override ECFieldElement Multiply(ECFieldElement b)
        {
            FieldElement bF2m = (FieldElement)b;
            IntArray mult = x.Multiply(bF2m.x, m);
            mult.Reduce(m, new int[] { k1, k2, k3 });
            return new FieldElement(m, k1, k2, k3, mult);
        }

        public override ECFieldElement Divide(ECFieldElement b)
        {
            ECFieldElement bInv = b.Invert();
            return Multiply(bInv);
        }

        public override ECFieldElement Negate()
        {
            return this;
        }

        public override ECFieldElement Square()
        {
            IntArray squared = x.Square(m);
            squared.Reduce(m, new int[] { k1, k2, k3 });
            return new FieldElement(m, k1, k2, k3, squared);
        }

        public override ECFieldElement Invert()
        {
            IntArray uz = (IntArray)this.x.Clone();

            IntArray vz = new IntArray(t);
            vz.SetBit(m);
            vz.SetBit(0);
            vz.SetBit(this.k1);

            if (this.representation == Ppb)
            {
                vz.SetBit(this.k2);
                vz.SetBit(this.k3);
            }

            IntArray g1z = new IntArray(t);
            g1z.SetBit(0);

            IntArray g2z = new IntArray(t);

            while (uz.GetUsedLength() > 0)
            {
                int j = uz.BitLength - vz.BitLength;

                if (j < 0)
                {
                    IntArray uzCopy = uz;
                    uz = vz;
                    vz = uzCopy;

                    IntArray g1zCopy = g1z;
                    g1z = g2z;
                    g2z = g1zCopy;

                    j = -j;
                }

                int jInt = j >> 5;
                int jBit = j & 0x1F;
                IntArray vzShift = vz.ShiftLeft(jBit);
                uz.AddShifted(vzShift, jInt);

                IntArray g2zShift = g2z.ShiftLeft(jBit);
                g1z.AddShifted(g2zShift, jInt);
            }

            return new FieldElement(this.m, this.k1, this.k2, this.k3, g2z);
        }

        public int Representation
        {
            get { return this.representation; }
        }

        public int M
        {
            get { return this.m; }
        }

        public int K1
        {
            get { return this.k1; }
        }

        public int K2
        {
            get { return this.k2; }
        }

        public int K3
        {
            get { return this.k3; }
        }

        public override bool Equals(
            object obj)
        {
            if (obj == this)
                return true;

            FieldElement other = obj as FieldElement;

            if (other == null)
                return false;

            return Equals(other);
        }

        protected bool Equals(
            FieldElement other)
        {
            return m == other.m
                && k1 == other.k1
                && k2 == other.k2
                && k3 == other.k3
                && representation == other.representation
                && base.Equals(other);
        }

        public override int GetHashCode()
        {
            return m.GetHashCode()
                ^ k1.GetHashCode()
                ^ k2.GetHashCode()
                ^ k3.GetHashCode()
                ^ representation.GetHashCode()
                ^ base.GetHashCode();
        }
    }
}
