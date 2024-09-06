#!/usr/bin/env bash

set -euo pipefail

function get_data_csv() {
  csv="$1"
  url="https://raw.githubusercontent.com/xivapi/ffxiv-datamining/master/csv/${csv}.csv"
  echo -e " \n=== $csv ==="
  echo "retrieving file from: $url"
  curl "$url" > /tmp/q
  echo "head:"
  head -n 5 /tmp/q
  echo "extracting header row..."
  head -n 2 /tmp/q | tail -n +2 > "/tmp/${csv}.csv"
  echo "extracting data..."
  tail -n +4 /tmp/q >> "/tmp/${csv}.csv"
}

get_data_csv Map
get_data_csv PlaceName
get_data_csv TerritoryType
get_data_csv TerritoryTypeTransient
