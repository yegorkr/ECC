using System;

namespace ECC
{
    public abstract class ECCurve
    {
        internal ECFieldElement a, b;

        public abstract int FieldSize { get; }
        public abstract ECFieldElement FromBigInteger(BigInteger x);
        public abstract ECPoint CreatePoint(BigInteger x, BigInteger y);
        public abstract ECPoint Infinity { get; }

        public ECFieldElement A
        {
            get { return a; }
        }

        public ECFieldElement B
        {
            get { return b; }
        }
    }

    public class FCurve : ECCurve
    {
        private readonly int m;
        private readonly int k1;
        private readonly int k2;
        private readonly int k3;
        private readonly BigInteger n;
        private readonly BigInteger h;
        private readonly FPoint infinity;

        public FCurve(int m, int k, BigInteger a, BigInteger b)
            : this(m, k, 0, 0, a, b)
        {
        }

        public FCurve(int m, int k, BigInteger a, BigInteger b, BigInteger n, BigInteger h)
            : this(m, k, 0, 0, a, b)
        {
        }


        public FCurve(int m, int k1, int k2, int k3, BigInteger a, BigInteger b)
        {
            this.m = m;
            this.k1 = k1;
            this.k2 = k2;
            this.k3 = k3;
            this.infinity = new FPoint(this, null, null);
            this.a = FromBigInteger(a);
            this.b = FromBigInteger(b);
        }

        public override ECPoint Infinity
        {
            get { return infinity; }
        }

        public override int FieldSize
        {
            get { return m; }
        }

        public override ECFieldElement FromBigInteger(BigInteger x)
        {
            return new FieldElement(this.m, this.k1, this.k2, this.k3, x);
        }

        public override ECPoint CreatePoint(BigInteger X1, BigInteger Y1)
        {
            return new FPoint(this, FromBigInteger(X1), FromBigInteger(Y1));
        }

        public int M
        {
            get { return m; }
        }

        public int K1
        {
            get { return k1; }
        }

        public int K2
        {
            get { return k2; }
        }

        public int K3
        {
            get { return k3; }
        }

        public BigInteger N
        {
            get { return n; }
        }

        public BigInteger H
        {
            get { return h; }
        }
    }
}
