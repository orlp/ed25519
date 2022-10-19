cmake_minimum_required(VERSION 3.19.1)

project(ed25519)

# Compile options
set(ARCH "x86_64" CACHE STRING "Processor architecture when targeting multiple bitmodes.")

set(
        SOURCES
        src/add_scalar.c
        src/fe.c
        src/ge.c
        src/key_exchange.c
        src/keypair.c
        src/sc.c
        src/seed.c
        src/sha512.c
        src/sign.c
        src/verify.c
)

# TODO>> MSVC flags
if (${ARCH} STREQUAL "x86_64")
    set(COMMON_COMPILER_OPTIONS "-m64")
elseif (${ARCH} STREQUAL "i686")
    set(COMMON_COMPILER_OPTIONS "-m32")
endif()

add_library(${PROJECT_NAME} STATIC ${SOURCES})
target_compile_options(
        ${PROJECT_NAME}
        BEFORE PRIVATE
        ${COMMON_COMPILER_OPTIONS}
        -O3
        -Wall
        -Wextra
        -Wpedantic
)

add_library(${PROJECT_NAME}-dyn SHARED ${SOURCES})
set_target_properties(${PROJECT_NAME}-dyn PROPERTIES OUTPUT_NAME ${PROJECT_NAME})
target_compile_options(
        ${PROJECT_NAME}-dyn
        BEFORE PRIVATE
        ${COMMON_COMPILER_OPTIONS}
        -O3
        -Wall
        -Wextra
        -Wpedantic
)

add_executable(${PROJECT_NAME}-test src/test.c)
target_compile_options(${PROJECT_NAME}-test PRIVATE ${COMMON_COMPILER_OPTIONS})
target_link_libraries(${PROJECT_NAME}-test ${PROJECT_NAME})
