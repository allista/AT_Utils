#!/bin/bash

cd $(dirname "$0")

../../PyKSPutils/make_mod_release \
-e '*.user' '*.orig' '*.mdb' '*.pdb' \
'GameData/ConfigurableContainers/Parts/*' \
'GameData/000_AT_Utils/ResourceHack.cfg' \
'*/AnimatedConverters.dll' \
-i './ConfigurableContainers/GameData' \
--dll 000_AT_Utils.dll
