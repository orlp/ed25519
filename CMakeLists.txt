cmake_minimum_required(VERSION 3.12)
project(orlp-ed25519 C)

set(CMAKE_C_STANDARD 11)

set(ORLP_ED25519_MAJOR 1)
set(ORLP_ED25519_MINOR 0)
set(ORLP_ED25519_PATCH 0)
set(ORLP_ED25519_VERSION_STRING "${ORLP_ED25519_MAJOR}.${ORLP_ED25519_MINOR}.${ORLP_ED25519_PATCH}")

option(ORLP_ED25519_DLL "Use as a DLL." OFF)
option(ORLP_ED25519_BUILD_DLL "Build as a DLL." OFF)
option(ORLP_ED25519_PACKAGE "Build the library and package it into a .tar.gz after successfully building." OFF)

if (${ORLP_ED25519_BUILD_DLL})
    add_compile_definitions("ORLP_ED25519_BUILD_DLL=1")
    set(ORLP_ED25519_DLL ON)
endif ()

if (${ORLP_ED25519_DLL})
    add_compile_definitions("ORLP_ED25519_DLL=1")
endif ()

set(ORLP_ED25519_SRC
        ${CMAKE_CURRENT_LIST_DIR}/src/precomp_data.h
        ${CMAKE_CURRENT_LIST_DIR}/src/fixedint.h
        ${CMAKE_CURRENT_LIST_DIR}/src/ed25519.h
        ${CMAKE_CURRENT_LIST_DIR}/src/sha512.h
        ${CMAKE_CURRENT_LIST_DIR}/src/ge.h
        ${CMAKE_CURRENT_LIST_DIR}/src/fe.h
        ${CMAKE_CURRENT_LIST_DIR}/src/sc.h
        ${CMAKE_CURRENT_LIST_DIR}/src/ge.c
        ${CMAKE_CURRENT_LIST_DIR}/src/fe.c
        ${CMAKE_CURRENT_LIST_DIR}/src/sc.c
        ${CMAKE_CURRENT_LIST_DIR}/src/seed.c
        ${CMAKE_CURRENT_LIST_DIR}/src/sha512.c
        ${CMAKE_CURRENT_LIST_DIR}/src/keypair.c
        ${CMAKE_CURRENT_LIST_DIR}/src/sign.c
        ${CMAKE_CURRENT_LIST_DIR}/src/verify.c
        ${CMAKE_CURRENT_LIST_DIR}/src/add_scalar.c
        ${CMAKE_CURRENT_LIST_DIR}/src/key_exchange.c
        ${CMAKE_CURRENT_LIST_DIR}/src/key_convert.c
        )

add_library(${PROJECT_NAME} ${ORLP_ED25519_SRC})

if (${ORLP_ED25519_BUILD_DLL} OR BUILD_SHARED_LIBS)
    set_target_properties(${PROJECT_NAME} PROPERTIES
            VERSION ${ORLP_ED25519_VERSION_STRING}
            SOVERSION ${ORLP_ED25519_MAJOR}
            )
endif ()

if (${ORLP_ED25519_PACKAGE})
    set_target_properties(${PROJECT_NAME} PROPERTIES
            ARCHIVE_OUTPUT_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin"
            LIBRARY_OUTPUT_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin"
            RUNTIME_OUTPUT_DIRECTORY "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin"

            ARCHIVE_OUTPUT_DIRECTORY_DEBUG "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin/debug"
            LIBRARY_OUTPUT_DIRECTORY_DEBUG "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin/debug"
            RUNTIME_OUTPUT_DIRECTORY_DEBUG "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin/debug"

            ARCHIVE_OUTPUT_DIRECTORY_RELEASE "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin/release"
            LIBRARY_OUTPUT_DIRECTORY_RELEASE "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin/release"
            RUNTIME_OUTPUT_DIRECTORY_RELEASE "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/bin/release"
            )

    add_custom_command(TARGET ${PROJECT_NAME}
            POST_BUILD COMMAND ${CMAKE_COMMAND}
            -E copy ${CMAKE_CURRENT_LIST_DIR}/src/ed25519.h "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/include/orlp-ed25519/ed25519.h"
            )

    add_custom_command(TARGET ${PROJECT_NAME}
            POST_BUILD COMMAND ${CMAKE_COMMAND}
            -E copy ${CMAKE_CURRENT_LIST_DIR}/license.txt "${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}/license.txt"
            )

    string(TOLOWER "${PROJECT_NAME}-${ORLP_ED25519_VERSION_STRING}-${CMAKE_SYSTEM_NAME}-${CMAKE_HOST_SYSTEM_PROCESSOR}.tar.gz" ${PROJECT_NAME}_OUTPUT_ARCHIVE_FILENAME)

    add_custom_command(TARGET ${PROJECT_NAME}
            POST_BUILD COMMAND ${CMAKE_COMMAND}
            -E tar "czvf" ${${PROJECT_NAME}_OUTPUT_ARCHIVE_FILENAME} ${CMAKE_CURRENT_BINARY_DIR}/${PROJECT_NAME}
            )
endif ()

target_include_directories(${PROJECT_NAME} PUBLIC ${CMAKE_CURRENT_LIST_DIR}/src)
