const fs = require('node:fs')
require('./data_utils.js')

console.log('parsing the raw spawn data...')
const rawSpawnData = JSON.parse(fs.readFileSync('SupportTools/raw_spawn_data.json'))
console.log('parsing the map metadata...')
const rawMapMetadata = JSON.parse(fs.readFileSync('SupportTools/map_data.json'))
const mapMetadata = rawMapMetadata
	.ownKeys()
	.reduce((acc, patch) => {
		rawMapMetadata[patch]
			.ownKeys()
			.forEach(mapName => {
				acc[mapName] = {
					patch: patch,
					zOffset: rawMapMetadata[patch][mapName].zOffset,
				}
			})
		return acc
	}, {})

const patches = {}
const maps = {}
const marks = {}
const spawns = []

function rawToMapPos(pos, map, patch) {
	const rawX = pos.X
	const rawY = pos.Y
	const rawZ = pos.Z
	const scale = patch == "hw" ? 95 : 100
	return {
		x: 2048 / scale + rawX / 50 + 1,
		y: 2048 / scale + rawZ / 50 + 1,
		z: (rawY - mapMetadata[map].zOffset) / 100,
	}
}

function indent(content, numIndents = 1) {
	let contentLines
	if (Array.isArray(content)) {
		contentLines = content.flatMap(str => str.split('\n'))
	} else {
		contentLines = content.split('\n')
	}
	return contentLines.map(line => '\t'.repeat(numIndents) + line)
}

function indentS(content, numIndents = 1) {
	return indent(content, numIndents).join('\n')
}

// === parse the raw data ===
console.log('consuming the raw data...')
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
	spawns.push(rawToMapPos(rawSpawnPos, mapName, patch))
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
			`${`"${markName}"`.padEnd(markKeyLength)} : { "spawns": [${marks[markName][mapName].join(',')}] }`
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
			indentS([
				'"marks": {',
				indentS(mapMarksJson),
				'},',
				'"spawns": [',
				indentS(mapSpawnsJson),
				']',
			]),
			'}',
		].join('\n')

	}).join(',\n')

	return [
		`"${patch}": {`,
		indentS(mapsJson),
		'}',
	].join('\n')
}).join(',\n')

const formattedHuntDataJson = [
	'{',
	indentS(patchesJson),
	'}',
].join('\n')

const huntDataFile = 'XIVHuntUtils/Data/hunt_data.json';
console.log(`writing data to: ${huntDataFile}`)
fs.writeFileSync(huntDataFile, formattedHuntDataJson)
