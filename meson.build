project('ed25519', 'c', version : '0.1.0')

version = meson.project_version()

src = [
'src/add_scalar.c',
'src/seed.c',
'src/verify.c',
'src/add_scalar.c',
'src/sha512.c',
'src/ge.c',
'src/fe.c',
'src/keypair.c',
'src/sign.c',
'src/sc.c',
'src/key_exchange.c']

include_dirs = include_directories('src/')

install_headers('src/ed25519.h', subdir:'ed25519')
lib = library('ed25519',
sources: src,
include_directories : include_dirs,
version: version,
c_args : ['-O2', '-Wall'],
install: true)

ed25519_dep = declare_dependency(include_directories : include_dirs,
  link_with : lib)
