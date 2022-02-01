public static partial class GlitchEd25519
{
    private static void fe_0(ref Span<int> h)
    {
        h[0] = 0;
        h[1] = 0;
        h[2] = 0;
        h[3] = 0;
        h[4] = 0;
        h[5] = 0;
        h[6] = 0;
        h[7] = 0;
        h[8] = 0;
        h[9] = 0;
    }

    private static void fe_1(ref Span<int> h)
    {
        h[0] = 1;
        h[1] = 0;
        h[2] = 0;
        h[3] = 0;
        h[4] = 0;
        h[5] = 0;
        h[6] = 0;
        h[7] = 0;
        h[8] = 0;
        h[9] = 0;
    }

    private static void fe_frombytes(ref Span<int> h, ReadOnlySpan<byte> s)
    {
        long h0 = (long)load_4(s);
        long h1 = (long)load_3(s[4..]) << 6;
        long h2 = (long)load_3(s[7..]) << 5;
        long h3 = (long)load_3(s[10..]) << 3;
        long h4 = (long)load_3(s[13..]) << 2;
        long h5 = (long)load_4(s[16..]);
        long h6 = (long)load_3(s[20..]) << 7;
        long h7 = (long)load_3(s[23..]) << 5;
        long h8 = (long)load_3(s[26..]) << 4;
        long h9 = (long)(load_3(s[29..]) & 8388607) << 2;
        long carry0;
        long carry1;
        long carry2;
        long carry3;
        long carry4;
        long carry5;
        long carry6;
        long carry7;
        long carry8;
        long carry9;

        carry9 = (h9 + (long)(1 << 24)) >> 25;
        h0 += carry9 * 19;
        h9 -= carry9 << 25;
        carry1 = (h1 + (long)(1 << 24)) >> 25;
        h2 += carry1;
        h1 -= carry1 << 25;
        carry3 = (h3 + (long)(1 << 24)) >> 25;
        h4 += carry3;
        h3 -= carry3 << 25;
        carry5 = (h5 + (long)(1 << 24)) >> 25;
        h6 += carry5;
        h5 -= carry5 << 25;
        carry7 = (h7 + (long)(1 << 24)) >> 25;
        h8 += carry7;
        h7 -= carry7 << 25;
        carry0 = (h0 + (long)(1 << 25)) >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;
        carry2 = (h2 + (long)(1 << 25)) >> 26;
        h3 += carry2;
        h2 -= carry2 << 26;
        carry4 = (h4 + (long)(1 << 25)) >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;
        carry6 = (h6 + (long)(1 << 25)) >> 26;
        h7 += carry6;
        h6 -= carry6 << 26;
        carry8 = (h8 + (long)(1 << 25)) >> 26;
        h9 += carry8;
        h8 -= carry8 << 26;

        h[0] = (int)h0;
        h[1] = (int)h1;
        h[2] = (int)h2;
        h[3] = (int)h3;
        h[4] = (int)h4;
        h[5] = (int)h5;
        h[6] = (int)h6;
        h[7] = (int)h7;
        h[8] = (int)h8;
        h[9] = (int)h9;
    }

    private static void fe_tobytes(ref Span<byte> s, ReadOnlySpan<int> h)
    {
        int h0 = h[0];
        int h1 = h[1];
        int h2 = h[2];
        int h3 = h[3];
        int h4 = h[4];
        int h5 = h[5];
        int h6 = h[6];
        int h7 = h[7];
        int h8 = h[8];
        int h9 = h[9];
        int q;
        int carry0;
        int carry1;
        int carry2;
        int carry3;
        int carry4;
        int carry5;
        int carry6;
        int carry7;
        int carry8;
        int carry9;
        q = (19 * h9 + (((int)1) << 24)) >> 25;
        q = (h0 + q) >> 26;
        q = (h1 + q) >> 25;
        q = (h2 + q) >> 26;
        q = (h3 + q) >> 25;
        q = (h4 + q) >> 26;
        q = (h5 + q) >> 25;
        q = (h6 + q) >> 26;
        q = (h7 + q) >> 25;
        q = (h8 + q) >> 26;
        q = (h9 + q) >> 25;
        /* Goal: Output h-(2^255-19)q, which is between 0 and 2^255-20. */
        h0 += 19 * q;
        /* Goal: Output h-2^255 q, which is between 0 and 2^255-20. */
        carry0 = h0 >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;
        carry1 = h1 >> 25;
        h2 += carry1;
        h1 -= carry1 << 25;
        carry2 = h2 >> 26;
        h3 += carry2;
        h2 -= carry2 << 26;
        carry3 = h3 >> 25;
        h4 += carry3;
        h3 -= carry3 << 25;
        carry4 = h4 >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;
        carry5 = h5 >> 25;
        h6 += carry5;
        h5 -= carry5 << 25;
        carry6 = h6 >> 26;
        h7 += carry6;
        h6 -= carry6 << 26;
        carry7 = h7 >> 25;
        h8 += carry7;
        h7 -= carry7 << 25;
        carry8 = h8 >> 26;
        h9 += carry8;
        h8 -= carry8 << 26;
        carry9 = h9 >> 25;
        h9 -= carry9 << 25;

        /* h10 = carry9 */
        /*
        Goal: Output h0+...+2^255 h10-2^255 q, which is between 0 and 2^255-20.
        Have h0+...+2^230 h9 between 0 and 2^255-1;
        evidently 2^255 h10-2^255 q = 0.
        Goal: Output h0+...+2^230 h9.
        */
        s[0] = (byte)(h0 >> 0);
        s[1] = (byte)(h0 >> 8);
        s[2] = (byte)(h0 >> 16);
        s[3] = (byte)((h0 >> 24) | (h1 << 2));
        s[4] = (byte)(h1 >> 6);
        s[5] = (byte)(h1 >> 14);
        s[6] = (byte)((h1 >> 22) | (h2 << 3));
        s[7] = (byte)(h2 >> 5);
        s[8] = (byte)(h2 >> 13);
        s[9] = (byte)((h2 >> 21) | (h3 << 5));
        s[10] = (byte)(h3 >> 3);
        s[11] = (byte)(h3 >> 11);
        s[12] = (byte)((h3 >> 19) | (h4 << 6));
        s[13] = (byte)(h4 >> 2);
        s[14] = (byte)(h4 >> 10);
        s[15] = (byte)(h4 >> 18);
        s[16] = (byte)(h5 >> 0);
        s[17] = (byte)(h5 >> 8);
        s[18] = (byte)(h5 >> 16);
        s[19] = (byte)((h5 >> 24) | (h6 << 1));
        s[20] = (byte)(h6 >> 7);
        s[21] = (byte)(h6 >> 15);
        s[22] = (byte)((h6 >> 23) | (h7 << 3));
        s[23] = (byte)(h7 >> 5);
        s[24] = (byte)(h7 >> 13);
        s[25] = (byte)((h7 >> 21) | (h8 << 4));
        s[26] = (byte)(h8 >> 4);
        s[27] = (byte)(h8 >> 12);
        s[28] = (byte)((h8 >> 20) | (h9 << 6));
        s[29] = (byte)(h9 >> 2);
        s[30] = (byte)(h9 >> 10);
        s[31] = (byte)(h9 >> 18);
    }

    private static void fe_copy(ref Span<int> h, ReadOnlySpan<int> f)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];

        h[0] = f0;
        h[1] = f1;
        h[2] = f2;
        h[3] = f3;
        h[4] = f4;
        h[5] = f5;
        h[6] = f6;
        h[7] = f7;
        h[8] = f8;
        h[9] = f9;
    }

    private static int fe_isnegative(ReadOnlySpan<int> f)
    {
        Span<byte> s = stackalloc byte[32];

        fe_tobytes(ref s, f);

        return s[0] & 1;
    }

    private static bool fe_isnonzero(ReadOnlySpan<int> f)
    {
        Span<byte> s = stackalloc byte[32];
        byte r;

        fe_tobytes(ref s, f);

        r = s[0];
        r |= s[1];
        r |= s[2];
        r |= s[3];
        r |= s[4];
        r |= s[5];
        r |= s[6];
        r |= s[7];
        r |= s[8];
        r |= s[9];
        r |= s[10];
        r |= s[11];
        r |= s[12];
        r |= s[13];
        r |= s[14];
        r |= s[15];
        r |= s[16];
        r |= s[17];
        r |= s[18];
        r |= s[19];
        r |= s[20];
        r |= s[21];
        r |= s[22];
        r |= s[23];
        r |= s[24];
        r |= s[25];
        r |= s[26];
        r |= s[27];
        r |= s[28];
        r |= s[29];
        r |= s[30];
        r |= s[31];

        return r != 0;
    }

    private static void fe_cmov(ref Span<int> f, ReadOnlySpan<int> g, uint b)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        int g0 = g[0];
        int g1 = g[1];
        int g2 = g[2];
        int g3 = g[3];
        int g4 = g[4];
        int g5 = g[5];
        int g6 = g[6];
        int g7 = g[7];
        int g8 = g[8];
        int g9 = g[9];
        int x0 = f0 ^ g0;
        int x1 = f1 ^ g1;
        int x2 = f2 ^ g2;
        int x3 = f3 ^ g3;
        int x4 = f4 ^ g4;
        int x5 = f5 ^ g5;
        int x6 = f6 ^ g6;
        int x7 = f7 ^ g7;
        int x8 = f8 ^ g8;
        int x9 = f9 ^ g9;

        b = (uint)(-(int)b); /* silence warning */
        x0 = (int)(x0 & b);
        x1 = (int)(x1 & b);
        x2 = (int)(x2 & b);
        x3 = (int)(x3 & b);
        x4 = (int)(x4 & b);
        x5 = (int)(x5 & b);
        x6 = (int)(x6 & b);
        x7 = (int)(x7 & b);
        x8 = (int)(x8 & b);
        x9 = (int)(x9 & b);

        f[0] = f0 ^ x0;
        f[1] = f1 ^ x1;
        f[2] = f2 ^ x2;
        f[3] = f3 ^ x3;
        f[4] = f4 ^ x4;
        f[5] = f5 ^ x5;
        f[6] = f6 ^ x6;
        f[7] = f7 ^ x7;
        f[8] = f8 ^ x8;
        f[9] = f9 ^ x9;
    }

    private static void fe_cswap(ref Span<int> f, ref Span<int> g, uint b)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        int g0 = g[0];
        int g1 = g[1];
        int g2 = g[2];
        int g3 = g[3];
        int g4 = g[4];
        int g5 = g[5];
        int g6 = g[6];
        int g7 = g[7];
        int g8 = g[8];
        int g9 = g[9];
        int x0 = f0 ^ g0;
        int x1 = f1 ^ g1;
        int x2 = f2 ^ g2;
        int x3 = f3 ^ g3;
        int x4 = f4 ^ g4;
        int x5 = f5 ^ g5;
        int x6 = f6 ^ g6;
        int x7 = f7 ^ g7;
        int x8 = f8 ^ g8;
        int x9 = f9 ^ g9;
        b = (uint)(-(int)b); /* silence warning */
        x0 = (int)(x0 & b);
        x1 = (int)(x1 & b);
        x2 = (int)(x2 & b);
        x3 = (int)(x3 & b);
        x4 = (int)(x4 & b);
        x5 = (int)(x5 & b);
        x6 = (int)(x6 & b);
        x7 = (int)(x7 & b);
        x8 = (int)(x8 & b);
        x9 = (int)(x9 & b);
        f[0] = f0 ^ x0;
        f[1] = f1 ^ x1;
        f[2] = f2 ^ x2;
        f[3] = f3 ^ x3;
        f[4] = f4 ^ x4;
        f[5] = f5 ^ x5;
        f[6] = f6 ^ x6;
        f[7] = f7 ^ x7;
        f[8] = f8 ^ x8;
        f[9] = f9 ^ x9;
        g[0] = g0 ^ x0;
        g[1] = g1 ^ x1;
        g[2] = g2 ^ x2;
        g[3] = g3 ^ x3;
        g[4] = g4 ^ x4;
        g[5] = g5 ^ x5;
        g[6] = g6 ^ x6;
        g[7] = g7 ^ x7;
        g[8] = g8 ^ x8;
        g[9] = g9 ^ x9;
    }

    private static void fe_neg(ref Span<int> h, ReadOnlySpan<int> f)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        int h0 = -f0;
        int h1 = -f1;
        int h2 = -f2;
        int h3 = -f3;
        int h4 = -f4;
        int h5 = -f5;
        int h6 = -f6;
        int h7 = -f7;
        int h8 = -f8;
        int h9 = -f9;

        h[0] = h0;
        h[1] = h1;
        h[2] = h2;
        h[3] = h3;
        h[4] = h4;
        h[5] = h5;
        h[6] = h6;
        h[7] = h7;
        h[8] = h8;
        h[9] = h9;
    }

    private static void fe_add(ref Span<int> h, ReadOnlySpan<int> f, ReadOnlySpan<int> g)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        int g0 = g[0];
        int g1 = g[1];
        int g2 = g[2];
        int g3 = g[3];
        int g4 = g[4];
        int g5 = g[5];
        int g6 = g[6];
        int g7 = g[7];
        int g8 = g[8];
        int g9 = g[9];
        int h0 = f0 + g0;
        int h1 = f1 + g1;
        int h2 = f2 + g2;
        int h3 = f3 + g3;
        int h4 = f4 + g4;
        int h5 = f5 + g5;
        int h6 = f6 + g6;
        int h7 = f7 + g7;
        int h8 = f8 + g8;
        int h9 = f9 + g9;

        h[0] = h0;
        h[1] = h1;
        h[2] = h2;
        h[3] = h3;
        h[4] = h4;
        h[5] = h5;
        h[6] = h6;
        h[7] = h7;
        h[8] = h8;
        h[9] = h9;
    }

    private static void fe_invert(ref Span<int> @out, ReadOnlySpan<int> z)
    {
        int i;
        Span<int> t0 = stackalloc int[10];
        Span<int> t1 = stackalloc int[10];
        Span<int> t2 = stackalloc int[10];
        Span<int> t3 = stackalloc int[10];
        Span<int> result = stackalloc int[10];

        fe_sq(ref t0, z);

        for (i = 1; i < 1; ++i)
        {
            fe_sq(ref t0, t0);
        }

        fe_sq(ref t1, t0);

        for (i = 1; i < 2; ++i)
        {
            fe_sq(ref t1, t1);
        }

        fe_mul(ref t1, z, t1);
        fe_mul(ref t0, t0, t1);
        fe_sq(ref t2, t0);

        for (i = 1; i < 1; ++i)
        {
            fe_sq(ref t2, t2);
        }

        fe_mul(ref t1, t1, t2);
        fe_sq(ref t2, t1);

        for (i = 1; i < 5; ++i)
        {
            fe_sq(ref t2, t2);
        }

        fe_mul(ref t1, t2, t1);
        fe_sq(ref t2, t1);

        for (i = 1; i < 10; ++i)
        {
            fe_sq(ref t2, t2);
        }

        fe_mul(ref t2, t2, t1);
        fe_sq(ref t3, t2);

        for (i = 1; i < 20; ++i)
        {
            fe_sq(ref t3, t3);
        }

        fe_mul(ref t2, t3, t2);
        fe_sq(ref t2, t2);

        for (i = 1; i < 10; ++i)
        {
            fe_sq(ref t2, t2);
        }

        fe_mul(ref t1, t2, t1);
        fe_sq(ref t2, t1);

        for (i = 1; i < 50; ++i)
        {
            fe_sq(ref t2, t2);
        }

        fe_mul(ref t2, t2, t1);
        fe_sq(ref t3, t2);

        for (i = 1; i < 100; ++i)
        {
            fe_sq(ref t3, t3);
        }

        fe_mul(ref t2, t3, t2);
        fe_sq(ref t2, t2);

        for (i = 1; i < 50; ++i)
        {
            fe_sq(ref t2, t2);
        }

        fe_mul(ref t1, t2, t1);
        fe_sq(ref t1, t1);

        for (i = 1; i < 5; ++i)
        {
            fe_sq(ref t1, t1);
        }

        fe_mul(ref result, t1, t0);
        result.CopyTo(@out);
    }
 
    private static void fe_sq(ref Span<int> h, ReadOnlySpan<int> f)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        int f0_2 = 2 * f0;
        int f1_2 = 2 * f1;
        int f2_2 = 2 * f2;
        int f3_2 = 2 * f3;
        int f4_2 = 2 * f4;
        int f5_2 = 2 * f5;
        int f6_2 = 2 * f6;
        int f7_2 = 2 * f7;
        int f5_38 = 38 * f5; /* 1.959375*2^30 */
        int f6_19 = 19 * f6; /* 1.959375*2^30 */
        int f7_38 = 38 * f7; /* 1.959375*2^30 */
        int f8_19 = 19 * f8; /* 1.959375*2^30 */
        int f9_38 = 38 * f9; /* 1.959375*2^30 */
        long f0f0 = f0 * (long)f0;
        long f0f1_2 = f0_2 * (long)f1;
        long f0f2_2 = f0_2 * (long)f2;
        long f0f3_2 = f0_2 * (long)f3;
        long f0f4_2 = f0_2 * (long)f4;
        long f0f5_2 = f0_2 * (long)f5;
        long f0f6_2 = f0_2 * (long)f6;
        long f0f7_2 = f0_2 * (long)f7;
        long f0f8_2 = f0_2 * (long)f8;
        long f0f9_2 = f0_2 * (long)f9;
        long f1f1_2 = f1_2 * (long)f1;
        long f1f2_2 = f1_2 * (long)f2;
        long f1f3_4 = f1_2 * (long)f3_2;
        long f1f4_2 = f1_2 * (long)f4;
        long f1f5_4 = f1_2 * (long)f5_2;
        long f1f6_2 = f1_2 * (long)f6;
        long f1f7_4 = f1_2 * (long)f7_2;
        long f1f8_2 = f1_2 * (long)f8;
        long f1f9_76 = f1_2 * (long)f9_38;
        long f2f2 = f2 * (long)f2;
        long f2f3_2 = f2_2 * (long)f3;
        long f2f4_2 = f2_2 * (long)f4;
        long f2f5_2 = f2_2 * (long)f5;
        long f2f6_2 = f2_2 * (long)f6;
        long f2f7_2 = f2_2 * (long)f7;
        long f2f8_38 = f2_2 * (long)f8_19;
        long f2f9_38 = f2 * (long)f9_38;
        long f3f3_2 = f3_2 * (long)f3;
        long f3f4_2 = f3_2 * (long)f4;
        long f3f5_4 = f3_2 * (long)f5_2;
        long f3f6_2 = f3_2 * (long)f6;
        long f3f7_76 = f3_2 * (long)f7_38;
        long f3f8_38 = f3_2 * (long)f8_19;
        long f3f9_76 = f3_2 * (long)f9_38;
        long f4f4 = f4 * (long)f4;
        long f4f5_2 = f4_2 * (long)f5;
        long f4f6_38 = f4_2 * (long)f6_19;
        long f4f7_38 = f4 * (long)f7_38;
        long f4f8_38 = f4_2 * (long)f8_19;
        long f4f9_38 = f4 * (long)f9_38;
        long f5f5_38 = f5 * (long)f5_38;
        long f5f6_38 = f5_2 * (long)f6_19;
        long f5f7_76 = f5_2 * (long)f7_38;
        long f5f8_38 = f5_2 * (long)f8_19;
        long f5f9_76 = f5_2 * (long)f9_38;
        long f6f6_19 = f6 * (long)f6_19;
        long f6f7_38 = f6 * (long)f7_38;
        long f6f8_38 = f6_2 * (long)f8_19;
        long f6f9_38 = f6 * (long)f9_38;
        long f7f7_38 = f7 * (long)f7_38;
        long f7f8_38 = f7_2 * (long)f8_19;
        long f7f9_76 = f7_2 * (long)f9_38;
        long f8f8_19 = f8 * (long)f8_19;
        long f8f9_38 = f8 * (long)f9_38;
        long f9f9_38 = f9 * (long)f9_38;
        long h0 = f0f0 + f1f9_76 + f2f8_38 + f3f7_76 + f4f6_38 + f5f5_38;
        long h1 = f0f1_2 + f2f9_38 + f3f8_38 + f4f7_38 + f5f6_38;
        long h2 = f0f2_2 + f1f1_2 + f3f9_76 + f4f8_38 + f5f7_76 + f6f6_19;
        long h3 = f0f3_2 + f1f2_2 + f4f9_38 + f5f8_38 + f6f7_38;
        long h4 = f0f4_2 + f1f3_4 + f2f2 + f5f9_76 + f6f8_38 + f7f7_38;
        long h5 = f0f5_2 + f1f4_2 + f2f3_2 + f6f9_38 + f7f8_38;
        long h6 = f0f6_2 + f1f5_4 + f2f4_2 + f3f3_2 + f7f9_76 + f8f8_19;
        long h7 = f0f7_2 + f1f6_2 + f2f5_2 + f3f4_2 + f8f9_38;
        long h8 = f0f8_2 + f1f7_4 + f2f6_2 + f3f5_4 + f4f4 + f9f9_38;
        long h9 = f0f9_2 + f1f8_2 + f2f7_2 + f3f6_2 + f4f5_2;
        long carry0;
        long carry1;
        long carry2;
        long carry3;
        long carry4;
        long carry5;
        long carry6;
        long carry7;
        long carry8;
        long carry9;
        carry0 = (h0 + (long)(1 << 25)) >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;
        carry4 = (h4 + (long)(1 << 25)) >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;
        carry1 = (h1 + (long)(1 << 24)) >> 25;
        h2 += carry1;
        h1 -= carry1 << 25;
        carry5 = (h5 + (long)(1 << 24)) >> 25;
        h6 += carry5;
        h5 -= carry5 << 25;
        carry2 = (h2 + (long)(1 << 25)) >> 26;
        h3 += carry2;
        h2 -= carry2 << 26;
        carry6 = (h6 + (long)(1 << 25)) >> 26;
        h7 += carry6;
        h6 -= carry6 << 26;
        carry3 = (h3 + (long)(1 << 24)) >> 25;
        h4 += carry3;
        h3 -= carry3 << 25;
        carry7 = (h7 + (long)(1 << 24)) >> 25;
        h8 += carry7;
        h7 -= carry7 << 25;
        carry4 = (h4 + (long)(1 << 25)) >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;
        carry8 = (h8 + (long)(1 << 25)) >> 26;
        h9 += carry8;
        h8 -= carry8 << 26;
        carry9 = (h9 + (long)(1 << 24)) >> 25;
        h0 += carry9 * 19;
        h9 -= carry9 << 25;
        carry0 = (h0 + (long)(1 << 25)) >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;
        h[0] = (int)h0;
        h[1] = (int)h1;
        h[2] = (int)h2;
        h[3] = (int)h3;
        h[4] = (int)h4;
        h[5] = (int)h5;
        h[6] = (int)h6;
        h[7] = (int)h7;
        h[8] = (int)h8;
        h[9] = (int)h9;
    }

    private static void fe_sq2(ref Span<int> h, ReadOnlySpan<int> f)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        int f0_2 = 2 * f0;
        int f1_2 = 2 * f1;
        int f2_2 = 2 * f2;
        int f3_2 = 2 * f3;
        int f4_2 = 2 * f4;
        int f5_2 = 2 * f5;
        int f6_2 = 2 * f6;
        int f7_2 = 2 * f7;
        int f5_38 = 38 * f5; /* 1.959375*2^30 */
        int f6_19 = 19 * f6; /* 1.959375*2^30 */
        int f7_38 = 38 * f7; /* 1.959375*2^30 */
        int f8_19 = 19 * f8; /* 1.959375*2^30 */
        int f9_38 = 38 * f9; /* 1.959375*2^30 */
        long f0f0 = f0 * (long)f0;
        long f0f1_2 = f0_2 * (long)f1;
        long f0f2_2 = f0_2 * (long)f2;
        long f0f3_2 = f0_2 * (long)f3;
        long f0f4_2 = f0_2 * (long)f4;
        long f0f5_2 = f0_2 * (long)f5;
        long f0f6_2 = f0_2 * (long)f6;
        long f0f7_2 = f0_2 * (long)f7;
        long f0f8_2 = f0_2 * (long)f8;
        long f0f9_2 = f0_2 * (long)f9;
        long f1f1_2 = f1_2 * (long)f1;
        long f1f2_2 = f1_2 * (long)f2;
        long f1f3_4 = f1_2 * (long)f3_2;
        long f1f4_2 = f1_2 * (long)f4;
        long f1f5_4 = f1_2 * (long)f5_2;
        long f1f6_2 = f1_2 * (long)f6;
        long f1f7_4 = f1_2 * (long)f7_2;
        long f1f8_2 = f1_2 * (long)f8;
        long f1f9_76 = f1_2 * (long)f9_38;
        long f2f2 = f2 * (long)f2;
        long f2f3_2 = f2_2 * (long)f3;
        long f2f4_2 = f2_2 * (long)f4;
        long f2f5_2 = f2_2 * (long)f5;
        long f2f6_2 = f2_2 * (long)f6;
        long f2f7_2 = f2_2 * (long)f7;
        long f2f8_38 = f2_2 * (long)f8_19;
        long f2f9_38 = f2 * (long)f9_38;
        long f3f3_2 = f3_2 * (long)f3;
        long f3f4_2 = f3_2 * (long)f4;
        long f3f5_4 = f3_2 * (long)f5_2;
        long f3f6_2 = f3_2 * (long)f6;
        long f3f7_76 = f3_2 * (long)f7_38;
        long f3f8_38 = f3_2 * (long)f8_19;
        long f3f9_76 = f3_2 * (long)f9_38;
        long f4f4 = f4 * (long)f4;
        long f4f5_2 = f4_2 * (long)f5;
        long f4f6_38 = f4_2 * (long)f6_19;
        long f4f7_38 = f4 * (long)f7_38;
        long f4f8_38 = f4_2 * (long)f8_19;
        long f4f9_38 = f4 * (long)f9_38;
        long f5f5_38 = f5 * (long)f5_38;
        long f5f6_38 = f5_2 * (long)f6_19;
        long f5f7_76 = f5_2 * (long)f7_38;
        long f5f8_38 = f5_2 * (long)f8_19;
        long f5f9_76 = f5_2 * (long)f9_38;
        long f6f6_19 = f6 * (long)f6_19;
        long f6f7_38 = f6 * (long)f7_38;
        long f6f8_38 = f6_2 * (long)f8_19;
        long f6f9_38 = f6 * (long)f9_38;
        long f7f7_38 = f7 * (long)f7_38;
        long f7f8_38 = f7_2 * (long)f8_19;
        long f7f9_76 = f7_2 * (long)f9_38;
        long f8f8_19 = f8 * (long)f8_19;
        long f8f9_38 = f8 * (long)f9_38;
        long f9f9_38 = f9 * (long)f9_38;
        long h0 = f0f0 + f1f9_76 + f2f8_38 + f3f7_76 + f4f6_38 + f5f5_38;
        long h1 = f0f1_2 + f2f9_38 + f3f8_38 + f4f7_38 + f5f6_38;
        long h2 = f0f2_2 + f1f1_2 + f3f9_76 + f4f8_38 + f5f7_76 + f6f6_19;
        long h3 = f0f3_2 + f1f2_2 + f4f9_38 + f5f8_38 + f6f7_38;
        long h4 = f0f4_2 + f1f3_4 + f2f2 + f5f9_76 + f6f8_38 + f7f7_38;
        long h5 = f0f5_2 + f1f4_2 + f2f3_2 + f6f9_38 + f7f8_38;
        long h6 = f0f6_2 + f1f5_4 + f2f4_2 + f3f3_2 + f7f9_76 + f8f8_19;
        long h7 = f0f7_2 + f1f6_2 + f2f5_2 + f3f4_2 + f8f9_38;
        long h8 = f0f8_2 + f1f7_4 + f2f6_2 + f3f5_4 + f4f4 + f9f9_38;
        long h9 = f0f9_2 + f1f8_2 + f2f7_2 + f3f6_2 + f4f5_2;
        long carry0;
        long carry1;
        long carry2;
        long carry3;
        long carry4;
        long carry5;
        long carry6;
        long carry7;
        long carry8;
        long carry9;
        h0 += h0;
        h1 += h1;
        h2 += h2;
        h3 += h3;
        h4 += h4;
        h5 += h5;
        h6 += h6;
        h7 += h7;
        h8 += h8;
        h9 += h9;
        carry0 = (h0 + (long)(1 << 25)) >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;
        carry4 = (h4 + (long)(1 << 25)) >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;
        carry1 = (h1 + (long)(1 << 24)) >> 25;
        h2 += carry1;
        h1 -= carry1 << 25;
        carry5 = (h5 + (long)(1 << 24)) >> 25;
        h6 += carry5;
        h5 -= carry5 << 25;
        carry2 = (h2 + (long)(1 << 25)) >> 26;
        h3 += carry2;
        h2 -= carry2 << 26;
        carry6 = (h6 + (long)(1 << 25)) >> 26;
        h7 += carry6;
        h6 -= carry6 << 26;
        carry3 = (h3 + (long)(1 << 24)) >> 25;
        h4 += carry3;
        h3 -= carry3 << 25;
        carry7 = (h7 + (long)(1 << 24)) >> 25;
        h8 += carry7;
        h7 -= carry7 << 25;
        carry4 = (h4 + (long)(1 << 25)) >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;
        carry8 = (h8 + (long)(1 << 25)) >> 26;
        h9 += carry8;
        h8 -= carry8 << 26;
        carry9 = (h9 + (long)(1 << 24)) >> 25;
        h0 += carry9 * 19;
        h9 -= carry9 << 25;
        carry0 = (h0 + (long)(1 << 25)) >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;
        h[0] = (int)h0;
        h[1] = (int)h1;
        h[2] = (int)h2;
        h[3] = (int)h3;
        h[4] = (int)h4;
        h[5] = (int)h5;
        h[6] = (int)h6;
        h[7] = (int)h7;
        h[8] = (int)h8;
        h[9] = (int)h9;
    }

    private static void fe_mul(ref Span<int> h, ReadOnlySpan<int> f, ReadOnlySpan<int> g)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        int g0 = g[0];
        int g1 = g[1];
        int g2 = g[2];
        int g3 = g[3];
        int g4 = g[4];
        int g5 = g[5];
        int g6 = g[6];
        int g7 = g[7];
        int g8 = g[8];
        int g9 = g[9];
        int g1_19 = 19 * g1; /* 1.959375*2^29 */
        int g2_19 = 19 * g2; /* 1.959375*2^30; still ok */
        int g3_19 = 19 * g3;
        int g4_19 = 19 * g4;
        int g5_19 = 19 * g5;
        int g6_19 = 19 * g6;
        int g7_19 = 19 * g7;
        int g8_19 = 19 * g8;
        int g9_19 = 19 * g9;
        int f1_2 = 2 * f1;
        int f3_2 = 2 * f3;
        int f5_2 = 2 * f5;
        int f7_2 = 2 * f7;
        int f9_2 = 2 * f9;
        long f0g0 = f0 * (long)g0;
        long f0g1 = f0 * (long)g1;
        long f0g2 = f0 * (long)g2;
        long f0g3 = f0 * (long)g3;
        long f0g4 = f0 * (long)g4;
        long f0g5 = f0 * (long)g5;
        long f0g6 = f0 * (long)g6;
        long f0g7 = f0 * (long)g7;
        long f0g8 = f0 * (long)g8;
        long f0g9 = f0 * (long)g9;
        long f1g0 = f1 * (long)g0;
        long f1g1_2 = f1_2 * (long)g1;
        long f1g2 = f1 * (long)g2;
        long f1g3_2 = f1_2 * (long)g3;
        long f1g4 = f1 * (long)g4;
        long f1g5_2 = f1_2 * (long)g5;
        long f1g6 = f1 * (long)g6;
        long f1g7_2 = f1_2 * (long)g7;
        long f1g8 = f1 * (long)g8;
        long f1g9_38 = f1_2 * (long)g9_19;
        long f2g0 = f2 * (long)g0;
        long f2g1 = f2 * (long)g1;
        long f2g2 = f2 * (long)g2;
        long f2g3 = f2 * (long)g3;
        long f2g4 = f2 * (long)g4;
        long f2g5 = f2 * (long)g5;
        long f2g6 = f2 * (long)g6;
        long f2g7 = f2 * (long)g7;
        long f2g8_19 = f2 * (long)g8_19;
        long f2g9_19 = f2 * (long)g9_19;
        long f3g0 = f3 * (long)g0;
        long f3g1_2 = f3_2 * (long)g1;
        long f3g2 = f3 * (long)g2;
        long f3g3_2 = f3_2 * (long)g3;
        long f3g4 = f3 * (long)g4;
        long f3g5_2 = f3_2 * (long)g5;
        long f3g6 = f3 * (long)g6;
        long f3g7_38 = f3_2 * (long)g7_19;
        long f3g8_19 = f3 * (long)g8_19;
        long f3g9_38 = f3_2 * (long)g9_19;
        long f4g0 = f4 * (long)g0;
        long f4g1 = f4 * (long)g1;
        long f4g2 = f4 * (long)g2;
        long f4g3 = f4 * (long)g3;
        long f4g4 = f4 * (long)g4;
        long f4g5 = f4 * (long)g5;
        long f4g6_19 = f4 * (long)g6_19;
        long f4g7_19 = f4 * (long)g7_19;
        long f4g8_19 = f4 * (long)g8_19;
        long f4g9_19 = f4 * (long)g9_19;
        long f5g0 = f5 * (long)g0;
        long f5g1_2 = f5_2 * (long)g1;
        long f5g2 = f5 * (long)g2;
        long f5g3_2 = f5_2 * (long)g3;
        long f5g4 = f5 * (long)g4;
        long f5g5_38 = f5_2 * (long)g5_19;
        long f5g6_19 = f5 * (long)g6_19;
        long f5g7_38 = f5_2 * (long)g7_19;
        long f5g8_19 = f5 * (long)g8_19;
        long f5g9_38 = f5_2 * (long)g9_19;
        long f6g0 = f6 * (long)g0;
        long f6g1 = f6 * (long)g1;
        long f6g2 = f6 * (long)g2;
        long f6g3 = f6 * (long)g3;
        long f6g4_19 = f6 * (long)g4_19;
        long f6g5_19 = f6 * (long)g5_19;
        long f6g6_19 = f6 * (long)g6_19;
        long f6g7_19 = f6 * (long)g7_19;
        long f6g8_19 = f6 * (long)g8_19;
        long f6g9_19 = f6 * (long)g9_19;
        long f7g0 = f7 * (long)g0;
        long f7g1_2 = f7_2 * (long)g1;
        long f7g2 = f7 * (long)g2;
        long f7g3_38 = f7_2 * (long)g3_19;
        long f7g4_19 = f7 * (long)g4_19;
        long f7g5_38 = f7_2 * (long)g5_19;
        long f7g6_19 = f7 * (long)g6_19;
        long f7g7_38 = f7_2 * (long)g7_19;
        long f7g8_19 = f7 * (long)g8_19;
        long f7g9_38 = f7_2 * (long)g9_19;
        long f8g0 = f8 * (long)g0;
        long f8g1 = f8 * (long)g1;
        long f8g2_19 = f8 * (long)g2_19;
        long f8g3_19 = f8 * (long)g3_19;
        long f8g4_19 = f8 * (long)g4_19;
        long f8g5_19 = f8 * (long)g5_19;
        long f8g6_19 = f8 * (long)g6_19;
        long f8g7_19 = f8 * (long)g7_19;
        long f8g8_19 = f8 * (long)g8_19;
        long f8g9_19 = f8 * (long)g9_19;
        long f9g0 = f9 * (long)g0;
        long f9g1_38 = f9_2 * (long)g1_19;
        long f9g2_19 = f9 * (long)g2_19;
        long f9g3_38 = f9_2 * (long)g3_19;
        long f9g4_19 = f9 * (long)g4_19;
        long f9g5_38 = f9_2 * (long)g5_19;
        long f9g6_19 = f9 * (long)g6_19;
        long f9g7_38 = f9_2 * (long)g7_19;
        long f9g8_19 = f9 * (long)g8_19;
        long f9g9_38 = f9_2 * (long)g9_19;
        long h0 = f0g0 + f1g9_38 + f2g8_19 + f3g7_38 + f4g6_19 + f5g5_38 + f6g4_19 + f7g3_38 + f8g2_19 + f9g1_38;
        long h1 = f0g1 + f1g0 + f2g9_19 + f3g8_19 + f4g7_19 + f5g6_19 + f6g5_19 + f7g4_19 + f8g3_19 + f9g2_19;
        long h2 = f0g2 + f1g1_2 + f2g0 + f3g9_38 + f4g8_19 + f5g7_38 + f6g6_19 + f7g5_38 + f8g4_19 + f9g3_38;
        long h3 = f0g3 + f1g2 + f2g1 + f3g0 + f4g9_19 + f5g8_19 + f6g7_19 + f7g6_19 + f8g5_19 + f9g4_19;
        long h4 = f0g4 + f1g3_2 + f2g2 + f3g1_2 + f4g0 + f5g9_38 + f6g8_19 + f7g7_38 + f8g6_19 + f9g5_38;
        long h5 = f0g5 + f1g4 + f2g3 + f3g2 + f4g1 + f5g0 + f6g9_19 + f7g8_19 + f8g7_19 + f9g6_19;
        long h6 = f0g6 + f1g5_2 + f2g4 + f3g3_2 + f4g2 + f5g1_2 + f6g0 + f7g9_38 + f8g8_19 + f9g7_38;
        long h7 = f0g7 + f1g6 + f2g5 + f3g4 + f4g3 + f5g2 + f6g1 + f7g0 + f8g9_19 + f9g8_19;
        long h8 = f0g8 + f1g7_2 + f2g6 + f3g5_2 + f4g4 + f5g3_2 + f6g2 + f7g1_2 + f8g0 + f9g9_38;
        long h9 = f0g9 + f1g8 + f2g7 + f3g6 + f4g5 + f5g4 + f6g3 + f7g2 + f8g1 + f9g0;
        long carry0;
        long carry1;
        long carry2;
        long carry3;
        long carry4;
        long carry5;
        long carry6;
        long carry7;
        long carry8;
        long carry9;

        carry0 = (h0 + (long)(1 << 25)) >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;
        carry4 = (h4 + (long)(1 << 25)) >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;

        carry1 = (h1 + (long)(1 << 24)) >> 25;
        h2 += carry1;
        h1 -= carry1 << 25;
        carry5 = (h5 + (long)(1 << 24)) >> 25;
        h6 += carry5;
        h5 -= carry5 << 25;

        carry2 = (h2 + (long)(1 << 25)) >> 26;
        h3 += carry2;
        h2 -= carry2 << 26;
        carry6 = (h6 + (long)(1 << 25)) >> 26;
        h7 += carry6;
        h6 -= carry6 << 26;

        carry3 = (h3 + (long)(1 << 24)) >> 25;
        h4 += carry3;
        h3 -= carry3 << 25;
        carry7 = (h7 + (long)(1 << 24)) >> 25;
        h8 += carry7;
        h7 -= carry7 << 25;

        carry4 = (h4 + (long)(1 << 25)) >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;
        carry8 = (h8 + (long)(1 << 25)) >> 26;
        h9 += carry8;
        h8 -= carry8 << 26;

        carry9 = (h9 + (long)(1 << 24)) >> 25;
        h0 += carry9 * 19;
        h9 -= carry9 << 25;

        carry0 = (h0 + (long)(1 << 25)) >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;

        h[0] = (int)h0;
        h[1] = (int)h1;
        h[2] = (int)h2;
        h[3] = (int)h3;
        h[4] = (int)h4;
        h[5] = (int)h5;
        h[6] = (int)h6;
        h[7] = (int)h7;
        h[8] = (int)h8;
        h[9] = (int)h9;
    }

    private static void fe_mul121666(ref Span<int> h, ReadOnlySpan<int> f)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        long h0 = f0 * (long)121666;
        long h1 = f1 * (long)121666;
        long h2 = f2 * (long)121666;
        long h3 = f3 * (long)121666;
        long h4 = f4 * (long)121666;
        long h5 = f5 * (long)121666;
        long h6 = f6 * (long)121666;
        long h7 = f7 * (long)121666;
        long h8 = f8 * (long)121666;
        long h9 = f9 * (long)121666;
        long carry0;
        long carry1;
        long carry2;
        long carry3;
        long carry4;
        long carry5;
        long carry6;
        long carry7;
        long carry8;
        long carry9;

        carry9 = (h9 + (long)(1 << 24)) >> 25;
        h0 += carry9 * 19;
        h9 -= carry9 << 25;
        carry1 = (h1 + (long)(1 << 24)) >> 25;
        h2 += carry1;
        h1 -= carry1 << 25;
        carry3 = (h3 + (long)(1 << 24)) >> 25;
        h4 += carry3;
        h3 -= carry3 << 25;
        carry5 = (h5 + (long)(1 << 24)) >> 25;
        h6 += carry5;
        h5 -= carry5 << 25;
        carry7 = (h7 + (long)(1 << 24)) >> 25;
        h8 += carry7;
        h7 -= carry7 << 25;

        carry0 = (h0 + (long)(1 << 25)) >> 26;
        h1 += carry0;
        h0 -= carry0 << 26;
        carry2 = (h2 + (long)(1 << 25)) >> 26;
        h3 += carry2;
        h2 -= carry2 << 26;
        carry4 = (h4 + (long)(1 << 25)) >> 26;
        h5 += carry4;
        h4 -= carry4 << 26;
        carry6 = (h6 + (long)(1 << 25)) >> 26;
        h7 += carry6;
        h6 -= carry6 << 26;
        carry8 = (h8 + (long)(1 << 25)) >> 26;
        h9 += carry8;
        h8 -= carry8 << 26;

        h[0] = (int)h0;
        h[1] = (int)h1;
        h[2] = (int)h2;
        h[3] = (int)h3;
        h[4] = (int)h4;
        h[5] = (int)h5;
        h[6] = (int)h6;
        h[7] = (int)h7;
        h[8] = (int)h8;
        h[9] = (int)h9;
    }

    private static void fe_pow22523(ref Span<int> @out, ReadOnlySpan<int> z)
    {
        Span<int> t0 = stackalloc int[10];
        Span<int> t1 = stackalloc int[10];
        Span<int> t2 = stackalloc int[10];
        Span<int> result = stackalloc int[10];
        int i;
        
        fe_sq(ref t0, z);

        for (i = 1; i < 1; ++i)
        {
            fe_sq(ref t0, t0);
        }

        fe_sq(ref t1, t0);

        for (i = 1; i < 2; ++i)
        {
            fe_sq(ref t1, t1);
        }

        fe_mul(ref t1, z, t1);
        fe_mul(ref t0, t0, t1);
        fe_sq(ref t0, t0);

        for (i = 1; i < 1; ++i)
        {
            fe_sq(ref t0, t0);
        }

        fe_mul(ref t0, t1, t0);
        fe_sq(ref t1, t0);

        for (i = 1; i < 5; ++i)
        {
            fe_sq(ref t1, t1);
        }

        fe_mul(ref t0, t1, t0);
        fe_sq(ref t1, t0);

        for (i = 1; i < 10; ++i)
        {
            fe_sq(ref t1, t1);
        }

        fe_mul(ref t1, t1, t0);
        fe_sq(ref t2, t1);

        for (i = 1; i < 20; ++i)
        {
            fe_sq(ref t2, t2);
        }

        fe_mul(ref t1, t2, t1);
        fe_sq(ref t1, t1);

        for (i = 1; i < 10; ++i)
        {
            fe_sq(ref t1, t1);
        }

        fe_mul(ref t0, t1, t0);
        fe_sq(ref t1, t0);

        for (i = 1; i < 50; ++i)
        {
            fe_sq(ref t1, t1);
        }

        fe_mul(ref t1, t1, t0);
        fe_sq(ref t2, t1);

        for (i = 1; i < 100; ++i)
        {
            fe_sq(ref t2, t2);
        }

        fe_mul(ref t1, t2, t1);
        fe_sq(ref t1, t1);

        for (i = 1; i < 50; ++i)
        {
            fe_sq(ref t1, t1);
        }

        fe_mul(ref t0, t1, t0);
        fe_sq(ref t0, t0);

        for (i = 1; i < 2; ++i)
        {
            fe_sq(ref t0, t0);
        }

        fe_mul(ref result, t0, z);
        result.CopyTo(@out);
    }

    private static void fe_sub(ref Span<int> h, ReadOnlySpan<int> f, ReadOnlySpan<int> g)
    {
        int f0 = f[0];
        int f1 = f[1];
        int f2 = f[2];
        int f3 = f[3];
        int f4 = f[4];
        int f5 = f[5];
        int f6 = f[6];
        int f7 = f[7];
        int f8 = f[8];
        int f9 = f[9];
        int g0 = g[0];
        int g1 = g[1];
        int g2 = g[2];
        int g3 = g[3];
        int g4 = g[4];
        int g5 = g[5];
        int g6 = g[6];
        int g7 = g[7];
        int g8 = g[8];
        int g9 = g[9];
        int h0 = f0 - g0;
        int h1 = f1 - g1;
        int h2 = f2 - g2;
        int h3 = f3 - g3;
        int h4 = f4 - g4;
        int h5 = f5 - g5;
        int h6 = f6 - g6;
        int h7 = f7 - g7;
        int h8 = f8 - g8;
        int h9 = f9 - g9;

        h[0] = h0;
        h[1] = h1;
        h[2] = h2;
        h[3] = h3;
        h[4] = h4;
        h[5] = h5;
        h[6] = h6;
        h[7] = h7;
        h[8] = h8;
        h[9] = h9;
    }
}