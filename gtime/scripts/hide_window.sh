#!/bin/bash

WIN_ID=$(hyprctl clients -j | jq --arg title "gtime" -r '.[] | select(.title == $title) | .address')
hyprctl dispatch movetoworkspacesilent special:T "$WIN_ID"
