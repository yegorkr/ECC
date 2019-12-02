using System;


namespace ECC
{
    public abstract class ECPoint
    {
        internal readonly ECCurve curve;
        internal readonly ECFieldElement x, y;

        protected internal ECPoint(ECCurve curve, ECFieldElement x, ECFieldElement y)
        {
            if (curve == null)
                throw new ArgumentNullException("curve");

            this.curve = curve;
            this.x = x;
            this.y = y;
        }

        public ECCurve Curve
        {
            get { return curve; }
        }

        public ECFieldElement X
        {
            get { return x; }
        }

        public ECFieldElement Y
        {
            get { return y; }
        }

        public bool IsInfinity
        {
            get { return x == null && y == null; }
        }

        public override bool Equals(object obj)
        {
            if (obj == this)
                return true;
            ECPoint o = obj as ECPoint;
            if (o == null)
                return false;
            if (this.IsInfinity)
                return o.IsInfinity;
            return x.Equals(o.x) && y.Equals(o.y);
        }

        public override int GetHashCode()
        {
            if (this.IsInfinity)
                return 0;
            return x.GetHashCode() ^ y.GetHashCode();
        }


        public abstract ECPoint Add(ECPoint b);
        public abstract ECPoint Subtract(ECPoint b);
        public abstract ECPoint Negate();
        public abstract ECPoint Twice();
        public abstract ECPoint Multiply(BigInteger b);

    }

    public class FPoint
        : ECPoint
    {
        public FPoint(ECCurve curve, ECFieldElement x, ECFieldElement y)
            : base(curve, x, y)
        {
        }

        public override ECPoint Multiply(BigInteger k)
        {
            if (this.IsInfinity)
                return this;

            if (k.SignValue == 0)
                return this.curve.Infinity;

            return Multiply(this, k);
        }

        public ECPoint Multiply(ECPoint p, BigInteger k)
        {
            BigInteger e = k;
            BigInteger h = e.Multiply(BigInteger.Three);

            ECPoint neg = p.Negate();
            ECPoint R = p;

            for (int i = h.BitLength - 2; i > 0; --i)
            {
                R = R.Twice();

                bool hBit = h.TestBit(i);
                bool eBit = e.TestBit(i);

                if (hBit != eBit)
                {
                    R = R.Add(hBit ? p : neg);
                }
            }

            return R;
        }

        public override ECPoint Add(ECPoint b)
        {
            return AddS((FPoint)b);
        }

        internal FPoint AddS(FPoint b)
        {
            if (this.IsInfinity)
                return b;

            if (b.IsInfinity)
                return this;

            FieldElement x2 = (FieldElement)b.X;
            FieldElement y2 = (FieldElement)b.Y;

            if (this.x.Equals(x2))
            {
                if (this.y.Equals(y2))
                    return (FPoint)this.Twice();

                return (FPoint)this.curve.Infinity;
            }

            ECFieldElement xSum = this.x.Add(x2);
            FieldElement lambda = (FieldElement)(this.y.Add(y2)).Divide(xSum);
            FieldElement x3 = (FieldElement)lambda.Square().Add(lambda).Add(xSum).Add(this.curve.A);
            FieldElement y3 = (FieldElement)lambda.Multiply(this.x.Add(x3)).Add(x3).Add(this.y);
            return new FPoint(curve, x3, y3);
        }

        public override ECPoint Subtract(ECPoint b)
        {
            return SubtractS((FPoint)b);
        }

        internal FPoint SubtractS(FPoint b)
        {
            if (b.IsInfinity)
                return this;

            return AddS((FPoint)b.Negate());
        }

        public override ECPoint Twice()
        {
            if (this.IsInfinity)
                return this;

            if (this.x.ToBigInteger().SignValue == 0)
                return this.curve.Infinity;

            FieldElement bX2 = (FieldElement)x.Invert().Square().Multiply(this.curve.B);
            FieldElement x2 = (FieldElement)x.Square().Add(bX2);

            FieldElement ydX = (FieldElement)x.Invert().Multiply(y);
            FieldElement by2 = (FieldElement)x.Add(ydX).Multiply(x2);
            FieldElement y2 = (FieldElement)x.Square().Add(by2).Add(x2);

            return new FPoint(this.curve, x2, y2);
        }

        public override ECPoint Negate()
        {
            return new FPoint(curve, this.x, this.x.Add(this.y));
        }
    }
}
