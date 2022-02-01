using System.Security.Cryptography;

namespace GlitchEd25519;

/// <summary>
/// C# translation of the <a href="https://github.com/GlitchedPolygons/GlitchEd25519">GlitchEd25519</a>
/// fork of the <a href="https://github.com/orlp/ed25519">orlp/ed25519</a> C library.<para> </para>
/// Exposes functions for generating Ed25519 key-pairs, performing key exchange operations on them, as well as signing and verifying messages.
/// </summary>
public static partial class GlitchEd25519
{
    /// <summary>
    /// Creates a new random seed to use for key-pair generation. <paramref name="additionalEntropy"/> is supported, but not mandatory (pass <c>Array.Empty&lt;byte&gt; for no additional user-provided entropy).</c>
    /// </summary>
    /// <param name="outSeed">Writable 32B buffer where to write the random seed into.</param>
    /// <param name="additionalEntropy">[OPTIONAL] Additional entropy to add randomness to the (hopefully) already very random seed.</param>
    public static void CreateSeed(ref Span<byte> outSeed, ReadOnlySpan<byte> additionalEntropy)
    {
        Span<byte> result = stackalloc byte[32];
        Span<byte> random = stackalloc byte[64];

        RandomNumberGenerator.Fill(random);
        RandomNumberGenerator.Fill(result);

        SHA256.HashData(additionalEntropy.IsEmpty ? result : additionalEntropy, random[32..]);
        SHA256.HashData(random, result);
        SHA256.HashData(random, result);

        result.CopyTo(outSeed);
    }

    /// <summary>
    /// Creates a new key-pair from the given <paramref name="seed"/>.
    /// </summary>
    /// <param name="outPublicKey">Writable 32 byte buffer in which the public key should be written into.</param>
    /// <param name="outPrivateKey">Writable 64 byte buffer in which the private key (in pre-hashed orlp format format) should be written into.</param>
    /// <param name="seed">32B seed to use for key-pair generation.</param>
    public static void CreateKeypair(ref Span<byte> outPublicKey, ref Span<byte> outPrivateKey, ReadOnlySpan<byte> seed)
    {
        Span<byte> publicKey = stackalloc byte[32];
        Span<byte> privateKey = stackalloc byte[64];

        ge_p3 A = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        SHA512.HashData(seed, privateKey);

        privateKey[0] &= 248;
        privateKey[31] &= 63;
        privateKey[31] |= 64;

        ge_scalarmult_base(ref A, privateKey);
        ge_p3_tobytes(ref publicKey, A);

        privateKey.CopyTo(outPrivateKey);
        publicKey.CopyTo(outPublicKey);
    }

    /// <summary>
    /// Creates a new key pair from the given seed in the Ref10 format (equivalent to what you'd get from the official SUPERCOP Ed25519 implementation and libsodium).
    /// </summary>
    /// <param name="outPublicKey">Writable 32 byte buffer in which the public key should be written into.</param>
    /// <param name="outPrivateKeyRef10">Writable 64 byte buffer in which the private key (in Ref10 format) should be written into.</param>
    /// <param name="seed">32B seed to use for key-pair generation.</param>
    public static void CreateKeypairRef10(ref Span<byte> outPublicKey, ref Span<byte> outPrivateKeyRef10, ReadOnlySpan<byte> seed)
    {
        Span<byte> publicKey = stackalloc byte[32];
        Span<byte> privateKey = stackalloc byte[64];

        ge_p3 A = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        SHA512.HashData(seed, privateKey);

        privateKey[0] &= 248;
        privateKey[31] &= 63;
        privateKey[31] |= 64;

        ge_scalarmult_base(ref A, privateKey);
        ge_p3_tobytes(ref publicKey, A);

        seed.CopyTo(outPrivateKeyRef10);
        publicKey.CopyTo(outPrivateKeyRef10[32..]);
        publicKey.CopyTo(outPublicKey);
    }

    /// <summary>
    /// Converts a private key that was generated using <see cref="CreateKeypairRef10"/>
    /// or via libsodium, NaCl or SUPERCOP (or any other Ed25519 implementation that uses the Ref10 private key format).
    /// </summary>
    /// <param name="outPrivateKeyOrlp">Where to write the 64 bytes of private key into.</param>
    /// <param name="privateKeyRef10">The private Ed25519 key in Ref10 format to convert.</param>
    public static void ConvertKeyFromRef10ToOrlp(ref Span<byte> outPrivateKeyOrlp, ReadOnlySpan<byte> privateKeyRef10)
    {
        Span<byte> privateKeyOrlp = stackalloc byte[64];

        SHA512.HashData(privateKeyRef10[..32], privateKeyOrlp);

        privateKeyOrlp[0] &= 248;
        privateKeyOrlp[31] &= 63;
        privateKeyOrlp[31] |= 64;

        privateKeyOrlp.CopyTo(outPrivateKeyOrlp);
    }

    /// <summary>
    /// Performs a key exchange on the given public key and private key, producing a shared secret.<para> </para>
    /// It is recommended to hash the shared secret before using it.<para> </para>
    /// <paramref name="outSharedSecret"/> must be a 32 byte writable buffer where the shared secret will be stored.
    /// </summary>
    /// <param name="outSharedSecret">32B writable output buffer where to store the shared secret.</param>
    /// <param name="publicKey">32B of very readable, high quality public key material.</param>
    /// <param name="privateKey">64B private key buffer.</param>
    public static void KeyExchange(ref Span<byte> outSharedSecret, ReadOnlySpan<byte> publicKey, ReadOnlySpan<byte> privateKey)
    {
        Span<byte> sharedSecret = stackalloc byte[32];
        Span<byte> e = stackalloc byte[32];
        Span<int> x1 = stackalloc int[10];
        Span<int> x2 = stackalloc int[10];
        Span<int> z2 = stackalloc int[10];
        Span<int> x3 = stackalloc int[10];
        Span<int> z3 = stackalloc int[10];
        Span<int> tmp0 = stackalloc int[10];
        Span<int> tmp1 = stackalloc int[10];

        int pos;
        uint b;
        uint swap;

        /* copy the private key and make sure it's valid */
        for (int i = 0; i < 32; ++i)
        {
            e[i] = privateKey[i];
        }

        e[0] &= 248;
        e[31] &= 63;
        e[31] |= 64;

        /* unpack the public key and convert edwards to montgomery */
        /* due to CodesInChaos: montgomeryX = (edwardsY + 1)*inverse(1 - edwardsY) mod p */
        fe_frombytes(ref x1, publicKey);
        fe_1(ref tmp1);
        fe_add(ref tmp0, x1, tmp1);
        fe_sub(ref tmp1, tmp1, x1);
        fe_invert(ref tmp1, tmp1);
        fe_mul(ref x1, tmp0, tmp1);

        fe_1(ref x2);
        fe_0(ref z2);
        fe_copy(ref x3, x1);
        fe_1(ref z3);

        swap = 0;
        for (pos = 254; pos >= 0; --pos)
        {
            b = (uint)(e[pos / 8] >> (pos & 7));
            b &= 1;
            swap ^= b;
            fe_cswap(ref x2, ref x3, swap);
            fe_cswap(ref z2, ref z3, swap);
            swap = b;

            /* from montgomery.h */
            fe_sub(ref tmp0, x3, z3);
            fe_sub(ref tmp1, x2, z2);
            fe_add(ref x2, x2, z2);
            fe_add(ref z2, x3, z3);
            fe_mul(ref z3, tmp0, x2);
            fe_mul(ref z2, z2, tmp1);
            fe_sq(ref tmp0, tmp1);
            fe_sq(ref tmp1, x2);
            fe_add(ref x3, z3, z2);
            fe_sub(ref z2, z3, z2);
            fe_mul(ref x2, tmp1, tmp0);
            fe_sub(ref tmp1, tmp1, tmp0);
            fe_sq(ref z2, z2);
            fe_mul121666(ref z3, tmp1);
            fe_sq(ref x3, x3);
            fe_add(ref tmp0, tmp0, z3);
            fe_mul(ref z3, x1, z2);
            fe_mul(ref z2, tmp1, tmp0);
        }

        fe_cswap(ref x2, ref x3, swap);
        fe_cswap(ref z2, ref z3, swap);

        fe_invert(ref z2, z2);
        fe_mul(ref x2, x2, z2);
        fe_tobytes(ref sharedSecret, x2);

        sharedSecret.CopyTo(outSharedSecret);
    }

    /// <summary>
    /// Adds scalar to the given key pair where scalar is a 32 byte buffer (possibly generated with ed25519_create_seed), generating a new key pair.<para> </para>
    /// You can calculate the public key sum without knowing the private key and vice versa by passing in NULL for the key you don't know.<para> </para>
    /// This is useful for enforcing randomness on a key pair by a third party while only knowing the public key, among other things.<para> </para>
    /// Warning: the last bit of the scalar is ignored - if comparing scalars, make sure to clear it with <c>scalar[31] &= 127</c>.
    /// </summary>
    /// <seealso href="http://crypto.stackexchange.com/a/6215/4697"/>
    /// <param name="outPublicKey">32B R/W public key buffer.</param>
    /// <param name="outPrivateKey">64B R/W private key buffer.</param>
    /// <param name="scalar">32B RO scalar buffer.</param>
    public static void AddScalar(ref Span<byte> outPublicKey, ref Span<byte> outPrivateKey, ReadOnlySpan<byte> scalar)
    {
        /* scalar with value 1 */
        Span<byte> SC_1 = stackalloc byte[32];
        SC_1.Fill(1);

        Span<byte> n = stackalloc byte[32];
        Span<byte> publicKey = outPublicKey.IsEmpty ? Array.Empty<byte>() : stackalloc byte[32];
        Span<byte> privateKey = outPrivateKey.IsEmpty ? Array.Empty<byte>() : stackalloc byte[32];

        if (publicKey.Length != 0)
            outPublicKey.CopyTo(publicKey);

        if (privateKey.Length != 0)
            outPrivateKey.CopyTo(privateKey);

        ge_p3 nB = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        ge_p1p1 A_p1p1 = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        ge_p3 A = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        ge_p3 publicKeyUnpacked = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        ge_cached T = new()
        {
            YminusX = stackalloc int[10],
            YplusX = stackalloc int[10],
            T2d = stackalloc int[10],
            Z = stackalloc int[10],
        };

        Span<byte> hashbuf = stackalloc byte[64];

        /* copy the scalar and clear highest bit */
        for (int i = 0; i < 31; ++i)
        {
            n[i] = scalar[i];
        }

        n[31] = (byte)(scalar[31] & 127);

        /* private key: a = n + t */
        if (!privateKey.IsEmpty)
        {
            sc_muladd(ref privateKey, SC_1, n, privateKey);

            // https://github.com/orlp/ed25519/issues/3
            using IncrementalHash sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);
            sha512.AppendData(privateKey[32..]);
            sha512.AppendData(scalar);
            sha512.GetHashAndReset(hashbuf);

            for (int i = 0; i < 32; ++i)
            {
                privateKey[32 + i] = hashbuf[i];
            }

            privateKey.CopyTo(outPrivateKey);
        }

        /* public key: A = nB + T */
        if (!publicKey.IsEmpty)
        {
            /* if we know the private key we don't need a point addition, which is faster */
            /* using a "timing attack" you could find out wether or not we know the private
               key, but this information seems rather useless - if this is important pass
               public_key and private_key seperately in 2 function calls */
            if (!privateKey.IsEmpty)
            {
                ge_scalarmult_base(ref A, privateKey);
            }
            else
            {
                /* unpack public key into T */
                ge_frombytes_negate_vartime(ref publicKeyUnpacked, publicKey);
                fe_neg(ref publicKeyUnpacked.X, publicKeyUnpacked.X); /* undo negate */
                fe_neg(ref publicKeyUnpacked.T, publicKeyUnpacked.T); /* undo negate */
                ge_p3_to_cached(ref T, publicKeyUnpacked);

                /* calculate n*B */
                ge_scalarmult_base(ref nB, n);

                /* A = n*B + T */
                ge_add(ref A_p1p1, nB, T);
                ge_p1p1_to_p3(ref A, A_p1p1);
            }

            /* pack public key */
            ge_p3_tobytes(ref publicKey, A);

            publicKey.CopyTo(outPublicKey);
        }
    }

    /// <summary>
    /// Creates a signature of the given <paramref name="message"/> with the given key pair.
    /// The <paramref name="outSignature"/> argument must be a writable 64 byte buffer.
    /// </summary>
    /// <param name="outSignature">Where to write the signature into (must be at least 64B of writable output buffer).</param>
    /// <param name="message">The data to sign.</param>
    /// <param name="publicKey">Public key (readable 32B buffer).</param>
    /// <param name="privateKey">Private key (readable 64B buffer).</param>
    public static void Sign(ref Span<byte> outSignature, ReadOnlySpan<byte> message, ReadOnlySpan<byte> publicKey, ReadOnlySpan<byte> privateKey)
    {
        ge_p3 R = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        Span<byte> s0 = stackalloc byte[32];
        Span<byte> s1 = stackalloc byte[32];
        Span<byte> r = stackalloc byte[64];
        Span<byte> hram = stackalloc byte[64];

        using IncrementalHash sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);

        sha512.AppendData(privateKey[32..]);
        sha512.AppendData(message);
        sha512.GetHashAndReset(r);

        sc_reduce(ref r);
        ge_scalarmult_base(ref R, r);
        ge_p3_tobytes(ref s0, R);

        sha512.AppendData(s0[..32]);
        sha512.AppendData(publicKey);
        sha512.AppendData(message);
        sha512.GetHashAndReset(hram);

        sc_reduce(ref hram);
        sc_muladd(ref s1, hram, privateKey, r);

        s0.CopyTo(outSignature[..32]);
        s1.CopyTo(outSignature[32..]);
    }

    /// <summary>
    /// Creates a signature of the given <paramref name="message"/> using only the given Ref10 (SUPERCOP) format Ed25519 private key. 
    /// </summary>
    /// <param name="outSignature">Where to write the signature into (must be at least 64B of writable output buffer).</param>
    /// <param name="message">The data to sign.</param>
    /// <param name="privateKeyRef10">Private key (readable 64B buffer containing the private key in Ref10-format).</param>
    public static void SignRef10(ref Span<byte> outSignature, ReadOnlySpan<byte> message, ReadOnlySpan<byte> privateKeyRef10)
    {
        ge_p3 R = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        Span<byte> s0 = stackalloc byte[32];
        Span<byte> s1 = stackalloc byte[32];
        Span<byte> a = stackalloc byte[64];
        Span<byte> hram = stackalloc byte[64];
        Span<byte> nonce = stackalloc byte[64];

        using IncrementalHash sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);

        sha512.AppendData(privateKeyRef10[..32]);
        sha512.GetHashAndReset(a);

        a[0] &= 248;
        a[31] &= 63;
        a[31] |= 64;

        sha512.AppendData(a[32..]);
        sha512.AppendData(message);
        sha512.GetHashAndReset(nonce);

        privateKeyRef10[32..].CopyTo(s1);

        sc_reduce(ref nonce);
        ge_scalarmult_base(ref R, nonce);
        ge_p3_tobytes(ref s0, R);

        sha512.AppendData(s0);
        sha512.AppendData(s1);
        sha512.AppendData(message);
        sha512.GetHashAndReset(hram);

        sc_reduce(ref hram);
        sc_muladd(ref s1, hram, a, nonce);

        s0.CopyTo(outSignature[..32]);
        s1.CopyTo(outSignature[32..]);

        a.Clear();
        nonce.Clear();
    }

    /// <summary>
    /// Verifies the <paramref name="signature"/> on the given <paramref name="message"/> using the provided <paramref name="publicKey"/>.<para> </para>
    /// <paramref name="signature"/> must be a readable 64B buffer. 
    /// </summary>
    /// <param name="signature">Readable 64B buffer containing the signature to verify.</param>
    /// <param name="message">Message to test the provided <paramref name="signature"/> against.</param>
    /// <param name="publicKey">The public Ed25519 key with which to verify the signature.</param>
    /// <returns>Whether or not the provided <paramref name="signature"/> could be validated successfully against the passed <paramref name="message"/> + <paramref name="publicKey"/>.</returns>
    public static bool Verify(ReadOnlySpan<byte> signature, ReadOnlySpan<byte> message, ReadOnlySpan<byte> publicKey)
    {
        Span<byte> h = stackalloc byte[64];
        Span<byte> checker = stackalloc byte[32];

        ge_p3 A = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
            T = stackalloc int[10],
        };

        ge_p2 R = new()
        {
            X = stackalloc int[10],
            Y = stackalloc int[10],
            Z = stackalloc int[10],
        };

        using IncrementalHash sha512 = IncrementalHash.CreateHash(HashAlgorithmName.SHA512);

        int r1 = signature[63] & 224;
        int r2 = ge_frombytes_negate_vartime(ref A, publicKey);

        sha512.AppendData(signature[..32]);
        sha512.AppendData(publicKey);
        sha512.AppendData(message);
        sha512.GetHashAndReset(h);

        sc_reduce(ref h);
        ge_double_scalarmult_vartime(ref R, h, A, signature[32..]);
        ge_tobytes(ref checker, R);

        return r1 == 0 && r2 == 0 && consttime_equal(checker, signature);
    }
}