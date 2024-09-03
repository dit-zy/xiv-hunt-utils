#!/usr/bin/env bash

set -xeuo pipefail

function get_data_csv() {
  csv="$1"
  url="https://raw.githubusercontent.com/xivapi/ffxiv-datamining/master/csv/${csv}.csv"
  echo "getting: $url"
  curl -o "$url" | tail -n +2 >> "/tmp/$csv"
}

get_data_csv Map
get_data_csv PlaceName
get_data_csv TerritoryType
get_data_csv TerritoryTypeTransient
