using System.Collections.Immutable;

namespace XivHuntUtils.Tests.Generators;

/**
 * value equatable array. 2 instances will be equal so long as they have equal elements.
 *
 * this is needed by the incremental generator framework in order to cache correctly.
 */
internal class EqArray<T> : IEquatable<EqArray<T>> {
	public IList<T> Values { get; }

	public EqArray(IEnumerable<T> source) {
		Values = source.ToImmutableArray();
	}

	public bool Equals(EqArray<T>? other) {
		if (this == other) return true;
		if (Values == other?.Values) return true;
		if (Values.Count != other!.Values.Count) return false;
		for (var i = 0; i < Values.Count; i++) {
			if (!Equals(Values[i], other.Values[i])) return false;
		}
		return true;
	}
}
