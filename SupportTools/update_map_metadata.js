const fs = require('node:fs')
const {parse} = require('csv-parse/sync')
require('./data_utils.js')

function getCsvData(filename) {
	const rows = parse(fs.readFileSync(`/tmp/${filename}.csv`), {columns: true,})
	console.log(` \n=== ${filename} // ${rows.length} rows found`)
	return rows
}

const maps = getCsvData('Map').map(mapEntry => ({
	rowId: parseInt(mapEntry['#']),
	xOffset: parseFloat(mapEntry['Offset{X}']).toFixed(2),
	yOffset: parseFloat(mapEntry['Offset{Y}']).toFixed(2),
})).arrayify()

const placeNames = getCsvData('PlaceName').map(placeName => ({
	rowId: parseInt(placeName['#']),
	name: placeName['Name'].toLowerCase(),
})).arrayify()

const territoryTypes = getCsvData('TerritoryType').map(territoryType => ({
	rowId: parseInt(territoryType['#']),
	mapId: parseInt(territoryType['Map']),
	placeNameId: parseInt(territoryType['PlaceName']),
	intendedUse: parseInt(territoryType['TerritoryIntendedUse']),
	patch: (parseInt(territoryType['ExVersion']) + 2).toPatchName(),
})).arrayify()

const territoryTypeTransients = getCsvData('TerritoryTypeTransient').map(transient => ({
	rowId: parseInt(transient['#']),
	zOffset: parseFloat(transient['Offset{Z}']).toFixed(2),
})).arrayify()

console.log(' \nconstructing map metadata...')
const metadata = {}
territoryTypes
	// idk what `intendedUse` is, but it filters out most non-hunt territories.
	// however, we need intendedUse 0 for city territories (e.g. limsa)
	.filter(territory => territory.intendedUse == 0 || territory.intendedUse == 1)
	.filter(territory => territory.placeNameId != 0)
	.forEach(territory => {
		const territoryName = placeNames[territory.placeNameId].name;
		metadata.getOrDefault(territory.patch)[territoryName] = {
			id: territory.rowId,
			xOffset: maps[territory.mapId].xOffset,
			yOffset: maps[territory.mapId].yOffset,
			zOffset: territoryTypeTransients[territory.rowId].zOffset,
		}
	})

const metadataFile = 'SupportTools/map_data.json';
console.log(`writing to: ${metadataFile}`)
fs.writeFileSync(metadataFile, JSON.stringify(metadata, null, '\t'))
