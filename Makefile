CC ?= cc
CFLAGS ?= -O2 -pipe
LDFLAGS ?= -s
DESTDIR ?=
PREFIX ?= /usr
LIB_DIR ?= $(PREFIX)/lib
INC_DIR ?= $(PREFIX)/include
DOC_DIR ?= $(PREFIX)/share/doc

CFLAGS += -std=gnu99 -Wall -pedantic
LDFLAGS += -L.

SRCS = $(wildcard src/*.c)
OBJS = $(SRCS:.c=.o)

LIB = libed25519.a

all: $(LIB) test

%.o: %.c
	$(CC) -c -o $@ $< $(CFLAGS)

$(LIB): $(OBJS)
	ar rcs $@ $^

test: test.o $(LIB)
	$(CC) -o $@ $^ $(LDFLAGS) -led25519

install: $(LIB)
	install -D -m 644 $(LIB) $(DESTDIR)/$(LIB_DIR)/$(LIB)
	install -D -m 644 src/ed25519.h $(DESTDIR)/$(INC_DIR)/ed25519.h
	install -D -m 644 readme.md $(DESTDIR)/$(DOC_DIR)/ed25519/readme.md

clean:
	rm -f test test.o $(LIB) $(OBJS)
