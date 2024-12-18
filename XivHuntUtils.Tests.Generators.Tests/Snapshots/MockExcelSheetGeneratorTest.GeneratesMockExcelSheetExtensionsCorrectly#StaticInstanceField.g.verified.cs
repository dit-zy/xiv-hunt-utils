﻿//HintName: StaticInstanceField.g.cs
// <autogenerated/>
using CSharpFunctionalExtensions;
using DitzyExtensions.Functional;

namespace XivHuntUtils.Tests.Generators;

public class StaticInstanceField<T> {
	private readonly IDictionary<int, T> _fieldValues = new Dictionary<int, T>();

	public Maybe<T> Get(object instance) => _fieldValues.MaybeGet(instance.GetHashCode());
	
	public T GetOrDefault(object instance, T defaultValue = default) {
		var existingValue = Get(instance);
		var value = existingValue.GetValueOrDefault(defaultValue);
		if (existingValue.HasNoValue) Set(instance, value);
		return value;
	}
	
	public void Set(object instance, T value) => _fieldValues[instance.GetHashCode()] = value;
}