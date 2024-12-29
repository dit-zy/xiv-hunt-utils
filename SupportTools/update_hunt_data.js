const fs = require('node:fs')
const du = require('./data_utils.js')

console.log('parsing the raw spawn data...')
const rawSpawnData = JSON.parse(fs.readFileSync('SupportTools/raw_spawn_data.json'))

console.log('parsing the map metadata...')
const rawMapMetadata = JSON.parse(fs.readFileSync('SupportTools/map_data.json'))
const {mapMetadata, mapNames} = rawMapMetadata
	.ownKeys()
	.reduce((acc, patch) => {
		rawMapMetadata[patch]
			.ownKeys()
			.forEach(mapName => {
				acc.mapMetadata[mapName] = {
					patch: patch,
					zOffset: rawMapMetadata[patch][mapName].zOffset,
				}
				acc.mapNames[rawMapMetadata[patch][mapName].id] = mapName
			})
		return acc
	}, {mapMetadata: {}, mapNames: {}})

function rawToMapPos(pos, map) {
	const rawX = pos.X
	const rawY = pos.Y
	const rawZ = pos.Z
	const scale = mapMetadata[map].patch == "hw" ? 95 : 100
	return {
		x: 2048 / scale + rawX / 50 + 1,
		y: 2048 / scale + rawZ / 50 + 1,
		z: (rawY - mapMetadata[map].zOffset) / 100,
	}
}

// === parse the raw data ===
console.log('consuming the raw data...')

const patches = {}
const maps = {}
const marks = {}
const spawns = []
rawSpawnData.forEach((spawn, i) => {
	const mapName = spawn.ZoneName.toLowerCase()
	const markNames = spawn.MobNames.map(mobName => mobName.toLowerCase())
	const rawSpawnPos = spawn.hasOwnProperty('ActualSpawnPosition') ? spawn.ActualSpawnPosition : spawn.Position
	const spawnId = i
	const patch = mapMetadata[mapName].patch;

	// patch
	patches.getOrDefault(patch, () => new Set()).add(mapName)

	// map marks
	markNames.forEach(markName =>
		maps.getOrDefault(mapName, () => new Set()).add(markName)
	)

	// mark spawns
	markNames.forEach(markName =>
		marks.getOrDefault(markName).getOrDefault(mapName, () => []).push(spawnId)
	)

	// spawn points
	spawns.push(rawToMapPos(rawSpawnPos, mapName))
})

// === generate the json ===
// this is done manually, instead of using JSON.stringify(), so the formatting
// of the json can be more deliberately customized for readability.
console.log('generating the hunt data...')
const patchesJson = patches.ownKeys().map(patch => {

	const mapsJson = patches[patch].map(mapName => {

		const mapMarks = [...maps[mapName]].sort()
		const mapSpawns = mapMarks.flatMap(markName => marks[markName][mapName]).uniqSorted((a, b) => a - b)

		const markKeyLength = mapMarks.max(mark => mark.length) + 2
		const mapMarksJson = mapMarks.map(markName =>
			`${`"${markName}"`.padEnd(markKeyLength)}: { "spawns": [${marks[markName][mapName].join(',')}] }`
		).join(',\n')

		const mapSpawnsJson = mapSpawns.map(spawnId => {
			const spawn = spawns[spawnId];
			return [
				`{ "id": ${spawnId}, "position": {`,
				`"x": ${spawn.x.toFixed(2).padStart(5)}, `,
				`"y": ${spawn.y.toFixed(2).padStart(5)}, `,
				`"z": ${spawn.z.toFixed(2).padStart(5)}`,
				`} }`,
			].join('')
		}).join(',\n')

		return [
			`"${mapName}": {`,
			du.indentS([
				'"marks": {',
				du.indentS(mapMarksJson),
				'},',
				'"spawns": [',
				du.indentS(mapSpawnsJson),
				']',
			]),
			'}',
		].join('\n')

	}).join(',\n')

	return [
		`"${patch}": {`,
		du.indentS(mapsJson),
		'}',
	].join('\n')
}).join(',\n')

const formattedHuntDataJson = [
	'{',
	du.indentS(patchesJson),
	'}',
].join('\n')

const huntDataFile = 'XIVHuntUtils/Data/hunt_data.json';
console.log(`writing data to: ${huntDataFile}`)
fs.writeFileSync(huntDataFile, formattedHuntDataJson)
