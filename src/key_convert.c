#include "../include/orlp-ed25519/ed25519.h"
#include "../include/orlp-ed25519/sha512.h"

void ed25519_key_convert_ref10_to_orlp(const unsigned char *private_key_ref10, unsigned char *private_key_orlp) {
  sha512(private_key_ref10, 32, private_key_orlp);
  private_key_orlp[0] &= 248;
  private_key_orlp[31] &= 63;
  private_key_orlp[31] |= 64;
}
