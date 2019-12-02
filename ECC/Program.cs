using System;
using System.Security.Cryptography;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ECC
{
    class Program
    {
        static FPoint computeBasePoint()
        {
            while (true)
            {
                var p = computeRandomPoint();
                var n = new BigInteger("400000000000000000002BEC12BE2262D39BCF14D", 16);
                var r = p.Multiply(n);
                //if (r.X != null || r.Y != null)
                  //  continue;
                return p;
            }
        }

        static FPoint computeRandomPoint()
        {
            int m = 163;
            int k1 = 3;
            int k2 = 6;
            int k3 = 7;

            var a = new BigInteger("1", 16);
            var b = new BigInteger("5FF6108462A2DC8210AB403925E638A19C1455D21", 16);
            var curve = new FCurve(m, k1, k2, k3, a, b);
            BigInteger u = getRandom();

            var u_element = new FieldElement(m, k1, k2, k3, u);
            var a_element = new FieldElement(m, k1, k2, k3, a);
            var b_element = new FieldElement(m, k1, k2, k3, b);

            var au = u_element.Multiply(u_element).Multiply(a_element);
            var w = u_element.Multiply(u_element).Multiply(u_element).Add(au).Add(b_element);
            var z = quadraticEquation(u_element.ToBigInteger(), w.ToBigInteger());
            var point = new FPoint(curve, u_element, z);

            return point;
        }
        static FieldElement quadraticEquation(BigInteger u, BigInteger w)
        {
            int m = 163;
            int k1 = 3;
            int k2 = 6;
            int k3 = 7;
            var w1 = new FieldElement(m, k1, k2, k3, w);
            var u1 = new FieldElement(m, k1, k2, k3, u);
            var u2 = u1.Invert().Square();
            var v = (FieldElement)w1.Multiply(u2);
            var tr = trace(v);
            var t = halfTrace(v);
            var z = (FieldElement)t.Multiply(u1);

            return z;
        }

        static FieldElement trace(FieldElement x)
        {
            int m = 163;
            FieldElement t = x;
            for (int i = 1; i < m; i++)
            {
                t = (FieldElement)t.Square().Add(x);
            }
            return t;
        }

        static FieldElement halfTrace(FieldElement x)
        {
            int m = 163;
            FieldElement t = x;
            for (int i = 1; i <= ((m - 1) / 2); i++)
            {
                t = (FieldElement)t.Square().Square().Add(x);
            }
            return t;
        }

        static BigInteger getRandom()
        {
            byte[] b = new byte[30];
            var rngCryptoServiceProvider = new RNGCryptoServiceProvider();
            rngCryptoServiceProvider.GetBytes(b);
            BigInteger bi = new BigInteger(b);
            return bi;
        }
        static void Main(string[] args)
        {
            int m = 163;
            int k1 = 3;
            int k2 = 6;
            int k3 = 7;

            var a = new BigInteger("1", 16);
            var b = new BigInteger("5FF6108462A2DC8210AB403925E638A19C1455D21", 16);
            var n = new BigInteger("400000000000000000002BEC12BE2262D39BCF14D", 16);
            //var basePoint = computeBasePoint();
            var x = new BigInteger("72D867F93A93AC27DF9FF01AFFE74885C8C540420", 16);
            var y = new BigInteger("0224A9C3947852B97C5599D5F4AB81122ADC3FD9B", 16);
            //var x = basePoint.X.ToBigInteger();
            //var y = basePoint.Y.ToBigInteger();
            var curve = new FCurve(m, k1, k2, k3, a, b);
            var x_element = new FieldElement(m, k1, k2, k3, x);
            var y_element = new FieldElement(m, k1, k2, k3, y);
            var P = new FPoint(curve, x_element, y_element);

            var d = getRandom();
            var Q = (FPoint)P.Multiply(d).Negate();
            Console.WriteLine("Q (x, y) = ({0}, {1})", Q.X.ToBigInteger().ToString(16), Q.Y.ToBigInteger().ToString(16));

            var M = new BigInteger("1263612ABD726", 16);

            var e = getRandom();
            var eP = P.Multiply(e);
            Console.WriteLine("eP (x, y) = ({0}, {1})", eP.X.ToBigInteger().ToString(16), eP.Y.ToBigInteger().ToString(16));
            var F_e = eP.X.ToBigInteger();
            Console.WriteLine("F_e = {0}", F_e.ToString(16));
            var M_temp = new FieldElement(m, k1, k2, k3, M);
            var F_temp = new FieldElement(m, k1, k2, k3, F_e);
            ECFieldElement y_temp = M_temp.Multiply(F_temp);
            var r = y_temp.ToBigInteger();
            Console.WriteLine("y = {0}", r.ToString(16));
            var s = (e.Add(d.Multiply(r))).Remainder(n);
            Console.WriteLine("s = {0}", s.ToString(16));

            // Validation

            var R = P.Multiply(s).Add(Q.Multiply(r));
            Console.WriteLine("R (x, y) = ({0}, {1})", R.X.ToBigInteger().ToString(16), R.Y.ToBigInteger().ToString(16));

            var M2 = new FieldElement(m, k1, k2, k3, M);
            var R_x = new FieldElement(m, k1, k2, k3, R.X.ToBigInteger());
            var y2 = M2.Multiply(R_x);

            Console.WriteLine("y2 = {0}", y_temp.ToBigInteger().ToString(16));
        }
    }
}
