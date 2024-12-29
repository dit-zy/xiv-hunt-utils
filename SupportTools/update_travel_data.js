const fs = require('node:fs')
const du = require('./data_utils.js')

console.log('parsing the raw aetheryte data...')
const rawAetheryteData = JSON.parse(fs.readFileSync('SupportTools/raw_aetheryte_data.json'))
const aetheryteData = rawAetheryteData
	.filter(aetheryteData => aetheryteData.CanTeleport)
	.reduce((acc, aetheryteData) => {
		acc.getOrDefault(aetheryteData.Zone)[aetheryteData.AetheryteName.toLowerCase()] = aetheryteData.Position
		return acc
	}, {})

console.log('parsing the custom travel nodes...')
const customNodeData = JSON.parse(fs.readFileSync('SupportTools/custom_travel_nodes.json'))

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

const formattedData = {}
mapNames
	.ownKeys()
	.forEach(mapId => {
		const mapName = mapNames[mapId]
		const patch = mapMetadata[mapName].patch;

		const mapData = formattedData.getOrDefault(patch).getOrDefault(mapName, () => ({
			aetherytes: {},
			travelNodes: [],
		}))

		// aetheryte
		if (aetheryteData.contains(mapId)) {
			aetheryteData[mapId]
				.ownKeys()
				.forEach(aetheryteName => {
					mapData.aetherytes[aetheryteName] = rawToMapPos(aetheryteData[mapId][aetheryteName], mapName)
				})
		}

		// travel nodes
		if (customNodeData.contains(patch) && customNodeData[patch].contains(mapName)) {
			mapData.travelNodes = customNodeData[patch][mapName]
		}
	})

// === generate the json ===
// this is done manually, instead of using JSON.stringify(), so the formatting
// of the json can be more deliberately customized for readability.
console.log('generating the hunt data...')
const patchesJson = formattedData.map((patch, patchData) => {

	const mapsJson = patchData.map((mapName, mapData) => {

		let mapAetherytesJson = '{}'
		if (mapData.aetherytes.isEmpty()) {
			console.log(`no aetherytes found for map [${mapName}]`)
		} else {
			const aetheryteNameLength = mapData.aetherytes.ownKeys().max(name => name.length) + 2
			const aetherytesJson = mapData.aetherytes.map((aetheryteName, aetherytePos) =>
				[
					`${`"${aetheryteName}"`.padEnd(aetheryteNameLength)}: {`,
					`"x": ${aetherytePos.x.toFixed(2).padStart(5)},`,
					`"y": ${aetherytePos.y.toFixed(2).padStart(5)},`,
					`"z": ${aetherytePos.z.toFixed(2).padStart(5)}`,
					'}',
				].join(' ')
			).join(',\n')
			mapAetherytesJson = [
				'{',
				du.indentS(aetherytesJson),
				'}'
			].join('\n')
		}

		let travelNodesJson = '[]'
		if (mapData.travelNodes.isEmpty()) {
			console.log(`no travel nodes found for map [${mapName}]`)
		} else {
			const nodesJson = mapData.travelNodes.map(travelNode => {
				return [
					'{',
					du.indentS([
						`"aetheryte": "${travelNode.aetheryte}"`,
						`"distanceModifier": "${travelNode.distanceModifier}"`,
						[
							'"position": {',
							`"x": ${travelNode.position.x},`,
							`"y": ${travelNode.position.y},`,
							`"z": ${travelNode.position.z}`,
							'}',
						].join(' '),
						`"path": "${travelNode.path}"`
					].join(',\n')),
					'}',
				].join('\n')
			}).join(',\n')
			travelNodesJson = [
				'[',
				du.indentS(nodesJson),
				']',
			].join('\n')
		}

		return [
			`"${mapName}": {`,
			du.indentS([
				'"aetherytes": ' + mapAetherytesJson + ',',
				'"travelNodes": ' + travelNodesJson,
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

const formattedTravelDataJson = [
	'{',
	du.indentS(patchesJson),
	'}',
].join('\n')

const travelDataFile = 'XIVHuntUtils/Data/travel_data.json';
console.log(`writing data to: ${travelDataFile}`)
fs.writeFileSync(travelDataFile, formattedTravelDataJson)
