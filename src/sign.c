#include "ed25519.h"
#include "sha512.h"
#include "ge.h"
#include "sc.h"
#include <string.h>

void ed25519_sign(unsigned char* signature, const unsigned char* message, size_t message_len, const unsigned char* public_key, const unsigned char* private_key)
{
    sha512_context hash;
    unsigned char hram[64];
    unsigned char r[64];
    ge_p3 R;

    sha512_init(&hash);
    sha512_update(&hash, private_key + 32, 32);
    sha512_update(&hash, message, message_len);
    sha512_final(&hash, r);

    sc_reduce(r);
    ge_scalarmult_base(&R, r);
    ge_p3_tobytes(signature, &R);

    sha512_init(&hash);
    sha512_update(&hash, signature, 32);
    sha512_update(&hash, public_key, 32);
    sha512_update(&hash, message, message_len);
    sha512_final(&hash, hram);

    sc_reduce(hram);
    sc_muladd(signature + 32, hram, private_key, r);
}

void ed25519_sign_ref10(unsigned char* signature, const unsigned char* message, size_t message_len, const unsigned char* private_key_ref10)
{
    sha512_context hash;
    unsigned char hram[64];
    unsigned char nonce[64];
    unsigned char a[64];
    ge_p3 R;

    sha512(private_key_ref10, 32, a);
    a[0] &= 248;
    a[31] &= 63;
    a[31] |= 64;

    sha512_init(&hash);
    sha512_update(&hash, a + 32, 32);
    sha512_update(&hash, message, message_len);
    sha512_final(&hash, nonce);

    memmove(signature + 32, private_key_ref10 + 32, 32);

    sc_reduce(nonce);
    ge_scalarmult_base(&R, nonce);
    ge_p3_tobytes(signature, &R);

    sha512_init(&hash);
    sha512_update(&hash, signature, 64);
    sha512_update(&hash, message, message_len);
    sha512_final(&hash, hram);

    sc_reduce(hram);
    sc_muladd(signature + 32, hram, a, nonce);

    memset(a, 0x00, sizeof(a));
    memset(nonce, 0x00, sizeof(nonce));
}