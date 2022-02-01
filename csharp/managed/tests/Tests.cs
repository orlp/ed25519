using System;
using System.Text;
using Xunit;
using Xunit.Abstractions;

public class GlitchEd25519Tests
{
    private readonly ITestOutputHelper testOutputHelper;

    public GlitchEd25519Tests(ITestOutputHelper testOutputHelper)
    {
        this.testOutputHelper = testOutputHelper;
    }

    [Fact]
    public void SmokeTest()
    {
        Span<byte> seed = stackalloc byte[32];
        Span<byte> otherSeed = stackalloc byte[32];
        Span<byte> publicKey = stackalloc byte[32];
        Span<byte> privateKey = stackalloc byte[64];
        Span<byte> otherPublicKey = stackalloc byte[32];
        Span<byte> otherPrivateKey = stackalloc byte[64];
        Span<byte> privateKeyRef10 = stackalloc byte[64];
        Span<byte> signature = stackalloc byte[64];
        Span<byte> signatureRef10 = stackalloc byte[64];
        Span<byte> sharedSecret = stackalloc byte[32];
        Span<byte> sharedSecretCmp = stackalloc byte[32];

        byte[] message = Encoding.UTF8.GetBytes("lorem ipsum dolor sick fuck something something ..");

        GlitchEd25519.CreateSeed(ref seed, Encoding.UTF8.GetBytes("additional entropy here plz - as random as possible!"));
        GlitchEd25519.CreateKeypairRef10(ref publicKey, ref privateKeyRef10, seed);
        GlitchEd25519.CreateKeypair(ref otherPublicKey, ref otherPrivateKey, otherSeed);
        GlitchEd25519.ConvertKeyFromRef10ToOrlp(ref privateKey, privateKeyRef10);
        GlitchEd25519.Sign(ref signature, message, publicKey, privateKey);
        GlitchEd25519.SignRef10(ref signatureRef10, message, privateKeyRef10);

        bool signatureValid = GlitchEd25519.Verify(signature, message, publicKey);

        Assert.True(signatureValid);
        Assert.Equal(signature.ToArray(), signatureRef10.ToArray());

        testOutputHelper.WriteLine($"Message: \"{Encoding.UTF8.GetString(message)}\"");
        testOutputHelper.WriteLine($"Seed: {Convert.ToHexString(seed).ToLower()}");
        testOutputHelper.WriteLine($"Public key: {Convert.ToHexString(publicKey).ToLower()}");
        testOutputHelper.WriteLine($"Private key: {Convert.ToHexString(privateKey).ToLower()}");
        testOutputHelper.WriteLine($"Private ref10 key: {Convert.ToHexString(privateKeyRef10).ToLower()}");
        testOutputHelper.WriteLine($"Signature: {Convert.ToHexString(signature).ToLower()}");
        testOutputHelper.WriteLine($"Signature valid: {signatureValid}");

        GlitchEd25519.KeyExchange(ref sharedSecret, otherPublicKey, privateKey);
        GlitchEd25519.KeyExchange(ref sharedSecretCmp, publicKey, otherPrivateKey);

        Assert.Equal(sharedSecret.ToArray(), sharedSecretCmp.ToArray());

        testOutputHelper.WriteLine($"Shared secret: {Convert.ToHexString(sharedSecret).ToLower()}");
    }

    [Fact]
    public void LibSodiumSignatureValid()
    {
        Span<byte> publicKey = Convert.FromHexString("9c84cc34c855e1c4cbcdefd09be4f234b9f81b95b8ff46ba352c6b07de922dc6");
        Span<byte> signature = Convert.FromHexString("a83095e7f8a7a9038f4d98a808ca13d0209add4004de1dc5000deb9184f138826ed8bc2605526f15a18f8b3ee0cb3d00fdf99ab2de4e6906d726103bf62e3108");
        Span<byte> message = Encoding.UTF8.GetBytes("Test string test test 123 ayyyy");

        Assert.True(GlitchEd25519.Verify(signature, message, publicKey));
    }
    
    [Fact]
    public void LibSodiumWrongPublicKeySignatureInvalid()
    {
        Span<byte> publicKey = Convert.FromHexString("3c38f9f8619cecc6d81a7c2493e4f78aa2b60d403a0c77c345c2003ad8e79593");
        Span<byte> signature = Convert.FromHexString("a83095e7f8a7a9038f4d98a808ca13d0209add4004de1dc5000deb9184f138826ed8bc2605526f15a18f8b3ee0cb3d00fdf99ab2de4e6906d726103bf62e3108");
        Span<byte> message = Encoding.UTF8.GetBytes("Test string test test 123 ayyyy");

        Assert.False(GlitchEd25519.Verify(signature, message, publicKey));
    }
}