const patchNames = ['arr', 'hw', 'sb', 'shb', 'ew', 'dt']
Number.prototype.toPatchName = function () {
	return patchNames[Math.trunc(this - 2)]
}

Array.prototype.max = function (keyFn = (x) => x) {
	if (keyFn == null) {
		return Math.max.apply(null, this)
	}
	return Math.max.apply(null, this.map(keyFn))
}

Array.prototype.arrayify = function () {
	const maxRowId = this.max(entry => entry.rowId);
	console.log(`max rowId: ${maxRowId}`)
	const res = new Array(maxRowId + 1)
	this.forEach(entry => res[entry.rowId] = entry)
	console.log('head entries:')
	res.slice(0, 5).forEach(entry => console.log(JSON.stringify(entry)))
	return res
}

Array.prototype.uniq = function () {
	return [...new Set(this)]
}

Array.prototype.uniqSorted = function (compareFn) {
	return this.uniq().toSorted(compareFn)
}

Set.prototype.map = function(callbackfn) {
	return [...this].map(callbackfn)
}

Set.prototype.flatMap = function(callback) {
	return [...this].flatMap(callback)
}

Object.prototype.getOrDefault = function (key, defaultValueSupplier = () => ({})) {
	if (!this.hasOwnProperty(key)) {
		this[key] = defaultValueSupplier();
	}
	return this[key]
}

Object.prototype.ownKeys = function() {
	return Object.getOwnPropertyNames(this)
}
