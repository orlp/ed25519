#include "ed25519.h"
#include "sha512.h"
#include "ge.h"
#include "sc.h"

static int consttime_equal(const unsigned char *x, const unsigned char *y) {
    unsigned char r = 0;

    r = x[0] ^ y[0];
    #define F(i) r |= x[i] ^ y[i]
    F(1);
    F(2);
    F(3);
    F(4);
    F(5);
    F(6);
    F(7);
    F(8);
    F(9);
    F(10);
    F(11);
    F(12);
    F(13);
    F(14);
    F(15);
    F(16);
    F(17);
    F(18);
    F(19);
    F(20);
    F(21);
    F(22);
    F(23);
    F(24);
    F(25);
    F(26);
    F(27);
    F(28);
    F(29);
    F(30);
    F(31);
    #undef F

    return !r;
}

int ed25519_verify(const unsigned char *signature, const unsigned char *message, size_t message_len, const unsigned char *public_key)
{
    unsigned char h[64];
    unsigned char checker[32];
    sha512_context hash;
    ge_p3 A;
    ge_p2 R;

    if (signature[63] & 224)
    {
        return 0;
    }

    if (ge_frombytes_negate_vartime(&A, public_key) != 0)
    {
        return 0;
    }

    sha512_init(&hash);
    sha512_update(&hash, signature, 32);
    sha512_update(&hash, public_key, 32);
    sha512_update(&hash, message, message_len);
    sha512_final(&hash, h);

    sc_reduce(h);
    ge_double_scalarmult_vartime(&R, h, &A, signature + 32);
    ge_tobytes(checker, &R);

    if (!consttime_equal(checker, signature))
    {
        return 0;
    }

    return 1;
}

void ed25519_verify_init(verify_context *ver_status, const unsigned char *signature, const unsigned char *public_key)
{
    ver_status->bool_number = 1;
    ver_status->sig = signature;
    if (signature[63] & 224)
    {
        ver_status->bool_number = 0;
    }

    if (ge_frombytes_negate_vartime(&ver_status->A, public_key) != 0)
    {
        ver_status->bool_number = 0;
    }

    sha512_init(&ver_status->hash_ct);
    sha512_update(&ver_status->hash_ct, signature, 32);
    sha512_update(&ver_status->hash_ct, public_key, 32);
}

void ed25519_verify_update(verify_context *ver_status, const unsigned char *message_block, size_t block_len)
{
    if (ver_status->bool_number)
    {
        sha512_update(&ver_status->hash_ct, message_block, block_len);
    }
}

int ed25519_verify_final(verify_context *ver_status)
{
    unsigned char h[64];
    unsigned char checker[32];
    ge_p2 R;

    if (ver_status->bool_number)
    {
        sha512_final(&ver_status->hash_ct, h);

        sc_reduce(h);
        ge_double_scalarmult_vartime(&R, h, &ver_status->A, ver_status->sig + 32);
        ge_tobytes(checker, &R);

        if (!consttime_equal(checker, ver_status->sig))
        {
            return 0;
        }
        else
        {
            return 1;
        }
    }
    else
    {
        return 0;
    }
}
