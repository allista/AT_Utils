#!/bin/bash

CWD=$(dirname "$0")
cd "${CWD}" || exit

make_mod_release \
    -e '*/config.xml' '*.user' '*.orig' '*.mdb' '*.pdb' \
    '*/System.*.dll' '*/Mono.*.dll' '*/Unity*.dll' \
    'GameData/000_AT_Utils/ResourceHack.cfg' \
    '*/AnimatedConverters.dll' \
    --dll 000_AT_Utils.dll
