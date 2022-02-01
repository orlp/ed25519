namespace GlitchEd25519;

public ref struct ge_p2
{
    public Span<int> X;
    public Span<int> Y;
    public Span<int> Z;
}

public ref struct ge_p3
{
    public Span<int> X;
    public Span<int> Y;
    public Span<int> Z;
    public Span<int> T;
}

public ref struct ge_p1p1
{
    public Span<int> X;
    public Span<int> Y;
    public Span<int> Z;
    public Span<int> T;
}

public ref struct ge_precomp
{
    public Span<int> yplusx;
    public Span<int> yminusx;
    public Span<int> xy2d;
}

public ref struct ge_cached
{
    public Span<int> YplusX;
    public Span<int> YminusX;
    public Span<int> Z;
    public Span<int> T2d;
}

public static partial class GlitchEd25519
{
    private static void slide(ref Span<sbyte> r, ReadOnlySpan<byte> a)
    {
        int i;
        int b;
        int k;

        for (i = 0; i < 256; ++i)
        {
            r[i] = (sbyte)(1 & (a[i >> 3] >> (i & 7)));
        }

        for (i = 0; i < 256; ++i)
        {
            if (r[i] != 0)
            {
                for (b = 1; b <= 6 && i + b < 256; ++b)
                {
                    if (r[i + b] != 0)
                    {
                        if (r[i] + (r[i + b] << b) <= 15)
                        {
                            r[i] += (sbyte)(r[i + b] << b);
                            r[i + b] = 0;
                        }
                        else if (r[i] - (r[i + b] << b) >= -15)
                        {
                            r[i] -= (sbyte)(r[i + b] << b);

                            for (k = i + b; k < 256; ++k)
                            {
                                if (r[k] == 0)
                                {
                                    r[k] = 1;
                                    break;
                                }

                                r[k] = 0;
                            }
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
        }
    }

    private static byte equal(sbyte b, sbyte c)
    {
        byte ub = (byte)b;
        byte uc = (byte)c;
        byte x = (byte)(ub ^ uc); /* 0: yes; 1..255: no */
        ulong y = x; /* 0: yes; 1..255: no */
        y -= 1; /* large: yes; 0..254: no */
        y >>= 63; /* 1: yes; 0: no */
        return (byte)y;
    }

    private static byte negative(sbyte b)
    {
        ulong x = (ulong)b; /* 18446744073709551361..18446744073709551615: yes; 0..255: no */
        x >>= 63; /* 1: yes; 0: no */
        return (byte)x;
    }

    private static void cmov(ref ge_precomp t, ge_precomp u, byte b)
    {
        Span<int> yplusx = stackalloc int[10];
        Span<int> yminusx = stackalloc int[10];
        Span<int> xy2d = stackalloc int[10];

        t.yplusx.CopyTo(yplusx);
        t.yminusx.CopyTo(yminusx);
        t.xy2d.CopyTo(xy2d);

        fe_cmov(ref yplusx, u.yplusx, b);
        fe_cmov(ref yminusx, u.yminusx, b);
        fe_cmov(ref xy2d, u.xy2d, b);

        yplusx.CopyTo(t.yplusx);
        yminusx.CopyTo(t.yminusx);
        xy2d.CopyTo(t.xy2d);
    }

    private static void select(ref ge_precomp t, int pos, sbyte b)
    {
        ge_precomp result = new()
        {
            yminusx = stackalloc int[10],
            yplusx = stackalloc int[10],
            xy2d = stackalloc int[10],
        };

        ge_precomp minust = new()
        {
            yminusx = stackalloc int[10],
            yplusx = stackalloc int[10],
            xy2d = stackalloc int[10],
        };

        ge_precomp p0 = new()
        {
            yminusx = stackalloc int[10],
            yplusx = stackalloc int[10],
            xy2d = stackalloc int[10],
        };

        byte bnegative = negative(b);
        byte babs = (byte)(b - (((-bnegative) & b) << 1));
        fe_1(ref result.yplusx);
        fe_1(ref result.yminusx);
        fe_0(ref result.xy2d);
        fe_0(ref minust.yplusx);
        fe_0(ref minust.yminusx);
        fe_0(ref minust.xy2d);

        GetPrecompBase(pos, 0, ref p0);
        cmov(ref result, p0, equal((sbyte)babs, 1));
        GetPrecompBase(pos, 1, ref p0);
        cmov(ref result, p0, equal((sbyte)babs, 2));
        GetPrecompBase(pos, 2, ref p0);
        cmov(ref result, p0, equal((sbyte)babs, 3));
        GetPrecompBase(pos, 3, ref p0);
        cmov(ref result, p0, equal((sbyte)babs, 4));
        GetPrecompBase(pos, 4, ref p0);
        cmov(ref result, p0, equal((sbyte)babs, 5));
        GetPrecompBase(pos, 5, ref p0);
        cmov(ref result, p0, equal((sbyte)babs, 6));
        GetPrecompBase(pos, 6, ref p0);
        cmov(ref result, p0, equal((sbyte)babs, 7));
        GetPrecompBase(pos, 7, ref p0);
        cmov(ref result, p0, equal((sbyte)babs, 8));

        fe_copy(ref minust.yminusx, result.yplusx);
        fe_copy(ref minust.yplusx, result.yminusx);
        fe_neg(ref minust.xy2d, result.xy2d);
        cmov(ref result, minust, bnegative);

        result.yplusx.CopyTo(t.yplusx);
        result.yminusx.CopyTo(t.yminusx);
        result.xy2d.CopyTo(t.xy2d);
    }

    private static void ge_p3_tobytes(ref Span<byte> s, ge_p3 h)
    {
        Span<int> x = stackalloc int[10];
        Span<int> y = stackalloc int[10];
        Span<int> recip = stackalloc int[10];
        Span<byte> result = stackalloc byte[32];

        fe_invert(ref recip, h.Z);
        fe_mul(ref x, h.X, recip);
        fe_mul(ref y, h.Y, recip);
        fe_tobytes(ref result, y);

        result[31] ^= (byte)(fe_isnegative(x) << 7);
        result.CopyTo(s);
    }

    private static void ge_tobytes(ref Span<byte> s, ge_p2 h)
    {
        Span<int> x = stackalloc int[10];
        Span<int> y = stackalloc int[10];
        Span<int> recip = stackalloc int[10];
        Span<byte> result = stackalloc byte[32];
        s.CopyTo(result);

        fe_invert(ref recip, h.Z);
        fe_mul(ref x, h.X, recip);
        fe_mul(ref y, h.Y, recip);
        fe_tobytes(ref result, y);

        result[31] ^= (byte)(fe_isnegative(x) << 7);
        result.CopyTo(s);
    }

    private static int ge_frombytes_negate_vartime(ref ge_p3 h, ReadOnlySpan<byte> s)
    {
        ReadOnlySpan<int> d = stackalloc int[10]
        {
            -10913610, 13857413, -15372611, 6949391, 114729, -8787816, -6275908, -3247719, -18696448, -12055116,
        };

        ReadOnlySpan<int> sqrtm1 = stackalloc int[10]
        {
            -32595792, -7943725, 9377950, 3500415, 12389472, -272473, -25146209, -2005654, 326686, 11406482,
        };

        Span<int> u = stackalloc int[10];
        Span<int> v = stackalloc int[10];
        Span<int> v3 = stackalloc int[10];
        Span<int> vxx = stackalloc int[10];
        Span<int> check = stackalloc int[10];

        ge_p3 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        h.X.CopyTo(result.X);
        h.Y.CopyTo(result.Y);
        h.Z.CopyTo(result.Z);
        h.T.CopyTo(result.T);

        fe_frombytes(ref result.Y, s);
        fe_1(ref result.Z);
        fe_sq(ref u, result.Y);
        fe_mul(ref v, u, d);
        fe_sub(ref u, u, result.Z); /* u = y^2-1 */
        fe_add(ref v, v, result.Z); /* v = dy^2+1 */
        fe_sq(ref v3, v);
        fe_mul(ref v3, v3, v); /* v3 = v^3 */
        fe_sq(ref result.X, v3);
        fe_mul(ref result.X, result.X, v);
        fe_mul(ref result.X, result.X, u); /* x = uv^7 */
        fe_pow22523(ref result.X, result.X); /* x = (uv^7)^((q-5)/8) */
        fe_mul(ref result.X, result.X, v3);
        fe_mul(ref result.X, result.X, u); /* x = uv^3(uv^7)^((q-5)/8) */
        fe_sq(ref vxx, result.X);
        fe_mul(ref vxx, vxx, v);
        fe_sub(ref check, vxx, u); /* vx^2-u */

        if (fe_isnonzero(check))
        {
            fe_add(ref check, vxx, u); /* vx^2+u */

            if (fe_isnonzero(check))
            {
                return -1;
            }

            fe_mul(ref result.X, result.X, sqrtm1);
        }

        if (fe_isnegative(result.X) == (s[31] >> 7))
        {
            fe_neg(ref result.X, result.X);
        }

        fe_mul(ref result.T, result.X, result.Y);

        result.X.CopyTo(h.X);
        result.Y.CopyTo(h.Y);
        result.Z.CopyTo(h.Z);
        result.T.CopyTo(h.T);

        return 0;
    }

    private static void ge_add(ref ge_p1p1 r, ge_p3 p, ge_cached q)
    {
        Span<int> t0 = stackalloc int[10];

        ge_p1p1 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);
        r.T.CopyTo(result.T);

        fe_add(ref result.X, p.Y, p.X);
        fe_sub(ref result.Y, p.Y, p.X);
        fe_mul(ref result.Z, result.X, q.YplusX);
        fe_mul(ref result.Y, result.Y, q.YminusX);
        fe_mul(ref result.T, q.T2d, p.T);
        fe_mul(ref result.X, p.Z, q.Z);
        fe_add(ref t0, result.X, result.X);
        fe_sub(ref result.X, result.Z, result.Y);
        fe_add(ref result.Y, result.Z, result.Y);
        fe_add(ref result.Z, t0, result.T);
        fe_sub(ref result.T, t0, result.T);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
        result.T.CopyTo(r.T);
    }

    private static void ge_sub(ref ge_p1p1 r, ge_p3 p, ge_cached q)
    {
        Span<int> t0 = stackalloc int[10];

        ge_p1p1 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);
        r.T.CopyTo(result.T);

        fe_add(ref result.X, p.Y, p.X);
        fe_sub(ref result.Y, p.Y, p.X);
        fe_mul(ref result.Z, result.X, q.YminusX);
        fe_mul(ref result.Y, result.Y, q.YplusX);
        fe_mul(ref result.T, q.T2d, p.T);
        fe_mul(ref result.X, p.Z, q.Z);
        fe_add(ref t0, result.X, result.X);
        fe_sub(ref result.X, result.Z, result.Y);
        fe_add(ref result.Y, result.Z, result.Y);
        fe_sub(ref result.Z, t0, result.T);
        fe_add(ref result.T, t0, result.T);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
        result.T.CopyTo(r.T);
    }

    private static void ge_double_scalarmult_vartime(ref ge_p2 r, ReadOnlySpan<byte> a, ge_p3 A, ReadOnlySpan<byte> b)
    {
        Span<sbyte> aslide = stackalloc sbyte[256];
        Span<sbyte> bslide = stackalloc sbyte[256];

        ge_p2 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);

        /* A,3A,5A,7A,9A,11A,13A,15A */
        ge_cached Ai0 = new()
        {
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            YminusX = stackalloc int[10],
            Z = stackalloc int[10],
        };
        ge_cached Ai1 = new()
        {
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            YminusX = stackalloc int[10],
            Z = stackalloc int[10],
        };
        ge_cached Ai2 = new()
        {
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            YminusX = stackalloc int[10],
            Z = stackalloc int[10],
        };
        ge_cached Ai3 = new()
        {
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            YminusX = stackalloc int[10],
            Z = stackalloc int[10],
        };
        ge_cached Ai4 = new()
        {
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            YminusX = stackalloc int[10],
            Z = stackalloc int[10],
        };
        ge_cached Ai5 = new()
        {
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            YminusX = stackalloc int[10],
            Z = stackalloc int[10],
        };
        ge_cached Ai6 = new()
        {
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            YminusX = stackalloc int[10],
            Z = stackalloc int[10],
        };
        ge_cached Ai7 = new()
        {
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            YminusX = stackalloc int[10],
            Z = stackalloc int[10],
        };

        ge_p1p1 t = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        ge_p3 u = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        ge_p3 A2 = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        int i;
        slide(ref aslide, a);
        slide(ref bslide, b);
        ge_p3_to_cached(ref Ai0, A);
        ge_p3_dbl(ref t, A);
        ge_p1p1_to_p3(ref A2, t);
        ge_add(ref t, A2, Ai0);
        ge_p1p1_to_p3(ref u, t);
        ge_p3_to_cached(ref Ai1, u);
        ge_add(ref t, A2, Ai1);
        ge_p1p1_to_p3(ref u, t);
        ge_p3_to_cached(ref Ai2, u);
        ge_add(ref t, A2, Ai2);
        ge_p1p1_to_p3(ref u, t);
        ge_p3_to_cached(ref Ai3, u);
        ge_add(ref t, A2, Ai3);
        ge_p1p1_to_p3(ref u, t);
        ge_p3_to_cached(ref Ai4, u);
        ge_add(ref t, A2, Ai4);
        ge_p1p1_to_p3(ref u, t);
        ge_p3_to_cached(ref Ai5, u);
        ge_add(ref t, A2, Ai5);
        ge_p1p1_to_p3(ref u, t);
        ge_p3_to_cached(ref Ai6, u);
        ge_add(ref t, A2, Ai6);
        ge_p1p1_to_p3(ref u, t);
        ge_p3_to_cached(ref Ai7, u);
        ge_p2_0(ref result);

        ge_precomp bi = new()
        {
            yminusx = stackalloc int[10],
            yplusx = stackalloc int[10],
            xy2d = stackalloc int[10],
        };

        for (i = 255; i >= 0; --i)
        {
            if (aslide[i] != 0 || bslide[i] != 0)
            {
                break;
            }
        }

        for (; i >= 0; --i)
        {
            ge_p2_dbl(ref t, result);

            switch (aslide[i])
            {
                case > 0:
                {
                    ge_p1p1_to_p3(ref u, t);

                    switch (aslide[i] / 2)
                    {
                        case 0:
                            ge_add(ref t, u, Ai0);
                            break;
                        case 1:
                            ge_add(ref t, u, Ai1);
                            break;
                        case 2:
                            ge_add(ref t, u, Ai2);
                            break;
                        case 3:
                            ge_add(ref t, u, Ai3);
                            break;
                        case 4:
                            ge_add(ref t, u, Ai4);
                            break;
                        case 5:
                            ge_add(ref t, u, Ai5);
                            break;
                        case 6:
                            ge_add(ref t, u, Ai6);
                            break;
                        case 7:
                            ge_add(ref t, u, Ai7);
                            break;
                    }

                    break;
                }
                case < 0:
                {
                    ge_p1p1_to_p3(ref u, t);

                    switch (-aslide[i] / 2)
                    {
                        case 0:
                            ge_sub(ref t, u, Ai0);
                            break;
                        case 1:
                            ge_sub(ref t, u, Ai1);
                            break;
                        case 2:
                            ge_sub(ref t, u, Ai2);
                            break;
                        case 3:
                            ge_sub(ref t, u, Ai3);
                            break;
                        case 4:
                            ge_sub(ref t, u, Ai4);
                            break;
                        case 5:
                            ge_sub(ref t, u, Ai5);
                            break;
                        case 6:
                            ge_sub(ref t, u, Ai6);
                            break;
                        case 7:
                            ge_sub(ref t, u, Ai7);
                            break;
                    }

                    break;
                }
            }

            switch (bslide[i])
            {
                case > 0:
                {
                    GetPrecompBi(bslide[i] / 2, ref bi);
                    ge_p1p1_to_p3(ref u, t);
                    ge_madd(ref t, u, bi);
                    break;
                }
                case < 0:
                {
                    GetPrecompBi(-bslide[i] / 2, ref bi);
                    ge_p1p1_to_p3(ref u, t);
                    ge_msub(ref t, u, bi);
                    break;
                }
            }

            ge_p1p1_to_p2(ref result, t);
        }

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
    }

    private static void ge_madd(ref ge_p1p1 r, ge_p3 p, ge_precomp q)
    {
        Span<int> t0 = stackalloc int[10];
        ge_p1p1 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);
        r.T.CopyTo(result.T);

        fe_add(ref result.X, p.Y, p.X);
        fe_sub(ref result.Y, p.Y, p.X);
        fe_mul(ref result.Z, result.X, q.yplusx);
        fe_mul(ref result.Y, result.Y, q.yminusx);
        fe_mul(ref result.T, q.xy2d, p.T);
        fe_add(ref t0, p.Z, p.Z);
        fe_sub(ref result.X, result.Z, result.Y);
        fe_add(ref result.Y, result.Z, result.Y);
        fe_add(ref result.Z, t0, result.T);
        fe_sub(ref result.T, t0, result.T);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
        result.T.CopyTo(r.T);
    }

    private static void ge_msub(ref ge_p1p1 r, ge_p3 p, ge_precomp q)
    {
        Span<int> t0 = stackalloc int[10];
        ge_p1p1 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);
        r.T.CopyTo(result.T);

        fe_add(ref result.X, p.Y, p.X);
        fe_sub(ref result.Y, p.Y, p.X);
        fe_mul(ref result.Z, result.X, q.yminusx);
        fe_mul(ref result.Y, result.Y, q.yplusx);
        fe_mul(ref result.T, q.xy2d, p.T);
        fe_add(ref t0, p.Z, p.Z);
        fe_sub(ref result.X, result.Z, result.Y);
        fe_add(ref result.Y, result.Z, result.Y);
        fe_sub(ref result.Z, t0, result.T);
        fe_add(ref result.T, t0, result.T);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
        result.T.CopyTo(r.T);
    }

    private static void ge_scalarmult_base(ref ge_p3 h, ReadOnlySpan<byte> a)
    {
        sbyte carry;
        Span<sbyte> e = stackalloc sbyte[64];

        ge_precomp t = new()
        {
            yminusx = stackalloc int[10],
            yplusx = stackalloc int[10],
            xy2d = stackalloc int[10],
        };

        ge_p1p1 r = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        ge_p2 s = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
        };

        ge_p3 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        h.X.CopyTo(result.X);
        h.Y.CopyTo(result.Y);
        h.Z.CopyTo(result.Z);
        h.T.CopyTo(result.T);

        int i;

        for (i = 0; i < 32; ++i)
        {
            e[2 * i + 0] = (sbyte)((a[i] >> 0) & 15);
            e[2 * i + 1] = (sbyte)((a[i] >> 4) & 15);
        }

        /* each e[i] is between 0 and 15 */
        /* e[63] is between 0 and 7 */
        carry = 0;

        for (i = 0; i < 63; ++i)
        {
            e[i] += carry;
            carry = (sbyte)(e[i] + 8);
            carry >>= 4;
            e[i] -= (sbyte)(carry << 4);
        }

        e[63] += carry;
        /* each e[i] is between -8 and 8 */
        ge_p3_0(ref result);

        for (i = 1; i < 64; i += 2)
        {
            select(ref t, i / 2, e[i]);
            ge_madd(ref r, result, t);
            ge_p1p1_to_p3(ref result, r);
        }

        ge_p3_dbl(ref r, result);
        ge_p1p1_to_p2(ref s, r);
        ge_p2_dbl(ref r, s);
        ge_p1p1_to_p2(ref s, r);
        ge_p2_dbl(ref r, s);
        ge_p1p1_to_p2(ref s, r);
        ge_p2_dbl(ref r, s);
        ge_p1p1_to_p3(ref result, r);

        for (i = 0; i < 64; i += 2)
        {
            select(ref t, i / 2, e[i]);
            ge_madd(ref r, result, t);
            ge_p1p1_to_p3(ref result, r);
        }

        result.X.CopyTo(h.X);
        result.Y.CopyTo(h.Y);
        result.Z.CopyTo(h.Z);
        result.T.CopyTo(h.T);
    }

    private static void ge_p1p1_to_p2(ref ge_p2 r, ge_p1p1 p)
    {
        ge_p2 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);

        fe_mul(ref result.X, p.X, p.T);
        fe_mul(ref result.Y, p.Y, p.Z);
        fe_mul(ref result.Z, p.Z, p.T);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
    }

    private static void ge_p1p1_to_p3(ref ge_p3 r, ge_p1p1 p)
    {
        ge_p3 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);
        r.T.CopyTo(result.T);

        fe_mul(ref result.X, p.X, p.T);
        fe_mul(ref result.Y, p.Y, p.Z);
        fe_mul(ref result.Z, p.Z, p.T);
        fe_mul(ref result.T, p.X, p.Y);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
        result.T.CopyTo(r.T);
    }

    private static void ge_p2_0(ref ge_p2 h)
    {
        ge_p2 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
        };

        fe_0(ref result.X);
        fe_1(ref result.Y);
        fe_1(ref result.Z);

        result.X.CopyTo(h.X);
        result.Y.CopyTo(h.Y);
        result.Z.CopyTo(h.Z);
    }

    private static void ge_p2_dbl(ref ge_p1p1 r, ge_p2 p)
    {
        Span<int> t0 = stackalloc int[10];
        ge_p1p1 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);
        r.T.CopyTo(result.T);

        fe_sq(ref result.X, p.X);
        fe_sq(ref result.Z, p.Y);
        fe_sq2(ref result.T, p.Z);
        fe_add(ref result.Y, p.X, p.Y);
        fe_sq(ref t0, result.Y);
        fe_add(ref result.Y, result.Z, result.X);
        fe_sub(ref result.Z, result.Z, result.X);
        fe_sub(ref result.X, t0, result.Y);
        fe_sub(ref result.T, result.T, result.Z);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
        result.T.CopyTo(r.T);
    }

    private static void ge_p3_0(ref ge_p3 h)
    {
        ge_p3 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        fe_0(ref result.X);
        fe_1(ref result.Y);
        fe_1(ref result.Z);
        fe_0(ref result.T);

        result.X.CopyTo(h.X);
        result.Y.CopyTo(h.Y);
        result.Z.CopyTo(h.Z);
        result.T.CopyTo(h.T);
    }

    private static void ge_p3_dbl(ref ge_p1p1 r, ge_p3 p)
    {
        ge_p2 q = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
        };

        ge_p1p1 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        r.X.CopyTo(result.X);
        r.Y.CopyTo(result.Y);
        r.Z.CopyTo(result.Z);
        r.T.CopyTo(result.T);

        ge_p3_to_p2(ref q, p);
        ge_p2_dbl(ref result, q);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
        result.T.CopyTo(r.T);
    }

    private static void ge_p3_to_cached(ref ge_cached r, ge_p3 p)
    {
        ReadOnlySpan<int> d2 = stackalloc int[] { -21827239, -5839606, -30745221, 13898782, 229458, 15978800, -12551817, -6495438, 29715968, 9444199 };

        ge_cached result = new()
        {
            YminusX = stackalloc int[10],
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            Z = stackalloc int[10],
        };

        r.YminusX.CopyTo(result.YminusX);
        r.YplusX.CopyTo(result.YplusX);
        r.T2d.CopyTo(result.T2d);
        r.Z.CopyTo(result.Z);

        fe_add(ref result.YplusX, p.Y, p.X);
        fe_sub(ref result.YminusX, p.Y, p.X);
        fe_copy(ref result.Z, p.Z);
        fe_mul(ref result.T2d, p.T, d2);

        result.YminusX.CopyTo(r.YminusX);
        result.YplusX.CopyTo(r.YplusX);
        result.T2d.CopyTo(r.T2d);
        result.Z.CopyTo(r.Z);
    }

    private static void ge_p3_to_p2(ref ge_p2 r, ge_p3 p)
    {
        ge_p2 result = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
        };

        fe_copy(ref result.X, p.X);
        fe_copy(ref result.Y, p.Y);
        fe_copy(ref result.Z, p.Z);

        result.X.CopyTo(r.X);
        result.Y.CopyTo(r.Y);
        result.Z.CopyTo(r.Z);
    }
}