#!/bin/bash

CURRENT_WS=$(hyprctl activeworkspace -j | jq -r '.id')
WIN_ID=$(hyprctl clients -j | jq --arg title "gtime" -r '.[] | select(.title == $title) | .address')

hyprctl dispatch focuswindow address:$WIN_ID
hyprctl dispatch movetoworkspace $CURRENT_WS
