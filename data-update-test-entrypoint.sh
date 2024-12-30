#!/bin/bash

set -eu

npm install csv-parse
SupportTools/get-excel-sheets.sh
node SupportTools/update_map_metadata.js
node SupportTools/update_hunt_data.js
node SupportTools/update_travel_data.js
