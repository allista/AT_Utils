#!/bin/bash

cd $(dirname "$0")

../../PyKSPutils/make_mod_release \
-e '*.user' '*.orig' '*.mdb' \
'GameData/ConfigurableContainers/Parts/*' \
'*/AnimatedConverters.dll' \
-i './ConfigurableContainers/GameData'
