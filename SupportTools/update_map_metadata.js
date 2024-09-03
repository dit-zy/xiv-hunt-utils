const fs = require('node:fs')
const {parse} = require('csv-parse/sync')

function getCsvData(filename) {
	return parse(fs.readFileSync(`/tmp/${filename}.csv`), {columns: true,})
}

const maps = getCsvData('Map')
const placeNames = getCsvData('PlaceName')
const territoryTypes = getCsvData('TerritoryType')
const territoryTypeTransients = getCsvData('TerritoryTypeTransient')
