#ifndef PROTO_TYPES_H
#define PROTO_TYPES_H

#include <stdint.h>
#include <stddef.h>

struct FieldRef {
    int wireType;
    uint64_t varintVal;
    const uint8_t* bytesPtr;
    size_t bytesLen;
};

#endif
