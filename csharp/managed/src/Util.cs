public static partial class GlitchEd25519
{
    private static ulong load_3(ReadOnlySpan<byte> input)
    {
        ulong result = (ulong)input[0];

        result |= (ulong)input[1] << 8;
        result |= (ulong)input[2] << 16;

        return result;
    }

    private static ulong load_4(ReadOnlySpan<byte> input)
    {
        ulong result = (ulong)input[0];

        result |= (ulong)input[1] << 8;
        result |= (ulong)input[2] << 16;
        result |= (ulong)input[3] << 24;

        return result;
    }

    private static bool consttime_equal(ReadOnlySpan<byte> x, ReadOnlySpan<byte> y)
    {
        byte r = (byte)(x[0] ^ y[0]);

        r |= (byte)(x[1] ^ y[1]);
        r |= (byte)(x[2] ^ y[2]);
        r |= (byte)(x[3] ^ y[3]);
        r |= (byte)(x[4] ^ y[4]);
        r |= (byte)(x[5] ^ y[5]);
        r |= (byte)(x[6] ^ y[6]);
        r |= (byte)(x[7] ^ y[7]);
        r |= (byte)(x[8] ^ y[8]);
        r |= (byte)(x[9] ^ y[9]);
        r |= (byte)(x[10] ^ y[10]);
        r |= (byte)(x[11] ^ y[11]);
        r |= (byte)(x[12] ^ y[12]);
        r |= (byte)(x[13] ^ y[13]);
        r |= (byte)(x[14] ^ y[14]);
        r |= (byte)(x[15] ^ y[15]);
        r |= (byte)(x[16] ^ y[16]);
        r |= (byte)(x[17] ^ y[17]);
        r |= (byte)(x[18] ^ y[18]);
        r |= (byte)(x[19] ^ y[19]);
        r |= (byte)(x[20] ^ y[20]);
        r |= (byte)(x[21] ^ y[21]);
        r |= (byte)(x[22] ^ y[22]);
        r |= (byte)(x[23] ^ y[23]);
        r |= (byte)(x[24] ^ y[24]);
        r |= (byte)(x[25] ^ y[25]);
        r |= (byte)(x[26] ^ y[26]);
        r |= (byte)(x[27] ^ y[27]);
        r |= (byte)(x[28] ^ y[28]);
        r |= (byte)(x[29] ^ y[29]);
        r |= (byte)(x[30] ^ y[30]);
        r |= (byte)(x[31] ^ y[31]);

        return r == 0;
    }
}