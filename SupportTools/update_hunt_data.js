const fs = require('node:fs')
require('./data_utils.js')

const MAPS_WITHOUT_AETHERYTES = new Set([
	'the dravanian hinterlands',
])

console.log('parsing the raw spawn data...')
const rawSpawnData = JSON.parse(fs.readFileSync('SupportTools/raw_spawn_data.json'))

console.log('parsing the raw aetheryte data...')
const rawAetheryteData = JSON.parse(fs.readFileSync('SupportTools/raw_aetheryte_data.json'))
const aetheryteData = rawAetheryteData
	.filter(aetheryteData => aetheryteData.CanTeleport)
	.reduce((acc, aetheryteData) => {
		acc.getOrDefault(aetheryteData.Zone)[aetheryteData.AetheryteName.toLowerCase()] = aetheryteData.Position
		return acc
	}, {})
// console.log(JSON.stringify(aetheryteData))

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
// console.log(JSON.stringify(mapMetadata))
// console.log(JSON.stringify(mapNames))

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

const patches = {}
const maps = {}
const marks = {}
const spawns = []
const aetherytes = {}
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

// aetherytes
aetheryteData
	.ownKeys()
	.filter(mapId => mapNames.hasOwnProperty(mapId))
	.forEach(mapId => {
	const mapName = mapNames[mapId]
	aetheryteData[mapId]
		.ownKeys()
		.forEach(aetheryteName => {
			aetherytes.getOrDefault(mapName)[aetheryteName] = rawToMapPos(aetheryteData[mapId][aetheryteName], mapName)
		})
})
// console.log(JSON.stringify(aetherytes))

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

		let mapAetherytesJson = ''
		if (!aetherytes.hasOwnProperty(mapName)) {
			if (!MAPS_WITHOUT_AETHERYTES.has(mapName)) {
				throw new Error(`map [${mapName}] has no aetherytes, but is not in the maps-without-aetherytes allow list`)
			}
			console.log(`no aetherytes found for map [${mapName}]`)
			mapAetherytesJson = '"aetherytes": {},'
		} else {
			const aetheryteNameLength = aetherytes[mapName].ownKeys().max(name => name.length) + 2
			const aetherytesJson = aetherytes[mapName]
				.ownKeys()
				.map(aetheryteName => {
					const aetherytePos = aetherytes[mapName][aetheryteName];
					return [
						`${`"${aetheryteName}"`.padEnd(aetheryteNameLength)}: {`,
						`"x": ${aetherytePos.x.toFixed(2).padStart(5)}, `,
						`"y": ${aetherytePos.y.toFixed(2).padStart(5)}, `,
						`"z": ${aetherytePos.z.toFixed(2).padStart(5)}`,
						`}`,
					].join('')
				}).join(',\n')
			mapAetherytesJson = [
				'"aetherytes": {',
				indentS(aetherytesJson),
				'},'
				].join('\n')
		}

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
				mapAetherytesJson,
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
