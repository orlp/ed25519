#ifndef ED25519_H
#define ED25519_H

#include <stddef.h>

#if defined(_WIN32)
    #if defined(ED25519_BUILD_DLL)
        #define ED25519_DECLSPEC __declspec(dllexport)
    #elif defined(ED25519_DLL)
        #define ED25519_DECLSPEC __declspec(dllimport)
    #else
        #define ED25519_DECLSPEC
    #endif
#else
    #define ED25519_DECLSPEC
#endif


#ifdef __cplusplus
extern "C" {
#endif

#ifndef ED25519_NO_SEED
int ED25519_DECLSPEC ed25519_create_seed(unsigned char *seed);
#endif

#include "sha512.h"
#include "ge.h"
typedef struct
{
    sha512_context hash_ct;
    unsigned char r[64];
} signature_context;
typedef struct
{
    sha512_context hash_ct;
    ge_p3 A;
    int bool_number;
    unsigned char *sig;
} verify_context;

void ED25519_DECLSPEC ed25519_create_keypair(unsigned char *public_key, unsigned char *private_key, const unsigned char *seed);
void ED25519_DECLSPEC ed25519_sign(unsigned char *signature, const unsigned char *message, size_t message_len, const unsigned char *public_key, const unsigned char *private_key);
int ED25519_DECLSPEC ed25519_verify(const unsigned char *signature, const unsigned char *message, size_t message_len, const unsigned char *public_key);
void ED25519_DECLSPEC ed25519_add_scalar(unsigned char *public_key, unsigned char *private_key, const unsigned char *scalar);
void ED25519_DECLSPEC ed25519_key_exchange(unsigned char *shared_secret, const unsigned char *public_key, const unsigned char *private_key);

void ED25519_DECLSPEC ed25519_create_public_from_private(unsigned char *public_key, unsigned char *private_key);
void ED25519_DECLSPEC ed25519_sign_init1(signature_context *sig_status, const unsigned char *private_key);
void ED25519_DECLSPEC ed25519_sign_update1(signature_context *sig_status, const unsigned char *message_block, size_t block_len);
void ED25519_DECLSPEC ed25519_sign_final1_init2(signature_context *sig_status, unsigned char *signature, const unsigned char *public_key);
void ED25519_DECLSPEC ed25519_sign_update2(signature_context *sig_status, const unsigned char *message_block, size_t block_len);
void ED25519_DECLSPEC ed25519_sign_final2(signature_context *sig_status, unsigned char *signature, const unsigned char *private_key);
void ED25519_DECLSPEC ed25519_verify_init(verify_context *ver_status, const unsigned char *signature, const unsigned char *public_key);
void ED25519_DECLSPEC ed25519_verify_update(verify_context *ver_status, const unsigned char *message_block, size_t block_len);
int ED25519_DECLSPEC ed25519_verify_final(verify_context *ver_status);

#ifdef __cplusplus
}
#endif

#endif
