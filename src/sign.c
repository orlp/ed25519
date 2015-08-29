#include <string.h>
#include "ed25519.h"
#include "sha512.h"
#include "ge.h"
#include "sc.h"


void ed25519_sign(unsigned char *signature, const unsigned char *message, size_t message_len, const unsigned char *private_key) {
    sha512_context hash;
    unsigned char hram[64];
    unsigned char nonce[64];
    unsigned char az[64];
    ge_p3 R;

    sha512(private_key, 32, az);
    az[0] &= 248;
    az[31] &= 63;
    az[31] |= 64;

    sha512_init(&hash);
    sha512_update(&hash, az + 32, 32);
    sha512_update(&hash, message, message_len);
    sha512_final(&hash, nonce);

    memmove(signature + 32, private_key + 32, 32);

    sc_reduce(nonce);
    ge_scalarmult_base(&R, nonce);
    ge_p3_tobytes(signature, &R);

    sha512_init(&hash);
    sha512_update(&hash, signature, 64);
    sha512_update(&hash, message, message_len);
    sha512_final(&hash, hram);

    sc_reduce(hram);
    sc_muladd(signature + 32, hram, az, nonce);

    memset(az, 0, sizeof(az));
    memset(nonce, 0, sizeof(nonce));
}
