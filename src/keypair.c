#include <string.h>
#include "ed25519.h"
#include "sha512.h"
#include "ge.h"

void ed25519_create_keypair(unsigned char *public_key, unsigned char *private_key, const unsigned char *seed) {
    ge_p3 A;

    sha512(seed, 32, private_key);
    private_key[0] &= 248;
    private_key[31] &= 63;
    private_key[31] |= 64;

    ge_scalarmult_base(&A, private_key);
    ge_p3_tobytes(public_key, &A);
}

void ed25519_create_keypair_ref10(unsigned char *public_key, unsigned char *private_key_ref10, const unsigned char *seed) {
    ge_p3 A;

    sha512(seed, 32, private_key_ref10);
    private_key_ref10[0] &= 248;
    private_key_ref10[31] &= 63;
    private_key_ref10[31] |= 64;

    ge_scalarmult_base(&A, private_key_ref10);
    ge_p3_tobytes(public_key, &A);

    memcpy(memcpy(private_key_ref10, seed, 32) + 32, public_key, 32);
}