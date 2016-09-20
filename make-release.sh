#!/bin/bash

cd $(dirname "$0")

../../PyKSPutils/make_mod_release -e '*/AT_Utils.user' '*.orig' '*.mdb' '*/VolumeConfigs.user' -i './ConfigurableContainers/GameData'
