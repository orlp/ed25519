#ifndef ORLP_ED25519_H
#define ORLP_ED25519_H

#include <stddef.h>

#if defined(_WIN32)
    #if defined(ORLP_ED25519_BUILD_DLL)
        #define ORLP_ED25519_DECLSPEC __declspec(dllexport)
    #elif defined(ORLP_ED25519_DLL)
        #define ORLP_ED25519_DECLSPEC __declspec(dllimport)
    #else
        #define ORLP_ED25519_DECLSPEC
    #endif
#else
    #define ORLP_ED25519_DECLSPEC
#endif


#ifdef __cplusplus
extern "C" {
#endif

#ifndef ED25519_NO_SEED
int ORLP_ED25519_DECLSPEC ed25519_create_seed(unsigned char *seed);
#endif

ORLP_ED25519_DECLSPEC void ed25519_create_keypair(unsigned char* public_key, unsigned char* private_key, const unsigned char* seed);
ORLP_ED25519_DECLSPEC void ed25519_create_keypair_ref10(unsigned char* public_key, unsigned char* private_key_ref10, const unsigned char* seed);
ORLP_ED25519_DECLSPEC void ed25519_sign(unsigned char* signature, const unsigned char* message, size_t message_len, const unsigned char* public_key, const unsigned char* private_key);
ORLP_ED25519_DECLSPEC void ed25519_sign_ref10(unsigned char* signature, const unsigned char* message, size_t message_len, const unsigned char* private_key_ref10);
ORLP_ED25519_DECLSPEC int  ed25519_verify(const unsigned char* signature, const unsigned char* message, size_t message_len, const unsigned char* public_key);
ORLP_ED25519_DECLSPEC void ed25519_add_scalar(unsigned char* public_key, unsigned char* private_key, const unsigned char* scalar);
ORLP_ED25519_DECLSPEC void ed25519_key_exchange(unsigned char* shared_secret, const unsigned char* public_key, const unsigned char* private_key);
ORLP_ED25519_DECLSPEC void ed25519_key_convert_ref10_to_orlp(const unsigned char* private_key_ref10, unsigned char* private_key_orlp);

#ifdef __cplusplus
}
#endif

#endif // ORLP_ED25519_H
