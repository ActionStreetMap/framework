using System;
using ActionStreetMap.Core.Geometry.Triangle.Geometry;

namespace ActionStreetMap.Core.Geometry.Triangle
{   
    /// <summary> Adaptive exact arithmetic geometric predicates. </summary>
    /// <remarks>
    /// The adaptive exact arithmetic geometric predicates implemented herein are described in
    /// detail in the paper "Adaptive Precision Floating-Point Arithmetic and Fast Robust
    /// Geometric Predicates." by Jonathan Richard Shewchuk, see
    /// http://www.cs.cmu.edu/~quake/robust.html
    /// 
    /// The macros of the original C code were automatically expanded using the Visual Studio
    /// command prompt with the command "CL /P /C EXACT.C", see
    /// http://msdn.microsoft.com/en-us/library/8z9z0bx6.aspx
    /// </remarks>
    public static class RobustPredicates
    {
        private static double epsilon, splitter, resulterrbound;
        private static double ccwerrboundA, ccwerrboundB, ccwerrboundC;
        private static double iccerrboundA, iccerrboundB, iccerrboundC;

        /// <summary> Initialize the variables used for exact arithmetic. </summary>
        /// <remarks>
        /// 'epsilon' is the largest power of two such that 1.0 + epsilon = 1.0 in
        /// floating-point arithmetic. 'epsilon' bounds the relative roundoff
        /// error. It is used for floating-point error analysis.
        ///
        /// 'splitter' is used to split floating-point numbers into two half-
        /// length significands for exact multiplication.
        ///
        /// I imagine that a highly optimizing compiler might be too smart for its
        /// own good, and somehow cause this routine to fail, if it pretends that
        /// floating-point arithmetic is too much like double arithmetic.
        ///
        /// Don't change this routine unless you fully understand it.
        /// </remarks>
        public static void ExactInit()
        {
            double half;
            double check, lastcheck;
            bool every_other;

            every_other = true;
            half = 0.5;
            epsilon = 1.0;
            splitter = 1.0;
            check = 1.0;
            // Repeatedly divide 'epsilon' by two until it is too small to add to
            // one without causing roundoff.  (Also check if the sum is equal to
            // the previous sum, for machines that round up instead of using exact
            // rounding.  Not that these routines will work on such machines.)
            do
            {
                lastcheck = check;
                epsilon *= half;
                if (every_other)
                    splitter *= 2.0;
                every_other = !every_other;
                check = 1.0 + epsilon;
            } while ((check != 1.0) && (check != lastcheck));
            splitter += 1.0;
            // Error bounds for orientation and incircle tests. 
            resulterrbound = (3.0 + 8.0 * epsilon) * epsilon;
            ccwerrboundA = (3.0 + 16.0 * epsilon) * epsilon;
            ccwerrboundB = (2.0 + 12.0 * epsilon) * epsilon;
            ccwerrboundC = (9.0 + 64.0 * epsilon) * epsilon * epsilon;
            iccerrboundA = (10.0 + 96.0 * epsilon) * epsilon;
            iccerrboundB = (4.0 + 48.0 * epsilon) * epsilon;
            iccerrboundC = (44.0 + 576.0 * epsilon) * epsilon * epsilon;
        }

        /// <summary>
        /// Check, if the three points appear in counterclockwise order. The result is 
        /// also a rough approximation of twice the signed area of the triangle defined 
        /// by the three points.
        /// </summary>
        /// <param name="pa">Point a.</param>
        /// <param name="pb">Point b.</param>
        /// <param name="pc">Point c.</param>
        /// <returns>Return a positive value if the points pa, pb, and pc occur in 
        /// counterclockwise order; a negative value if they occur in clockwise order; 
        /// and zero if they are collinear.</returns>
        public static double CounterClockwise(Point pa, Point pb, Point pc)
        {
            double detleft, detright, det;
            double detsum, errbound;

            detleft = (pa.X - pc.X) * (pb.Y - pc.Y);
            detright = (pa.Y - pc.Y) * (pb.X - pc.X);
            det = detleft - detright;

            if (Behavior.NoExact)
                return det;

            if (detleft > 0.0)
            {
                if (detright <= 0.0)
                    return det;
                else
                    detsum = detleft + detright;
            }
            else if (detleft < 0.0)
            {
                if (detright >= 0.0)
                    return det;
                else
                    detsum = -detleft - detright;
            }
            else
                return det;

            errbound = ccwerrboundA * detsum;
            if ((det >= errbound) || (-det >= errbound))
                return det;

            return CounterClockwiseAdapt(pa, pb, pc, detsum);
        }

        /// <summary>
        /// Check if the point pd lies inside the circle passing through pa, pb, and pc. The 
        /// points pa, pb, and pc must be in counterclockwise order, or the sign of the result 
        /// will be reversed.
        /// </summary>
        /// <param name="pa">Point a.</param>
        /// <param name="pb">Point b.</param>
        /// <param name="pc">Point c.</param>
        /// <param name="pd">Point d.</param>
        /// <returns>Return a positive value if the point pd lies inside the circle passing through 
        /// pa, pb, and pc; a negative value if it lies outside; and zero if the four points 
        /// are cocircular.</returns>
        public static double InCircle(Point pa, Point pb, Point pc, Point pd)
        {
            double adx, bdx, cdx, ady, bdy, cdy;
            double bdxcdy, cdxbdy, cdxady, adxcdy, adxbdy, bdxady;
            double alift, blift, clift;
            double det;
            double permanent, errbound;

            adx = pa.X - pd.X;
            bdx = pb.X - pd.X;
            cdx = pc.X - pd.X;
            ady = pa.Y - pd.Y;
            bdy = pb.Y - pd.Y;
            cdy = pc.Y - pd.Y;

            bdxcdy = bdx * cdy;
            cdxbdy = cdx * bdy;
            alift = adx * adx + ady * ady;

            cdxady = cdx * ady;
            adxcdy = adx * cdy;
            blift = bdx * bdx + bdy * bdy;

            adxbdy = adx * bdy;
            bdxady = bdx * ady;
            clift = cdx * cdx + cdy * cdy;

            det = alift * (bdxcdy - cdxbdy)
                + blift * (cdxady - adxcdy)
                + clift * (adxbdy - bdxady);

            if (Behavior.NoExact)
                return det;

            permanent = (Math.Abs(bdxcdy) + Math.Abs(cdxbdy)) * alift
                      + (Math.Abs(cdxady) + Math.Abs(adxcdy)) * blift
                      + (Math.Abs(adxbdy) + Math.Abs(bdxady)) * clift;
            errbound = iccerrboundA * permanent;
            if ((det > errbound) || (-det > errbound))
                return det;

            return InCircleAdapt(pa, pb, pc, pd, permanent);
        }

        /// <summary>
        /// Return a positive value if the point pd is incompatible with the circle 
        /// or plane passing through pa, pb, and pc (meaning that pd is inside the 
        /// circle or below the plane); a negative value if it is compatible; and 
        /// zero if the four points are cocircular/coplanar. The points pa, pb, and 
        /// pc must be in counterclockwise order, or the sign of the result will be 
        /// reversed.
        /// </summary>
        /// <param name="pa">Point a.</param>
        /// <param name="pb">Point b.</param>
        /// <param name="pc">Point c.</param>
        /// <param name="pd">Point d.</param>
        /// <returns>Return a positive value if the point pd lies inside the circle passing through 
        /// pa, pb, and pc; a negative value if it lies outside; and zero if the four points 
        /// are cocircular.</returns>
        public static double NonRegular(Point pa, Point pb, Point pc, Point pd)
        {
            return InCircle(pa, pb, pc, pd);
        }

        /// <summary> Find the circumcenter of a triangle. </summary>
        /// <param name="org">Triangle point.</param>
        /// <param name="dest">Triangle point.</param>
        /// <param name="apex">Triangle point.</param>
        /// <param name="xi">Relative coordinate of new location.</param>
        /// <param name="eta">Relative coordinate of new location.</param>
        /// <param name="offconstant">Off-center constant.</param>
        /// <returns>Coordinates of the circumcenter (or off-center)</returns>
        public static Point FindCircumcenter(Point org, Point dest, Point apex,
            ref double xi, ref double eta, double offconstant)
        {
            double xdo, ydo, xao, yao;
            double dodist, aodist, dadist;
            double denominator;
            double dx, dy, dxoff, dyoff;

            // Compute the circumcenter of the triangle.
            xdo = dest.X - org.X;
            ydo = dest.Y - org.Y;
            xao = apex.X - org.X;
            yao = apex.Y - org.Y;
            dodist = xdo * xdo + ydo * ydo;
            aodist = xao * xao + yao * yao;
            dadist = (dest.X - apex.X) * (dest.X - apex.X) +
                     (dest.Y - apex.Y) * (dest.Y - apex.Y);

            if (Behavior.NoExact)
            {
                denominator = 0.5 / (xdo * yao - xao * ydo);
            }
            else
            {
                // Use the counterclockwise() routine to ensure a positive (and
                // reasonably accurate) result, avoiding any possibility of
                // division by zero.
                denominator = 0.5 / CounterClockwise(dest, apex, org);
            }

            dx = (yao * dodist - ydo * aodist) * denominator;
            dy = (xdo * aodist - xao * dodist) * denominator;

            // Find the (squared) length of the triangle's shortest edge.  This
            // serves as a conservative estimate of the insertion radius of the
            // circumcenter's parent. The estimate is used to ensure that
            // the algorithm terminates even if very small angles appear in
            // the input PSLG.
            if ((dodist < aodist) && (dodist < dadist))
            {
                if (offconstant > 0.0)
                {
                    // Find the position of the off-center, as described by Alper Ungor.
                    dxoff = 0.5 * xdo - offconstant * ydo;
                    dyoff = 0.5 * ydo + offconstant * xdo;
                    // If the off-center is closer to the origin than the
                    // circumcenter, use the off-center instead.
                    if (dxoff * dxoff + dyoff * dyoff < dx * dx + dy * dy)
                    {
                        dx = dxoff;
                        dy = dyoff;
                    }
                }
            }
            else if (aodist < dadist)
            {
                if (offconstant > 0.0)
                {
                    dxoff = 0.5 * xao + offconstant * yao;
                    dyoff = 0.5 * yao - offconstant * xao;
                    // If the off-center is closer to the origin than the
                    // circumcenter, use the off-center instead.
                    if (dxoff * dxoff + dyoff * dyoff < dx * dx + dy * dy)
                    {
                        dx = dxoff;
                        dy = dyoff;
                    }
                }
            }
            else
            {
                if (offconstant > 0.0)
                {
                    dxoff = 0.5 * (apex.X - dest.X) - offconstant * (apex.Y - dest.Y);
                    dyoff = 0.5 * (apex.Y - dest.Y) + offconstant * (apex.X - dest.X);
                    // If the off-center is closer to the destination than the
                    // circumcenter, use the off-center instead.
                    if (dxoff * dxoff + dyoff * dyoff <
                        (dx - xdo) * (dx - xdo) + (dy - ydo) * (dy - ydo))
                    {
                        dx = xdo + dxoff;
                        dy = ydo + dyoff;
                    }
                }
            }

            // To interpolate vertex attributes for the new vertex inserted at
            // the circumcenter, define a coordinate system with a xi-axis,
            // directed from the triangle's origin to its destination, and
            // an eta-axis, directed from its origin to its apex.
            // Calculate the xi and eta coordinates of the circumcenter.
            xi = (yao * dx - xao * dy) * (2.0 * denominator);
            eta = (xdo * dy - ydo * dx) * (2.0 * denominator);

            return new Point(org.X + dx, org.Y + dy);
        }

        /// <summary> Find the circumcenter of a triangle. </summary>
        /// <param name="org">Triangle point.</param>
        /// <param name="dest">Triangle point.</param>
        /// <param name="apex">Triangle point.</param>
        /// <param name="xi">Relative coordinate of new location.</param>
        /// <param name="eta">Relative coordinate of new location.</param>
        /// <returns>Coordinates of the circumcenter</returns>
        /// <remarks>
        /// The result is returned both in terms of x-y coordinates and xi-eta
        /// (barycentric) coordinates. The xi-eta coordinate system is defined in
        /// terms of the triangle: the origin of the triangle is the origin of the
        /// coordinate system; the destination of the triangle is one unit along the
        /// xi axis; and the apex of the triangle is one unit along the eta axis.
        /// This procedure also returns the square of the length of the triangle's
        /// shortest edge.
        /// </remarks>
        public static Point FindCircumcenter(Point org, Point dest, Point apex,
            ref double xi, ref double eta)
        {
            double xdo, ydo, xao, yao;
            double dodist, aodist;
            double denominator;
            double dx, dy;

            // Compute the circumcenter of the triangle.
            xdo = dest.X - org.X;
            ydo = dest.Y - org.Y;
            xao = apex.X - org.X;
            yao = apex.Y - org.Y;
            dodist = xdo * xdo + ydo * ydo;
            aodist = xao * xao + yao * yao;

            if (Behavior.NoExact)
            {
                denominator = 0.5 / (xdo * yao - xao * ydo);
            }
            else
            {
                // Use the counterclockwise() routine to ensure a positive (and
                // reasonably accurate) result, avoiding any possibility of
                // division by zero.
                denominator = 0.5 / CounterClockwise(dest, apex, org);
            }

            dx = (yao * dodist - ydo * aodist) * denominator;
            dy = (xdo * aodist - xao * dodist) * denominator;

            // To interpolate vertex attributes for the new vertex inserted at
            // the circumcenter, define a coordinate system with a xi-axis,
            // directed from the triangle's origin to its destination, and
            // an eta-axis, directed from its origin to its apex.
            // Calculate the xi and eta coordinates of the circumcenter.
            xi = (yao * dx - xao * dy) * (2.0 * denominator);
            eta = (xdo * dy - ydo * dx) * (2.0 * denominator);

            return new Point(org.X + dx, org.Y + dy);
        }

        #region Exact arithmetics

        /// <summary> Sum two expansions, eliminating zero components from the output expansion.  </summary>
        /// <param name="elen"></param>
        /// <param name="e"></param>
        /// <param name="flen"></param>
        /// <param name="f"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sets h = e + f.  See the Robust Predicates paper for details.
        /// 
        /// If round-to-even is used (as with IEEE 754), maintains the strongly nonoverlapping
        /// property.  (That is, if e is strongly nonoverlapping, h will be also.) Does NOT
        /// maintain the nonoverlapping or nonadjacent properties. 
        /// </remarks>
        static int FastExpansionSumZeroElim(int elen, double[] e, int flen, double[] f, double[] h)
        {
            double Q;
            double Qnew;
            double hh;
            double bvirt;
            double avirt, bround, around;
            int eindex, findex, hindex;
            double enow, fnow;

            enow = e[0];
            fnow = f[0];
            eindex = findex = 0;
            if ((fnow > enow) == (fnow > -enow))
            {
                Q = enow;
                enow = e[++eindex];
            }
            else
            {
                Q = fnow;
                fnow = f[++findex];
            }
            hindex = 0;
            if ((eindex < elen) && (findex < flen))
            {
                if ((fnow > enow) == (fnow > -enow))
                {
                    Qnew = (enow + Q); bvirt = Qnew - enow; hh = Q - bvirt;
                    enow = e[++eindex];
                }
                else
                {
                    Qnew = (fnow + Q); bvirt = Qnew - fnow; hh = Q - bvirt;
                    fnow = f[++findex];
                }
                Q = Qnew;
                if (hh != 0.0)
                {
                    h[hindex++] = hh;
                }
                while ((eindex < elen) && (findex < flen))
                {
                    if ((fnow > enow) == (fnow > -enow))
                    {
                        Qnew = (Q + enow);
                        bvirt = (Qnew - Q);
                        avirt = Qnew - bvirt;
                        bround = enow - bvirt;
                        around = Q - avirt;
                        hh = around + bround;

                        enow = e[++eindex];
                    }
                    else
                    {
                        Qnew = (Q + fnow);
                        bvirt = (Qnew - Q);
                        avirt = Qnew - bvirt;
                        bround = fnow - bvirt;
                        around = Q - avirt;
                        hh = around + bround;

                        fnow = f[++findex];
                    }
                    Q = Qnew;
                    if (hh != 0.0)
                    {
                        h[hindex++] = hh;
                    }
                }
            }
            while (eindex < elen)
            {
                Qnew = (Q + enow);
                bvirt = (Qnew - Q);
                avirt = Qnew - bvirt;
                bround = enow - bvirt;
                around = Q - avirt;
                hh = around + bround;

                enow = e[++eindex];
                Q = Qnew;
                if (hh != 0.0)
                {
                    h[hindex++] = hh;
                }
            }
            while (findex < flen)
            {
                Qnew = (Q + fnow);
                bvirt = (Qnew - Q);
                avirt = Qnew - bvirt;
                bround = fnow - bvirt;
                around = Q - avirt;
                hh = around + bround;

                fnow = f[++findex];
                Q = Qnew;
                if (hh != 0.0)
                {
                    h[hindex++] = hh;
                }
            }
            if ((Q != 0.0) || (hindex == 0))
            {
                h[hindex++] = Q;
            }
            return hindex;
        }

        /// <summary>
        /// Multiply an expansion by a scalar, eliminating zero components from the output expansion.  
        /// </summary>
        /// <param name="elen"></param>
        /// <param name="e"></param>
        /// <param name="b"></param>
        /// <param name="h"></param>
        /// <returns></returns>
        /// <remarks>
        /// Sets h = be.  See my Robust Predicates paper for details.
        /// 
        /// Maintains the nonoverlapping property.  If round-to-even is used (as with IEEE 754),
        /// maintains the strongly nonoverlapping and nonadjacent properties as well. (That is,
        /// if e has one of these properties, so will h.)
        /// </remarks>
        static int ScaleExpansionZeroElim(int elen, double[] e, double b, double[] h)
        {
            double Q, sum;
            double hh;
            double product1;
            double product0;
            int eindex, hindex;
            double enow;
            double bvirt;
            double avirt, bround, around;
            double c;
            double abig;
            double ahi, alo, bhi, blo;
            double err1, err2, err3;

            c = (splitter * b); abig = (c - b); bhi = c - abig; blo = b - bhi;
            Q = (e[0] * b); c = (splitter * e[0]); abig = (c - e[0]); ahi = c - abig; alo = e[0] - ahi; err1 = Q - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); hh = (alo * blo) - err3;
            hindex = 0;
            if (hh != 0)
            {
                h[hindex++] = hh;
            }
            for (eindex = 1; eindex < elen; eindex++)
            {
                enow = e[eindex];
                product1 = (enow * b); c = (splitter * enow); abig = (c - enow); ahi = c - abig; alo = enow - ahi; err1 = product1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); product0 = (alo * blo) - err3;
                sum = (Q + product0); bvirt = (sum - Q); avirt = sum - bvirt; bround = product0 - bvirt; around = Q - avirt; hh = around + bround;
                if (hh != 0)
                {
                    h[hindex++] = hh;
                }
                Q = (product1 + sum); bvirt = Q - product1; hh = sum - bvirt;
                if (hh != 0)
                {
                    h[hindex++] = hh;
                }
            }
            if ((Q != 0.0) || (hindex == 0))
            {
                h[hindex++] = Q;
            }
            return hindex;
        }

        /// <summary> Produce a one-word estimate of an expansion's value. </summary>
        /// <param name="elen"></param>
        /// <param name="e"></param>
        /// <returns></returns>
        static double Estimate(int elen, double[] e)
        {
            double Q;
            int eindex;

            Q = e[0];
            for (eindex = 1; eindex < elen; eindex++)
                Q += e[eindex];
            
            return Q;
        }

        /// <summary>
        /// Return a positive value if the points pa, pb, and pc occur in counterclockwise
        /// order; a negative value if they occur in clockwise order; and zero if they are
        /// collinear. The result is also a rough approximation of twice the signed area of
        /// the triangle defined by the three points. 
        /// </summary>
        /// <param name="pa"></param>
        /// <param name="pb"></param>
        /// <param name="pc"></param>
        /// <param name="detsum"></param>
        /// <returns></returns>
        /// <remarks>
        /// Uses exact arithmetic if necessary to ensure a correct answer. The result returned
        /// is the determinant of a matrix. This determinant is computed adaptively, in the
        /// sense that exact arithmetic is used only to the degree it is needed to ensure that
        /// the returned value has the correct sign.  Hence, this function is usually quite fast,
        /// but will run more slowly when the input points are collinear or nearly so.
        /// </remarks>
        static double CounterClockwiseAdapt(Point pa, Point pb, Point pc, double detsum)
        {
            double acx, acy, bcx, bcy;
            double acxtail, acytail, bcxtail, bcytail;
            double detleft, detright;
            double detlefttail, detrighttail;
            double det, errbound;
            // Edited to work around index out of range exceptions (changed array length from 4 to 5).
            // See unsafe indexing in FastExpansionSumZeroElim.

            double B3;
            int C1length, C2length, Dlength;
            
            double u3;
            double s1, t1;
            double s0, t0;

            double bvirt;
            double avirt, bround, around;
            double c;
            double abig;
            double ahi, alo, bhi, blo;
            double err1, err2, err3;
            double _i, _j;
            double _0;

            acx = (pa.X - pc.X);
            bcx = (pb.X - pc.X);
            acy = (pa.Y - pc.Y);
            bcy = (pb.Y - pc.Y);

            detleft = (acx * bcy); c = (splitter * acx); abig = c - acx; ahi = c - abig; alo = acx - ahi; c = (splitter * bcy); abig = (c - bcy); bhi = c - abig; blo = bcy - bhi; err1 = detleft - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); detlefttail = (alo * blo) - err3;
            detright = (acy * bcx); c = (splitter * acy); abig = (c - acy); ahi = c - abig; alo = acy - ahi; c = (splitter * bcx); abig = (c - bcx); bhi = c - abig; blo = bcx - bhi; err1 = detright - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); detrighttail = (alo * blo) - err3;

            double[] B = TrianglePool.AllocDoubleArray(5);
            _i = (detlefttail - detrighttail); bvirt = (detlefttail - _i); avirt = _i + bvirt; bround = bvirt - detrighttail; around = detlefttail - avirt; B[0] = around + bround; _j = (detleft + _i); bvirt = (_j - detleft); avirt = _j - bvirt; bround = _i - bvirt; around = detleft - avirt; _0 = around + bround; _i = (_0 - detright); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - detright; around = _0 - avirt; B[1] = around + bround; B3 = (_j + _i); bvirt = (B3 - _j); avirt = B3 - bvirt; bround = _i - bvirt; around = _j - avirt; B[2] = around + bround;

           
            B[3] = B3;

            det = Estimate(4, B);
            TrianglePool.FreeDoubleArray(B);
            errbound = ccwerrboundB * detsum;
            if ((det >= errbound) || (-det >= errbound))
            {
                return det;
            }

            bvirt = pa.X - acx; avirt = acx + bvirt; bround = bvirt - pc.X; around = pa.X - avirt; acxtail = around + bround;
            bvirt = (pb.X - bcx); avirt = bcx + bvirt; bround = bvirt - pc.X; around = pb.X - avirt; bcxtail = around + bround;
            bvirt = (pa.Y - acy); avirt = acy + bvirt; bround = bvirt - pc.Y; around = pa.Y - avirt; acytail = around + bround;
            bvirt = (pb.Y - bcy); avirt = bcy + bvirt; bround = bvirt - pc.Y; around = pb.Y - avirt; bcytail = around + bround;

            if ((acxtail == 0.0) && (acytail == 0.0)
                && (bcxtail == 0.0) && (bcytail == 0.0))
            {
                return det;
            }

            errbound = ccwerrboundC * detsum + resulterrbound * ((det) >= 0.0 ? (det) : -(det));
            det += (acx * bcytail + bcy * acxtail)
                 - (acy * bcxtail + bcx * acytail);
            if ((det >= errbound) || (-det >= errbound))
            {
                return det;
            }

            s1 = (acxtail * bcy); c = (splitter * acxtail); abig = (c - acxtail); ahi = c - abig; alo = acxtail - ahi; c = (splitter * bcy); abig = (c - bcy); bhi = c - abig; blo = bcy - bhi; err1 = s1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); s0 = (alo * blo) - err3;
            t1 = (acytail * bcx); c = (splitter * acytail); abig = (c - acytail); ahi = c - abig; alo = acytail - ahi; c = (splitter * bcx); abig = (c - bcx); bhi = c - abig; blo = bcx - bhi; err1 = t1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); t0 = (alo * blo) - err3;
            double[] u = TrianglePool.AllocDoubleArray(5);
            _i = (s0 - t0); bvirt = (s0 - _i); avirt = _i + bvirt; bround = bvirt - t0; around = s0 - avirt; u[0] = around + bround; _j = (s1 + _i); bvirt = (_j - s1); avirt = _j - bvirt; bround = _i - bvirt; around = s1 - avirt; _0 = around + bround; _i = (_0 - t1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - t1; around = _0 - avirt; u[1] = around + bround; u3 = (_j + _i); bvirt = (u3 - _j); avirt = u3 - bvirt; bround = _i - bvirt; around = _j - avirt; u[2] = around + bround;
            u[3] = u3;
            double[] C1 = TrianglePool.AllocDoubleArray(8);
            C1length = FastExpansionSumZeroElim(4, B, 4, u, C1);

            s1 = (acx * bcytail); c = (splitter * acx); abig = (c - acx); ahi = c - abig; alo = acx - ahi; c = (splitter * bcytail); abig = (c - bcytail); bhi = c - abig; blo = bcytail - bhi; err1 = s1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); s0 = (alo * blo) - err3;
            t1 = (acy * bcxtail); c = (splitter * acy); abig = (c - acy); ahi = c - abig; alo = acy - ahi; c = (splitter * bcxtail); abig = (c - bcxtail); bhi = c - abig; blo = bcxtail - bhi; err1 = t1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); t0 = (alo * blo) - err3;
            _i = (s0 - t0); bvirt = (s0 - _i); avirt = _i + bvirt; bround = bvirt - t0; around = s0 - avirt; u[0] = around + bround; _j = (s1 + _i); bvirt = (_j - s1); avirt = _j - bvirt; bround = _i - bvirt; around = s1 - avirt; _0 = around + bround; _i = (_0 - t1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - t1; around = _0 - avirt; u[1] = around + bround; u3 = (_j + _i); bvirt = (u3 - _j); avirt = u3 - bvirt; bround = _i - bvirt; around = _j - avirt; u[2] = around + bround;
            u[3] = u3;

            double[] C2 = TrianglePool.AllocDoubleArray(12);
            C2length = FastExpansionSumZeroElim(C1length, C1, 4, u, C2);
            TrianglePool.FreeDoubleArray(C1);

            s1 = (acxtail * bcytail); c = (splitter * acxtail); abig = (c - acxtail); ahi = c - abig; alo = acxtail - ahi; c = (splitter * bcytail); abig = (c - bcytail); bhi = c - abig; blo = bcytail - bhi; err1 = s1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); s0 = (alo * blo) - err3;
            t1 = (acytail * bcxtail); c = (splitter * acytail); abig = (c - acytail); ahi = c - abig; alo = acytail - ahi; c = (splitter * bcxtail); abig = (c - bcxtail); bhi = c - abig; blo = bcxtail - bhi; err1 = t1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); t0 = (alo * blo) - err3;
            _i = (s0 - t0); bvirt = (s0 - _i); avirt = _i + bvirt; bround = bvirt - t0; around = s0 - avirt; u[0] = around + bround; _j = (s1 + _i); bvirt = (_j - s1); avirt = _j - bvirt; bround = _i - bvirt; around = s1 - avirt; _0 = around + bround; _i = (_0 - t1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - t1; around = _0 - avirt; u[1] = around + bround; u3 = (_j + _i); bvirt = (u3 - _j); avirt = u3 - bvirt; bround = _i - bvirt; around = _j - avirt; u[2] = around + bround;
            u[3] = u3;
            double[] D = TrianglePool.AllocDoubleArray(16);
            Dlength = FastExpansionSumZeroElim(C2length, C2, 4, u, D);
            TrianglePool.FreeDoubleArray(C2);
            TrianglePool.FreeDoubleArray(D);
            return (D[Dlength - 1]);
        }

        /// <summary>
        /// Return a positive value if the point pd lies inside the circle passing through
        /// pa, pb, and pc; a negative value if it lies outside; and zero if the four points
        /// are cocircular. The points pa, pb, and pc must be in counterclockwise order, or 
        /// the sign of the result will be reversed.
        /// </summary>
        /// <param name="pa"></param>
        /// <param name="pb"></param>
        /// <param name="pc"></param>
        /// <param name="pd"></param>
        /// <param name="permanent"></param>
        /// <returns></returns>
        /// <remarks>
        /// Uses exact arithmetic if necessary to ensure a correct answer. The result returned
        /// is the determinant of a matrix. This determinant is computed adaptively, in the
        /// sense that exact arithmetic is used only to the degree it is needed to ensure that
        /// the returned value has the correct sign. Hence, this function is usually quite fast,
        /// but will run more slowly when the input points are cocircular or nearly so.
        /// </remarks>
        static double InCircleAdapt(Point pa, Point pb, Point pc, Point pd, double permanent)
        {
            double adx, bdx, cdx, ady, bdy, cdy;
            double det, errbound;

            double bdxcdy1, cdxbdy1, cdxady1, adxcdy1, adxbdy1, bdxady1;
            double bdxcdy0, cdxbdy0, cdxady0, adxcdy0, adxbdy0, bdxady0;
            
            double bc3, ca3, ab3;
            int axbclen, axxbclen, aybclen, ayybclen, alen;

            int bxcalen, bxxcalen, bycalen, byycalen, blen;
            int cxablen, cxxablen, cyablen, cyyablen, clen;

            int ablen;
            double[] finnow, finother, finswap;
            int finlength;

            double adxtail, bdxtail, cdxtail, adytail, bdytail, cdytail;
            double adxadx1, adyady1, bdxbdx1, bdybdy1, cdxcdx1, cdycdy1;
            double adxadx0, adyady0, bdxbdx0, bdybdy0, cdxcdx0, cdycdy0;
            double aa3, bb3, cc3;
            double ti1, tj1;
            double ti0, tj0;

            double u3, v3;

            int temp8len, temp16alen, temp16blen, temp16clen;
            int temp32alen, temp32blen, temp48len, temp64len;

            int axtbblen, axtcclen, aytbblen, aytcclen;

            int bxtaalen, bxtcclen, bytaalen, bytcclen;
            int cxtaalen, cxtbblen, cytaalen, cytbblen;

            int axtbclen = 0, aytbclen = 0, bxtcalen = 0, bytcalen = 0, cxtablen = 0, cytablen = 0;

            int axtbctlen, aytbctlen, bxtcatlen, bytcatlen, cxtabtlen, cytabtlen;

            int axtbcttlen, aytbcttlen, bxtcattlen, bytcattlen, cxtabttlen, cytabttlen;

            int abtlen, bctlen, catlen;
            int abttlen, bcttlen, cattlen;
            double abtt3, bctt3, catt3;
            double negate;

            double bvirt;
            double avirt, bround, around;
            double c;
            double abig;
            double ahi, alo, bhi, blo;
            double err1, err2, err3;
            double _i, _j;
            double _0;

            adx = (pa.X - pd.X);
            bdx = (pb.X - pd.X);
            cdx = (pc.X - pd.X);
            ady = (pa.Y - pd.Y);
            bdy = (pb.Y - pd.Y);
            cdy = (pc.Y - pd.Y);

            bdxcdy1 = (bdx * cdy); c = (splitter * bdx); abig = (c - bdx); ahi = c - abig; alo = bdx - ahi; c = (splitter * cdy); abig = (c - cdy); bhi = c - abig; blo = cdy - bhi; err1 = bdxcdy1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); bdxcdy0 = (alo * blo) - err3;
            cdxbdy1 = (cdx * bdy); c = (splitter * cdx); abig = (c - cdx); ahi = c - abig; alo = cdx - ahi; c = (splitter * bdy); abig = (c - bdy); bhi = c - abig; blo = bdy - bhi; err1 = cdxbdy1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); cdxbdy0 = (alo * blo) - err3;
            double[] bc = TrianglePool.AllocDoubleArray(4);
            _i = (bdxcdy0 - cdxbdy0); bvirt = (bdxcdy0 - _i); avirt = _i + bvirt; bround = bvirt - cdxbdy0; around = bdxcdy0 - avirt; bc[0] = around + bround; _j = (bdxcdy1 + _i); bvirt = (_j - bdxcdy1); avirt = _j - bvirt; bround = _i - bvirt; around = bdxcdy1 - avirt; _0 = around + bround; _i = (_0 - cdxbdy1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - cdxbdy1; around = _0 - avirt; bc[1] = around + bround; bc3 = (_j + _i); bvirt = (bc3 - _j); avirt = bc3 - bvirt; bround = _i - bvirt; around = _j - avirt; bc[2] = around + bround;

            
            bc[3] = bc3;
            double[] axbc = TrianglePool.AllocDoubleArray(8);
            axbclen = ScaleExpansionZeroElim(4, bc, adx, axbc);
            double[] axxbc = TrianglePool.AllocDoubleArray(16);
            axxbclen = ScaleExpansionZeroElim(axbclen, axbc, adx, axxbc);
            TrianglePool.FreeDoubleArray(axbc);
            double[] aybc = TrianglePool.AllocDoubleArray(8);
            aybclen = ScaleExpansionZeroElim(4, bc, ady, aybc);
            double[] ayybc = TrianglePool.AllocDoubleArray(16);
            ayybclen = ScaleExpansionZeroElim(aybclen, aybc, ady, ayybc);
            TrianglePool.FreeDoubleArray(aybc);
            double[] adet = TrianglePool.AllocDoubleArray(32);
            alen = FastExpansionSumZeroElim(axxbclen, axxbc, ayybclen, ayybc, adet);
            TrianglePool.FreeDoubleArray(axxbc);
            TrianglePool.FreeDoubleArray(ayybc);


            cdxady1 = (cdx * ady); c = (splitter * cdx); abig = (c - cdx); ahi = c - abig; alo = cdx - ahi; c = (splitter * ady); abig = (c - ady); bhi = c - abig; blo = ady - bhi; err1 = cdxady1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); cdxady0 = (alo * blo) - err3;
            adxcdy1 = (adx * cdy); c = (splitter * adx); abig = (c - adx); ahi = c - abig; alo = adx - ahi; c = (splitter * cdy); abig = (c - cdy); bhi = c - abig; blo = cdy - bhi; err1 = adxcdy1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); adxcdy0 = (alo * blo) - err3;

            double[] ca = TrianglePool.AllocDoubleArray(4);
            _i = (cdxady0 - adxcdy0); bvirt = (cdxady0 - _i); avirt = _i + bvirt; bround = bvirt - adxcdy0; around = cdxady0 - avirt; ca[0] = around + bround; _j = (cdxady1 + _i); bvirt = (_j - cdxady1); avirt = _j - bvirt; bround = _i - bvirt; around = cdxady1 - avirt; _0 = around + bround; _i = (_0 - adxcdy1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - adxcdy1; around = _0 - avirt; ca[1] = around + bround; ca3 = (_j + _i); bvirt = (ca3 - _j); avirt = ca3 - bvirt; bround = _i - bvirt; around = _j - avirt; ca[2] = around + bround;
            
            ca[3] = ca3;
            double[] bxca = TrianglePool.AllocDoubleArray(8);
            bxcalen = ScaleExpansionZeroElim(4, ca, bdx, bxca);
            double[] bxxca = TrianglePool.AllocDoubleArray(16);
            bxxcalen = ScaleExpansionZeroElim(bxcalen, bxca, bdx, bxxca);
            TrianglePool.FreeDoubleArray(bxca);
            double[] byca = TrianglePool.AllocDoubleArray(8);
            bycalen = ScaleExpansionZeroElim(4, ca, bdy, byca);
            double[] byyca = TrianglePool.AllocDoubleArray(16);
            byycalen = ScaleExpansionZeroElim(bycalen, byca, bdy, byyca);
            TrianglePool.FreeDoubleArray(byca);
            double[] bdet = TrianglePool.AllocDoubleArray(32);
            blen = FastExpansionSumZeroElim(bxxcalen, bxxca, byycalen, byyca, bdet);
            TrianglePool.FreeDoubleArray(bxxca);
            TrianglePool.FreeDoubleArray(byyca);

            adxbdy1 = (adx * bdy); c = (splitter * adx); abig = (c - adx); ahi = c - abig; alo = adx - ahi; c = (splitter * bdy); abig = (c - bdy); bhi = c - abig; blo = bdy - bhi; err1 = adxbdy1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); adxbdy0 = (alo * blo) - err3;
            bdxady1 = (bdx * ady); c = (splitter * bdx); abig = (c - bdx); ahi = c - abig; alo = bdx - ahi; c = (splitter * ady); abig = (c - ady); bhi = c - abig; blo = ady - bhi; err1 = bdxady1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); bdxady0 = (alo * blo) - err3;
            double[] ab = TrianglePool.AllocDoubleArray(4);
            _i = (adxbdy0 - bdxady0); bvirt = (adxbdy0 - _i); avirt = _i + bvirt; bround = bvirt - bdxady0; around = adxbdy0 - avirt; ab[0] = around + bround; _j = (adxbdy1 + _i); bvirt = (_j - adxbdy1); avirt = _j - bvirt; bround = _i - bvirt; around = adxbdy1 - avirt; _0 = around + bround; _i = (_0 - bdxady1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - bdxady1; around = _0 - avirt; ab[1] = around + bround; ab3 = (_j + _i); bvirt = (ab3 - _j); avirt = ab3 - bvirt; bround = _i - bvirt; around = _j - avirt; ab[2] = around + bround;
            
            ab[3] = ab3;
            double[] cxab = TrianglePool.AllocDoubleArray(8);
            cxablen = ScaleExpansionZeroElim(4, ab, cdx, cxab);
            double[] cxxab = TrianglePool.AllocDoubleArray(16);
            cxxablen = ScaleExpansionZeroElim(cxablen, cxab, cdx, cxxab);
            TrianglePool.FreeDoubleArray(cxab);
            double[] cyab = TrianglePool.AllocDoubleArray(8);
            cyablen = ScaleExpansionZeroElim(4, ab, cdy, cyab);
            double[] cyyab = TrianglePool.AllocDoubleArray(16);
            cyyablen = ScaleExpansionZeroElim(cyablen, cyab, cdy, cyyab);
            TrianglePool.FreeDoubleArray(cyab);
            double[] cdet = TrianglePool.AllocDoubleArray(32);
            clen = FastExpansionSumZeroElim(cxxablen, cxxab, cyyablen, cyyab, cdet);
            TrianglePool.FreeDoubleArray(cyyab);
            TrianglePool.FreeDoubleArray(cxxab);

            double[] abdet = TrianglePool.AllocDoubleArray(64);
            ablen = FastExpansionSumZeroElim(alen, adet, blen, bdet, abdet);
            TrianglePool.FreeDoubleArray(adet);
            TrianglePool.FreeDoubleArray(bdet);
            double[] fin1 = TrianglePool.AllocDoubleArray(1152);
            finlength = FastExpansionSumZeroElim(ablen, abdet, clen, cdet, fin1);
            TrianglePool.FreeDoubleArray(abdet);
            TrianglePool.FreeDoubleArray(cdet);

            det = Estimate(finlength, fin1);
            errbound = iccerrboundB * permanent;
            if ((det >= errbound) || (-det >= errbound))
            {
                TrianglePool.FreeDoubleArray(ab);
                TrianglePool.FreeDoubleArray(bc);
                TrianglePool.FreeDoubleArray(ca);
                TrianglePool.FreeDoubleArray(fin1);
                return det;
            }

            bvirt = (pa.X - adx); avirt = adx + bvirt; bround = bvirt - pd.X; around = pa.X - avirt; adxtail = around + bround;
            bvirt = (pa.Y - ady); avirt = ady + bvirt; bround = bvirt - pd.Y; around = pa.Y - avirt; adytail = around + bround;
            bvirt = (pb.X - bdx); avirt = bdx + bvirt; bround = bvirt - pd.X; around = pb.X - avirt; bdxtail = around + bround;
            bvirt = (pb.Y - bdy); avirt = bdy + bvirt; bround = bvirt - pd.Y; around = pb.Y - avirt; bdytail = around + bround;
            bvirt = (pc.X - cdx); avirt = cdx + bvirt; bround = bvirt - pd.X; around = pc.X - avirt; cdxtail = around + bround;
            bvirt = (pc.Y - cdy); avirt = cdy + bvirt; bround = bvirt - pd.Y; around = pc.Y - avirt; cdytail = around + bround;
            if ((adxtail == 0.0) && (bdxtail == 0.0) && (cdxtail == 0.0)
                && (adytail == 0.0) && (bdytail == 0.0) && (cdytail == 0.0))
            {
                TrianglePool.FreeDoubleArray(ab);
                TrianglePool.FreeDoubleArray(bc);
                TrianglePool.FreeDoubleArray(ca);
                TrianglePool.FreeDoubleArray(fin1);
                return det;
            }

            errbound = iccerrboundC * permanent + resulterrbound * ((det) >= 0.0 ? (det) : -(det));
            det += ((adx * adx + ady * ady) * ((bdx * cdytail + cdy * bdxtail) - (bdy * cdxtail + cdx * bdytail))
                    + 2.0 * (adx * adxtail + ady * adytail) * (bdx * cdy - bdy * cdx))
                 + ((bdx * bdx + bdy * bdy) * ((cdx * adytail + ady * cdxtail) - (cdy * adxtail + adx * cdytail))
                    + 2.0 * (bdx * bdxtail + bdy * bdytail) * (cdx * ady - cdy * adx))
                 + ((cdx * cdx + cdy * cdy) * ((adx * bdytail + bdy * adxtail) - (ady * bdxtail + bdx * adytail))
                    + 2.0 * (cdx * cdxtail + cdy * cdytail) * (adx * bdy - ady * bdx));
            if ((det >= errbound) || (-det >= errbound))
            {
                TrianglePool.FreeDoubleArray(ab);
                TrianglePool.FreeDoubleArray(bc);
                TrianglePool.FreeDoubleArray(ca);
                TrianglePool.FreeDoubleArray(fin1);
                return det;
            }

            finnow = fin1;
            double[] fin2 = TrianglePool.AllocDoubleArray(1152);
            finother = fin2;

            double[] aa = TrianglePool.AllocDoubleArray(4);
            double[] bb = TrianglePool.AllocDoubleArray(4);
            double[] cc = TrianglePool.AllocDoubleArray(4);

            double[] temp16a = TrianglePool.AllocDoubleArray(16), temp16b = TrianglePool.AllocDoubleArray(16), 
                temp16c = TrianglePool.AllocDoubleArray(16);
            double[] temp32a = TrianglePool.AllocDoubleArray(32), temp32b = TrianglePool.AllocDoubleArray(32),
                temp48 = TrianglePool.AllocDoubleArray(48), temp64 = TrianglePool.AllocDoubleArray(64);

            if ((bdxtail != 0.0) || (bdytail != 0.0)
                || (cdxtail != 0.0) || (cdytail != 0.0))
            {
                adxadx1 = (adx * adx); c = (splitter * adx); abig = (c - adx); ahi = c - abig; alo = adx - ahi; err1 = adxadx1 - (ahi * ahi); err3 = err1 - ((ahi + ahi) * alo); adxadx0 = (alo * alo) - err3;
                adyady1 = (ady * ady); c = (splitter * ady); abig = (c - ady); ahi = c - abig; alo = ady - ahi; err1 = adyady1 - (ahi * ahi); err3 = err1 - ((ahi + ahi) * alo); adyady0 = (alo * alo) - err3;
                _i = (adxadx0 + adyady0); bvirt = (_i - adxadx0); avirt = _i - bvirt; bround = adyady0 - bvirt; around = adxadx0 - avirt; aa[0] = around + bround; _j = (adxadx1 + _i); bvirt = (_j - adxadx1); avirt = _j - bvirt; bround = _i - bvirt; around = adxadx1 - avirt; _0 = around + bround; _i = (_0 + adyady1); bvirt = (_i - _0); avirt = _i - bvirt; bround = adyady1 - bvirt; around = _0 - avirt; aa[1] = around + bround; aa3 = (_j + _i); bvirt = (aa3 - _j); avirt = aa3 - bvirt; bround = _i - bvirt; around = _j - avirt; aa[2] = around + bround;
                aa[3] = aa3;
            }
            if ((cdxtail != 0.0) || (cdytail != 0.0)
                || (adxtail != 0.0) || (adytail != 0.0))
            {
                bdxbdx1 = (bdx * bdx); c = (splitter * bdx); abig = (c - bdx); ahi = c - abig; alo = bdx - ahi; err1 = bdxbdx1 - (ahi * ahi); err3 = err1 - ((ahi + ahi) * alo); bdxbdx0 = (alo * alo) - err3;
                bdybdy1 = (bdy * bdy); c = (splitter * bdy); abig = (c - bdy); ahi = c - abig; alo = bdy - ahi; err1 = bdybdy1 - (ahi * ahi); err3 = err1 - ((ahi + ahi) * alo); bdybdy0 = (alo * alo) - err3;
                _i = (bdxbdx0 + bdybdy0); bvirt = (_i - bdxbdx0); avirt = _i - bvirt; bround = bdybdy0 - bvirt; around = bdxbdx0 - avirt; bb[0] = around + bround; _j = (bdxbdx1 + _i); bvirt = (_j - bdxbdx1); avirt = _j - bvirt; bround = _i - bvirt; around = bdxbdx1 - avirt; _0 = around + bround; _i = (_0 + bdybdy1); bvirt = (_i - _0); avirt = _i - bvirt; bround = bdybdy1 - bvirt; around = _0 - avirt; bb[1] = around + bround; bb3 = (_j + _i); bvirt = (bb3 - _j); avirt = bb3 - bvirt; bround = _i - bvirt; around = _j - avirt; bb[2] = around + bround;
                bb[3] = bb3;
            }
            if ((adxtail != 0.0) || (adytail != 0.0)
                || (bdxtail != 0.0) || (bdytail != 0.0))
            {
                cdxcdx1 = (cdx * cdx); c = (splitter * cdx); abig = (c - cdx); ahi = c - abig; alo = cdx - ahi; err1 = cdxcdx1 - (ahi * ahi); err3 = err1 - ((ahi + ahi) * alo); cdxcdx0 = (alo * alo) - err3;
                cdycdy1 = (cdy * cdy); c = (splitter * cdy); abig = (c - cdy); ahi = c - abig; alo = cdy - ahi; err1 = cdycdy1 - (ahi * ahi); err3 = err1 - ((ahi + ahi) * alo); cdycdy0 = (alo * alo) - err3;
                _i = (cdxcdx0 + cdycdy0); bvirt = (_i - cdxcdx0); avirt = _i - bvirt; bround = cdycdy0 - bvirt; around = cdxcdx0 - avirt; cc[0] = around + bround; _j = (cdxcdx1 + _i); bvirt = (_j - cdxcdx1); avirt = _j - bvirt; bround = _i - bvirt; around = cdxcdx1 - avirt; _0 = around + bround; _i = (_0 + cdycdy1); bvirt = (_i - _0); avirt = _i - bvirt; bround = cdycdy1 - bvirt; around = _0 - avirt; cc[1] = around + bround; cc3 = (_j + _i); bvirt = (cc3 - _j); avirt = cc3 - bvirt; bround = _i - bvirt; around = _j - avirt; cc[2] = around + bround;
                cc[3] = cc3;
            }


            double[] axtbc = TrianglePool.AllocDoubleArray(8);
            if (adxtail != 0.0)
            {
                axtbclen = ScaleExpansionZeroElim(4, bc, adxtail, axtbc);
                temp16alen = ScaleExpansionZeroElim(axtbclen, axtbc, 2.0 * adx, temp16a);

                double[] axtcc = TrianglePool.AllocDoubleArray(8);
                axtcclen = ScaleExpansionZeroElim(4, cc, adxtail, axtcc);
                temp16blen = ScaleExpansionZeroElim(axtcclen, axtcc, bdy, temp16b);
                TrianglePool.FreeDoubleArray(axtcc);

                double[] axtbb = TrianglePool.AllocDoubleArray(8); 
                axtbblen = ScaleExpansionZeroElim(4, bb, adxtail, axtbb);
                temp16clen = ScaleExpansionZeroElim(axtbblen, axtbb, -cdy, temp16c);
                TrianglePool.FreeDoubleArray(axtbb);

                temp32alen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32a);
                temp48len = FastExpansionSumZeroElim(temp16clen, temp16c, temp32alen, temp32a, temp48);
                finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                finswap = finnow; finnow = finother; finother = finswap;
            }

            double[] aytbc = TrianglePool.AllocDoubleArray(8); 
            if (adytail != 0.0)
            {
                aytbclen = ScaleExpansionZeroElim(4, bc, adytail, aytbc);
                temp16alen = ScaleExpansionZeroElim(aytbclen, aytbc, 2.0 * ady, temp16a);

                double[] aytbb = TrianglePool.AllocDoubleArray(8);
                aytbblen = ScaleExpansionZeroElim(4, bb, adytail, aytbb);
                temp16blen = ScaleExpansionZeroElim(aytbblen, aytbb, cdx, temp16b);
                TrianglePool.FreeDoubleArray(aytbb);

                double[] aytcc = TrianglePool.AllocDoubleArray(8);
                aytcclen = ScaleExpansionZeroElim(4, cc, adytail, aytcc);
                temp16clen = ScaleExpansionZeroElim(aytcclen, aytcc, -bdx, temp16c);
                TrianglePool.FreeDoubleArray(aytcc);

                temp32alen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32a);
                temp48len = FastExpansionSumZeroElim(temp16clen, temp16c, temp32alen, temp32a, temp48);
                finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                finswap = finnow; finnow = finother; finother = finswap;
            }
            TrianglePool.FreeDoubleArray(bc);

            double[] bxtca = TrianglePool.AllocDoubleArray(8);
            if (bdxtail != 0.0)
            {
                bxtcalen = ScaleExpansionZeroElim(4, ca, bdxtail, bxtca);
                temp16alen = ScaleExpansionZeroElim(bxtcalen, bxtca, 2.0 * bdx, temp16a);

                double[] bxtaa = TrianglePool.AllocDoubleArray(8);
                bxtaalen = ScaleExpansionZeroElim(4, aa, bdxtail, bxtaa);
                temp16blen = ScaleExpansionZeroElim(bxtaalen, bxtaa, cdy, temp16b);
                TrianglePool.FreeDoubleArray(bxtaa);

                double[] bxtcc = TrianglePool.AllocDoubleArray(8);
                bxtcclen = ScaleExpansionZeroElim(4, cc, bdxtail, bxtcc);
                temp16clen = ScaleExpansionZeroElim(bxtcclen, bxtcc, -ady, temp16c);
                TrianglePool.FreeDoubleArray(bxtcc);

                temp32alen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32a);
                temp48len = FastExpansionSumZeroElim(temp16clen, temp16c, temp32alen, temp32a, temp48);
                finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                finswap = finnow; finnow = finother; finother = finswap;
            }

            double[] bytca = TrianglePool.AllocDoubleArray(8);
            if (bdytail != 0.0)
            {
                bytcalen = ScaleExpansionZeroElim(4, ca, bdytail, bytca);
                temp16alen = ScaleExpansionZeroElim(bytcalen, bytca, 2.0 * bdy, temp16a);

                double[] bytcc = TrianglePool.AllocDoubleArray(8);
                bytcclen = ScaleExpansionZeroElim(4, cc, bdytail, bytcc);
                temp16blen = ScaleExpansionZeroElim(bytcclen, bytcc, adx, temp16b);
                TrianglePool.FreeDoubleArray(bytcc);

                double[] bytaa = TrianglePool.AllocDoubleArray(8);
                bytaalen = ScaleExpansionZeroElim(4, aa, bdytail, bytaa);
                temp16clen = ScaleExpansionZeroElim(bytaalen, bytaa, -cdx, temp16c);
                TrianglePool.FreeDoubleArray(bytaa);

                temp32alen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32a);
                temp48len = FastExpansionSumZeroElim(temp16clen, temp16c, temp32alen, temp32a, temp48);
                finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                finswap = finnow; finnow = finother; finother = finswap;
            }
            TrianglePool.FreeDoubleArray(ca);

            double[] cxtab = TrianglePool.AllocDoubleArray(8); 
            if (cdxtail != 0.0)
            {
                cxtablen = ScaleExpansionZeroElim(4, ab, cdxtail, cxtab);
                temp16alen = ScaleExpansionZeroElim(cxtablen, cxtab, 2.0 * cdx, temp16a);

                double[] cxtbb = TrianglePool.AllocDoubleArray(8);
                cxtbblen = ScaleExpansionZeroElim(4, bb, cdxtail, cxtbb);
                temp16blen = ScaleExpansionZeroElim(cxtbblen, cxtbb, ady, temp16b);
                TrianglePool.FreeDoubleArray(cxtbb);

                double[] cxtaa = TrianglePool.AllocDoubleArray(8);
                cxtaalen = ScaleExpansionZeroElim(4, aa, cdxtail, cxtaa);
                temp16clen = ScaleExpansionZeroElim(cxtaalen, cxtaa, -bdy, temp16c);
                TrianglePool.FreeDoubleArray(cxtaa);

                temp32alen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32a);
                temp48len = FastExpansionSumZeroElim(temp16clen, temp16c, temp32alen, temp32a, temp48);
                finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                finswap = finnow; finnow = finother; finother = finswap;
            }

            double[] cytab = TrianglePool.AllocDoubleArray(8);
            if (cdytail != 0.0)
            {
                cytablen = ScaleExpansionZeroElim(4, ab, cdytail, cytab);
                temp16alen = ScaleExpansionZeroElim(cytablen, cytab, 2.0 * cdy, temp16a);

                double[] cytaa = TrianglePool.AllocDoubleArray(8);
                cytaalen = ScaleExpansionZeroElim(4, aa, cdytail, cytaa);
                temp16blen = ScaleExpansionZeroElim(cytaalen, cytaa, bdx, temp16b);
                TrianglePool.FreeDoubleArray(cytaa);

                double[] cytbb = TrianglePool.AllocDoubleArray(8);
                cytbblen = ScaleExpansionZeroElim(4, bb, cdytail, cytbb);
                temp16clen = ScaleExpansionZeroElim(cytbblen, cytbb, -adx, temp16c);
                TrianglePool.FreeDoubleArray(cytbb);

                temp32alen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32a);
                temp48len = FastExpansionSumZeroElim(temp16clen, temp16c, temp32alen, temp32a, temp48);
                finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                finswap = finnow; finnow = finother; finother = finswap;
            }
            
            TrianglePool.FreeDoubleArray(ab);

            // Edited to work around index out of range exceptions (changed array length from 4 to 5).
            // See unsafe indexing in FastExpansionSumZeroElim.
            double[] v = TrianglePool.AllocDoubleArray(5);
            double[] u = TrianglePool.AllocDoubleArray(5);
            double[] temp8 = TrianglePool.AllocDoubleArray(8);

            if ((adxtail != 0.0) || (adytail != 0.0))
            {
                double[] bctt = TrianglePool.AllocDoubleArray(8);
                double[] bct = TrianglePool.AllocDoubleArray(8);
                if ((bdxtail != 0.0) || (bdytail != 0.0)
                    || (cdxtail != 0.0) || (cdytail != 0.0))
                {
                    ti1 = (bdxtail * cdy); c = (splitter * bdxtail); abig = (c - bdxtail); ahi = c - abig; alo = bdxtail - ahi; c = (splitter * cdy); abig = (c - cdy); bhi = c - abig; blo = cdy - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    tj1 = (bdx * cdytail); c = (splitter * bdx); abig = (c - bdx); ahi = c - abig; alo = bdx - ahi; c = (splitter * cdytail); abig = (c - cdytail); bhi = c - abig; blo = cdytail - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 + tj0); bvirt = (_i - ti0); avirt = _i - bvirt; bround = tj0 - bvirt; around = ti0 - avirt; u[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 + tj1); bvirt = (_i - _0); avirt = _i - bvirt; bround = tj1 - bvirt; around = _0 - avirt; u[1] = around + bround; u3 = (_j + _i); bvirt = (u3 - _j); avirt = u3 - bvirt; bround = _i - bvirt; around = _j - avirt; u[2] = around + bround;
                    u[3] = u3;
                    negate = -bdy;
                    ti1 = (cdxtail * negate); c = (splitter * cdxtail); abig = (c - cdxtail); ahi = c - abig; alo = cdxtail - ahi; c = (splitter * negate); abig = (c - negate); bhi = c - abig; blo = negate - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    negate = -bdytail;
                    tj1 = (cdx * negate); c = (splitter * cdx); abig = (c - cdx); ahi = c - abig; alo = cdx - ahi; c = (splitter * negate); abig = (c - negate); bhi = c - abig; blo = negate - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 + tj0); bvirt = (_i - ti0); avirt = _i - bvirt; bround = tj0 - bvirt; around = ti0 - avirt; v[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 + tj1); bvirt = (_i - _0); avirt = _i - bvirt; bround = tj1 - bvirt; around = _0 - avirt; v[1] = around + bround; v3 = (_j + _i); bvirt = (v3 - _j); avirt = v3 - bvirt; bround = _i - bvirt; around = _j - avirt; v[2] = around + bround;
                    v[3] = v3;
                    bctlen = FastExpansionSumZeroElim(4, u, 4, v, bct);

                    ti1 = (bdxtail * cdytail); c = (splitter * bdxtail); abig = (c - bdxtail); ahi = c - abig; alo = bdxtail - ahi; c = (splitter * cdytail); abig = (c - cdytail); bhi = c - abig; blo = cdytail - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    tj1 = (cdxtail * bdytail); c = (splitter * cdxtail); abig = (c - cdxtail); ahi = c - abig; alo = cdxtail - ahi; c = (splitter * bdytail); abig = (c - bdytail); bhi = c - abig; blo = bdytail - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 - tj0); bvirt = (ti0 - _i); avirt = _i + bvirt; bround = bvirt - tj0; around = ti0 - avirt; bctt[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 - tj1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - tj1; around = _0 - avirt; bctt[1] = around + bround; bctt3 = (_j + _i); bvirt = (bctt3 - _j); avirt = bctt3 - bvirt; bround = _i - bvirt; around = _j - avirt; bctt[2] = around + bround;
                    bctt[3] = bctt3;
                    bcttlen = 4;
                }
                else
                {
                    bct[0] = 0.0;
                    bctlen = 1;
                    bctt[0] = 0.0;
                    bcttlen = 1;
                }

                if (adxtail != 0.0)
                {
                    double[] axtbct = TrianglePool.AllocDoubleArray(16);
                    temp16alen = ScaleExpansionZeroElim(axtbclen, axtbc, adxtail, temp16a);
                    axtbctlen = ScaleExpansionZeroElim(bctlen, bct, adxtail, axtbct);
                    temp32alen = ScaleExpansionZeroElim(axtbctlen, axtbct, 2.0 * adx, temp32a);
                    temp48len = FastExpansionSumZeroElim(temp16alen, temp16a, temp32alen, temp32a, temp48);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                    if (bdytail != 0.0)
                    {
                        temp8len = ScaleExpansionZeroElim(4, cc, adxtail, temp8);
                        temp16alen = ScaleExpansionZeroElim(temp8len, temp8, bdytail, temp16a);
                        finlength = FastExpansionSumZeroElim(finlength, finnow, temp16alen, temp16a, finother);
                        finswap = finnow; finnow = finother; finother = finswap;
                    }
                    if (cdytail != 0.0)
                    {
                        temp8len = ScaleExpansionZeroElim(4, bb, -adxtail, temp8);
                        temp16alen = ScaleExpansionZeroElim(temp8len, temp8, cdytail, temp16a);
                        finlength = FastExpansionSumZeroElim(finlength, finnow, temp16alen, temp16a, finother);
                        finswap = finnow; finnow = finother; finother = finswap;
                    }

                    temp32alen = ScaleExpansionZeroElim(axtbctlen, axtbct, adxtail, temp32a);
                    TrianglePool.FreeDoubleArray(axtbct);
                    double[] axtbctt = TrianglePool.AllocDoubleArray(8);
                    axtbcttlen = ScaleExpansionZeroElim(bcttlen, bctt, adxtail, axtbctt);
                    temp16alen = ScaleExpansionZeroElim(axtbcttlen, axtbctt, 2.0 * adx, temp16a);
                    temp16blen = ScaleExpansionZeroElim(axtbcttlen, axtbctt, adxtail, temp16b);
                    TrianglePool.FreeDoubleArray(axtbctt);
                    temp32blen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32b);
                    temp64len = FastExpansionSumZeroElim(temp32alen, temp32a, temp32blen, temp32b, temp64);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp64len, temp64, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                }
                if (adytail != 0.0)
                {
                    double[] aytbct = TrianglePool.AllocDoubleArray(16);
                    temp16alen = ScaleExpansionZeroElim(aytbclen, aytbc, adytail, temp16a);
                    aytbctlen = ScaleExpansionZeroElim(bctlen, bct, adytail, aytbct);
                    temp32alen = ScaleExpansionZeroElim(aytbctlen, aytbct, 2.0 * ady, temp32a);
                    temp48len = FastExpansionSumZeroElim(temp16alen, temp16a, temp32alen, temp32a, temp48);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                    finswap = finnow; finnow = finother; finother = finswap;


                    temp32alen = ScaleExpansionZeroElim(aytbctlen, aytbct, adytail, temp32a);
                    TrianglePool.FreeDoubleArray(aytbct);
                    double[] aytbctt = TrianglePool.AllocDoubleArray(8);
                    aytbcttlen = ScaleExpansionZeroElim(bcttlen, bctt, adytail, aytbctt);
                    temp16alen = ScaleExpansionZeroElim(aytbcttlen, aytbctt, 2.0 * ady, temp16a);
                    temp16blen = ScaleExpansionZeroElim(aytbcttlen, aytbctt, adytail, temp16b);
                    TrianglePool.FreeDoubleArray(aytbctt);
                    temp32blen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32b);
                    temp64len = FastExpansionSumZeroElim(temp32alen, temp32a, temp32blen, temp32b, temp64);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp64len, temp64, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                }
                TrianglePool.FreeDoubleArray(bctt);
                TrianglePool.FreeDoubleArray(bct);
            }
            if ((bdxtail != 0.0) || (bdytail != 0.0))
            {
                double[] catt = TrianglePool.AllocDoubleArray(4);
                double[] cat = TrianglePool.AllocDoubleArray(8);
                if ((cdxtail != 0.0) || (cdytail != 0.0)
                    || (adxtail != 0.0) || (adytail != 0.0))
                {
                    ti1 = (cdxtail * ady); c = (splitter * cdxtail); abig = (c - cdxtail); ahi = c - abig; alo = cdxtail - ahi; c = (splitter * ady); abig = (c - ady); bhi = c - abig; blo = ady - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    tj1 = (cdx * adytail); c = (splitter * cdx); abig = (c - cdx); ahi = c - abig; alo = cdx - ahi; c = (splitter * adytail); abig = (c - adytail); bhi = c - abig; blo = adytail - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 + tj0); bvirt = (_i - ti0); avirt = _i - bvirt; bround = tj0 - bvirt; around = ti0 - avirt; u[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 + tj1); bvirt = (_i - _0); avirt = _i - bvirt; bround = tj1 - bvirt; around = _0 - avirt; u[1] = around + bround; u3 = (_j + _i); bvirt = (u3 - _j); avirt = u3 - bvirt; bround = _i - bvirt; around = _j - avirt; u[2] = around + bround;
                    u[3] = u3;
                    negate = -cdy;
                    ti1 = (adxtail * negate); c = (splitter * adxtail); abig = (c - adxtail); ahi = c - abig; alo = adxtail - ahi; c = (splitter * negate); abig = (c - negate); bhi = c - abig; blo = negate - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    negate = -cdytail;
                    tj1 = (adx * negate); c = (splitter * adx); abig = (c - adx); ahi = c - abig; alo = adx - ahi; c = (splitter * negate); abig = (c - negate); bhi = c - abig; blo = negate - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 + tj0); bvirt = (_i - ti0); avirt = _i - bvirt; bround = tj0 - bvirt; around = ti0 - avirt; v[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 + tj1); bvirt = (_i - _0); avirt = _i - bvirt; bround = tj1 - bvirt; around = _0 - avirt; v[1] = around + bround; v3 = (_j + _i); bvirt = (v3 - _j); avirt = v3 - bvirt; bround = _i - bvirt; around = _j - avirt; v[2] = around + bround;
                    v[3] = v3;
                    catlen = FastExpansionSumZeroElim(4, u, 4, v, cat);

                    ti1 = (cdxtail * adytail); c = (splitter * cdxtail); abig = (c - cdxtail); ahi = c - abig; alo = cdxtail - ahi; c = (splitter * adytail); abig = (c - adytail); bhi = c - abig; blo = adytail - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    tj1 = (adxtail * cdytail); c = (splitter * adxtail); abig = (c - adxtail); ahi = c - abig; alo = adxtail - ahi; c = (splitter * cdytail); abig = (c - cdytail); bhi = c - abig; blo = cdytail - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 - tj0); bvirt = (ti0 - _i); avirt = _i + bvirt; bround = bvirt - tj0; around = ti0 - avirt; catt[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 - tj1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - tj1; around = _0 - avirt; catt[1] = around + bround; catt3 = (_j + _i); bvirt = (catt3 - _j); avirt = catt3 - bvirt; bround = _i - bvirt; around = _j - avirt; catt[2] = around + bround;
                    catt[3] = catt3;
                    cattlen = 4;
                }
                else
                {
                    cat[0] = 0.0;
                    catlen = 1;
                    catt[0] = 0.0;
                    cattlen = 1;
                }

                if (bdxtail != 0.0)
                {
                    double[] bxtcat = TrianglePool.AllocDoubleArray(16);
                    temp16alen = ScaleExpansionZeroElim(bxtcalen, bxtca, bdxtail, temp16a);
                    bxtcatlen = ScaleExpansionZeroElim(catlen, cat, bdxtail, bxtcat);
                    temp32alen = ScaleExpansionZeroElim(bxtcatlen, bxtcat, 2.0 * bdx, temp32a);
                    temp48len = FastExpansionSumZeroElim(temp16alen, temp16a, temp32alen, temp32a, temp48);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                    if (cdytail != 0.0)
                    {
                        temp8len = ScaleExpansionZeroElim(4, aa, bdxtail, temp8);
                        temp16alen = ScaleExpansionZeroElim(temp8len, temp8, cdytail, temp16a);
                        finlength = FastExpansionSumZeroElim(finlength, finnow, temp16alen, temp16a, finother);
                        finswap = finnow; finnow = finother; finother = finswap;
                    }
                    if (adytail != 0.0)
                    {
                        temp8len = ScaleExpansionZeroElim(4, cc, -bdxtail, temp8);
                        temp16alen = ScaleExpansionZeroElim(temp8len, temp8, adytail, temp16a);
                        finlength = FastExpansionSumZeroElim(finlength, finnow, temp16alen, temp16a, finother);
                        finswap = finnow; finnow = finother; finother = finswap;
                    }

                    temp32alen = ScaleExpansionZeroElim(bxtcatlen, bxtcat, bdxtail, temp32a);
                    TrianglePool.FreeDoubleArray(bxtcat);
                    double[] bxtcatt = TrianglePool.AllocDoubleArray(16);
                    bxtcattlen = ScaleExpansionZeroElim(cattlen, catt, bdxtail, bxtcatt);
                    temp16alen = ScaleExpansionZeroElim(bxtcattlen, bxtcatt, 2.0 * bdx, temp16a);
                    temp16blen = ScaleExpansionZeroElim(bxtcattlen, bxtcatt, bdxtail, temp16b);
                    TrianglePool.FreeDoubleArray(bxtcatt);
                    temp32blen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32b);
                    temp64len = FastExpansionSumZeroElim(temp32alen, temp32a, temp32blen, temp32b, temp64);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp64len, temp64, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                }
                if (bdytail != 0.0)
                {
                    double[] bytcat = TrianglePool.AllocDoubleArray(16);
                    temp16alen = ScaleExpansionZeroElim(bytcalen, bytca, bdytail, temp16a);
                    bytcatlen = ScaleExpansionZeroElim(catlen, cat, bdytail, bytcat);
                    temp32alen = ScaleExpansionZeroElim(bytcatlen, bytcat, 2.0 * bdy, temp32a);
                    temp48len = FastExpansionSumZeroElim(temp16alen, temp16a, temp32alen, temp32a, temp48);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                    finswap = finnow; finnow = finother; finother = finswap;

                    temp32alen = ScaleExpansionZeroElim(bytcatlen, bytcat, bdytail, temp32a);
                    TrianglePool.FreeDoubleArray(bytcat);
                    double[] bytcatt = TrianglePool.AllocDoubleArray(8);
                    bytcattlen = ScaleExpansionZeroElim(cattlen, catt, bdytail, bytcatt);
                    temp16alen = ScaleExpansionZeroElim(bytcattlen, bytcatt, 2.0 * bdy, temp16a);
                    temp16blen = ScaleExpansionZeroElim(bytcattlen, bytcatt, bdytail, temp16b);
                    TrianglePool.FreeDoubleArray(bytcatt);
                    temp32blen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32b);
                    temp64len = FastExpansionSumZeroElim(temp32alen, temp32a, temp32blen, temp32b, temp64);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp64len, temp64, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                }
                TrianglePool.FreeDoubleArray(catt);
                TrianglePool.FreeDoubleArray(cat);
            }
            if ((cdxtail != 0.0) || (cdytail != 0.0))
            {
                double[] abtt = TrianglePool.AllocDoubleArray(4);
                double[] abt = TrianglePool.AllocDoubleArray(8);
                if ((adxtail != 0.0) || (adytail != 0.0)
                    || (bdxtail != 0.0) || (bdytail != 0.0))
                {
                    ti1 = (adxtail * bdy); c = (splitter * adxtail); abig = (c - adxtail); ahi = c - abig; alo = adxtail - ahi; c = (splitter * bdy); abig = (c - bdy); bhi = c - abig; blo = bdy - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    tj1 = (adx * bdytail); c = (splitter * adx); abig = (c - adx); ahi = c - abig; alo = adx - ahi; c = (splitter * bdytail); abig = (c - bdytail); bhi = c - abig; blo = bdytail - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 + tj0); bvirt = (_i - ti0); avirt = _i - bvirt; bround = tj0 - bvirt; around = ti0 - avirt; u[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 + tj1); bvirt = (_i - _0); avirt = _i - bvirt; bround = tj1 - bvirt; around = _0 - avirt; u[1] = around + bround; u3 = (_j + _i); bvirt = (u3 - _j); avirt = u3 - bvirt; bround = _i - bvirt; around = _j - avirt; u[2] = around + bround;
                    u[3] = u3;
                    negate = -ady;
                    ti1 = (bdxtail * negate); c = (splitter * bdxtail); abig = (c - bdxtail); ahi = c - abig; alo = bdxtail - ahi; c = (splitter * negate); abig = (c - negate); bhi = c - abig; blo = negate - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    negate = -adytail;
                    tj1 = (bdx * negate); c = (splitter * bdx); abig = (c - bdx); ahi = c - abig; alo = bdx - ahi; c = (splitter * negate); abig = (c - negate); bhi = c - abig; blo = negate - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 + tj0); bvirt = (_i - ti0); avirt = _i - bvirt; bround = tj0 - bvirt; around = ti0 - avirt; v[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 + tj1); bvirt = (_i - _0); avirt = _i - bvirt; bround = tj1 - bvirt; around = _0 - avirt; v[1] = around + bround; v3 = (_j + _i); bvirt = (v3 - _j); avirt = v3 - bvirt; bround = _i - bvirt; around = _j - avirt; v[2] = around + bround;
                    v[3] = v3;
                    abtlen = FastExpansionSumZeroElim(4, u, 4, v, abt);

                    ti1 = (adxtail * bdytail); c = (splitter * adxtail); abig = (c - adxtail); ahi = c - abig; alo = adxtail - ahi; c = (splitter * bdytail); abig = (c - bdytail); bhi = c - abig; blo = bdytail - bhi; err1 = ti1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); ti0 = (alo * blo) - err3;
                    tj1 = (bdxtail * adytail); c = (splitter * bdxtail); abig = (c - bdxtail); ahi = c - abig; alo = bdxtail - ahi; c = (splitter * adytail); abig = (c - adytail); bhi = c - abig; blo = adytail - bhi; err1 = tj1 - (ahi * bhi); err2 = err1 - (alo * bhi); err3 = err2 - (ahi * blo); tj0 = (alo * blo) - err3;
                    _i = (ti0 - tj0); bvirt = (ti0 - _i); avirt = _i + bvirt; bround = bvirt - tj0; around = ti0 - avirt; abtt[0] = around + bround; _j = (ti1 + _i); bvirt = (_j - ti1); avirt = _j - bvirt; bround = _i - bvirt; around = ti1 - avirt; _0 = around + bround; _i = (_0 - tj1); bvirt = (_0 - _i); avirt = _i + bvirt; bround = bvirt - tj1; around = _0 - avirt; abtt[1] = around + bround; abtt3 = (_j + _i); bvirt = (abtt3 - _j); avirt = abtt3 - bvirt; bround = _i - bvirt; around = _j - avirt; abtt[2] = around + bround;
                    abtt[3] = abtt3;
                    abttlen = 4;
                }
                else
                {
                    abt[0] = 0.0;
                    abtlen = 1;
                    abtt[0] = 0.0;
                    abttlen = 1;
                }

                if (cdxtail != 0.0)
                {
                    double[] cxtabt = TrianglePool.AllocDoubleArray(16);
                    temp16alen = ScaleExpansionZeroElim(cxtablen, cxtab, cdxtail, temp16a);
                    cxtabtlen = ScaleExpansionZeroElim(abtlen, abt, cdxtail, cxtabt);
                    temp32alen = ScaleExpansionZeroElim(cxtabtlen, cxtabt, 2.0 * cdx, temp32a);
                    temp48len = FastExpansionSumZeroElim(temp16alen, temp16a, temp32alen, temp32a, temp48);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                    if (adytail != 0.0)
                    {
                        temp8len = ScaleExpansionZeroElim(4, bb, cdxtail, temp8);
                        temp16alen = ScaleExpansionZeroElim(temp8len, temp8, adytail, temp16a);
                        finlength = FastExpansionSumZeroElim(finlength, finnow, temp16alen, temp16a, finother);
                        finswap = finnow; finnow = finother; finother = finswap;
                    }
                    if (bdytail != 0.0)
                    {
                        temp8len = ScaleExpansionZeroElim(4, aa, -cdxtail, temp8);
                        temp16alen = ScaleExpansionZeroElim(temp8len, temp8, bdytail, temp16a);
                        finlength = FastExpansionSumZeroElim(finlength, finnow, temp16alen, temp16a, finother);
                        finswap = finnow; finnow = finother; finother = finswap;
                    }

                    temp32alen = ScaleExpansionZeroElim(cxtabtlen, cxtabt, cdxtail, temp32a);
                    TrianglePool.FreeDoubleArray(cxtabt);
                    double[] cxtabtt = TrianglePool.AllocDoubleArray(8);
                    cxtabttlen = ScaleExpansionZeroElim(abttlen, abtt, cdxtail, cxtabtt);
                    temp16alen = ScaleExpansionZeroElim(cxtabttlen, cxtabtt, 2.0 * cdx, temp16a);
                    temp16blen = ScaleExpansionZeroElim(cxtabttlen, cxtabtt, cdxtail, temp16b);
                    TrianglePool.FreeDoubleArray(cxtabtt);
                    temp32blen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32b);
                    temp64len = FastExpansionSumZeroElim(temp32alen, temp32a, temp32blen, temp32b, temp64);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp64len, temp64, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                }
                if (cdytail != 0.0)
                {
                    double[] cytabt = TrianglePool.AllocDoubleArray(16);
                    temp16alen = ScaleExpansionZeroElim(cytablen, cytab, cdytail, temp16a);
                    cytabtlen = ScaleExpansionZeroElim(abtlen, abt, cdytail, cytabt);
                    temp32alen = ScaleExpansionZeroElim(cytabtlen, cytabt, 2.0 * cdy, temp32a);
                    temp48len = FastExpansionSumZeroElim(temp16alen, temp16a, temp32alen, temp32a, temp48);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp48len, temp48, finother);
                    finswap = finnow; finnow = finother; finother = finswap;


                    temp32alen = ScaleExpansionZeroElim(cytabtlen, cytabt, cdytail, temp32a);
                    TrianglePool.FreeDoubleArray(cytabt);
                    double[] cytabtt = TrianglePool.AllocDoubleArray(8);
                    cytabttlen = ScaleExpansionZeroElim(abttlen, abtt, cdytail, cytabtt);
                    temp16alen = ScaleExpansionZeroElim(cytabttlen, cytabtt, 2.0 * cdy, temp16a);
                    temp16blen = ScaleExpansionZeroElim(cytabttlen, cytabtt, cdytail, temp16b);
                    TrianglePool.FreeDoubleArray(cytabtt);
                    temp32blen = FastExpansionSumZeroElim(temp16alen, temp16a, temp16blen, temp16b, temp32b);
                    temp64len = FastExpansionSumZeroElim(temp32alen, temp32a, temp32blen, temp32b, temp64);
                    finlength = FastExpansionSumZeroElim(finlength, finnow, temp64len, temp64, finother);
                    finswap = finnow; finnow = finother; finother = finswap;
                }
                TrianglePool.FreeDoubleArray(abt);
                TrianglePool.FreeDoubleArray(abtt);
            }

            var result = finnow[finlength - 1];

            TrianglePool.FreeDoubleArray(axtbc);
            TrianglePool.FreeDoubleArray(aytbc);
            TrianglePool.FreeDoubleArray(bxtca);
            TrianglePool.FreeDoubleArray(bytca);
            TrianglePool.FreeDoubleArray(cxtab);
            TrianglePool.FreeDoubleArray(cytab);
            
            TrianglePool.FreeDoubleArray(v);
            TrianglePool.FreeDoubleArray(u);

            TrianglePool.FreeDoubleArray(temp8);
            TrianglePool.FreeDoubleArray(temp16a);
            TrianglePool.FreeDoubleArray(temp16b);
            TrianglePool.FreeDoubleArray(temp16c);
            TrianglePool.FreeDoubleArray(temp32a);
            TrianglePool.FreeDoubleArray(temp32b);
            TrianglePool.FreeDoubleArray(temp48);
            TrianglePool.FreeDoubleArray(temp64);

            TrianglePool.FreeDoubleArray(aa);
            TrianglePool.FreeDoubleArray(bb);
            TrianglePool.FreeDoubleArray(cc);
            TrianglePool.FreeDoubleArray(fin1);
            TrianglePool.FreeDoubleArray(fin2);

            return result;
        }

        #endregion
    }
}
