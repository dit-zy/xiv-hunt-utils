const fs = require('node:fs')

const rawSpawnData = JSON.parse(fs.readFileSync('SupportTools/raw_spawn_data.json'))
const zOffsets = JSON.parse(fs.readFileSync('SupportTools/z_offsets.json'))

const patches = [
	{name: 'arr', maps: [], patchChangeMap: 'coerthas western highlands'},
	{name: 'hw', maps: [], patchChangeMap: 'the fringes'},
	{name: 'sb', maps: [], patchChangeMap: 'lakeland'},
	{name: 'shb', maps: [], patchChangeMap: 'labyrinthos'},
	{name: 'ew', maps: [], patchChangeMap: 'urqopacha'},
	{name: 'dt', maps: [], patchChangeMap: ''},
]
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
		y: (rawZ - zOffsets[map]) / 100,
		z: 2048 / scale + rawY / 50 + 1,
	}
}

function getOrDefault(dict, key, defaultValueSupplier) {
	return dict.hasOwnProperty(key) ? dict[key] : defaultValueSupplier();
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

Array.prototype['uniqSorted'] = function () {
	return [...new Set(this)]
}

// === parse the raw data ===
let patchIx = 0
rawSpawnData.forEach((spawn, i) => {
	const mapName = spawn.ZoneName.toLowerCase()
	const markNames = spawn.MobNames.map(mobName => mobName.toLowerCase())
	const rawSpawnPos = spawn.ActualSpawnPosition
	const spawnId = i

	if (patches[patchIx].patchChangeMap == mapName) {
		patchIx += 1
	}

	// patch
	patches[patchIx].maps.push(mapName)

	// map marks
	markNames.forEach(markName =>
		getOrDefault(maps, mapName, () => new Set()).add(markName)
	)

	// mark spawns
	markNames.forEach(markName =>
		getOrDefault(marks, markName, () => []).push(spawnId)
	)

	// spawn points
	spawns.push(rawToMapPos(rawSpawnPos, mapName, patches[patchIx]))
})

// === generate the json ===
// this is done manually, instead of using JSON.stringify(), so the formatting
// of the json can be more deliberately customized for readability.
const patchesJson = patches.map(patch => {
	
	const mapsJson = patch.maps.map(mapName => {
		
		const mapMarks = maps[mapName].uniqSorted()
		const mapSpawns = mapMarks.flatMap(markName => marks[markName]).uniqSorted()
		
		const mapMarksJson = mapMarks.map(markName =>
			`"${markName}": { "spawns": [${marks[markName].join(',')}]`
		).join(',\n')
		const mapSpawnsJson = mapSpawns.map(spawnId => {
			return [
				`{ "id": ${spawnId}, "position": {`,
				`"x": ${spawns[spawnId].x.tofixed(2).padStart(5)}, `,
				`"y": ${spawns[spawnId].y.tofixed(2).padStart(5)}, `,
				`"z": ${spawns[spawnId].z.tofixed(2).padStart(5)}`,
				`} }`,
			].join()
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
		`"${patch.name}": {`,
		indentS(mapsJson),
		'}',
	].join('\n')
}).join(',\n')

const formattedHuntDataJson = [
	'{',
	indentS(patchesJson),
	'}',
].join('\n')

fs.writeFileSync('XIVHuntUtils/Data/hunt_data.json', formattedHuntDataJson)
