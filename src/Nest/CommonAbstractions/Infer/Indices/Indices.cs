using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net.Connection;
using Elasticsearch.Net.Serialization;
using Newtonsoft.Json;

namespace Nest
{
	[JsonConverter(typeof(IndicesJsonConverter))]
	public class Indices : Union<Indices.AllIndices, Indices.ManyIndices>, IUrlParameter
	{
		public class AllIndices { internal AllIndices() { } }
		public static AllIndices All { get; } = new AllIndices();
		public class ManyIndices
		{
			private readonly List<IndexName> _indices = new List<IndexName>();
			public IReadOnlyList<IndexName> Indices => _indices;
			internal ManyIndices(IEnumerable<IndexName> indices) { this._indices.AddRange(indices); }

			public ManyIndices And<T>()
			{
				this._indices.Add(typeof(T));
				return this;
			}
		}

		internal Indices(Indices.AllIndices all) : base(all) { }
		internal Indices(Indices.ManyIndices indices) : base(indices) { }

		public static Indices Single(IndexName index) => new ManyIndices(new[] { index });
		public static Indices Single<T>() => new ManyIndices(new IndexName[] { typeof(T) });
		public static Indices Many(IEnumerable<IndexName> indices) => new ManyIndices(indices);
		public static Indices Many(params IndexName[] indices) => new ManyIndices(indices);
		public static ManyIndices Type<T>() => new ManyIndices(new IndexName[] { typeof(T) });

		public static Indices Parse(string indicesString)
		{
			if (indicesString.IsNullOrEmpty()) throw new Exception("can not parse an empty string to Indices");
			var indices = indicesString.Split(new[] { "," }, StringSplitOptions.RemoveEmptyEntries);
			if (indices.Contains("_all")) return Indices.All;
			return Many(indices.Select(i => (IndexName)i));
		}

		public static implicit operator Indices(string indicesString) => Parse(indicesString);
		public static implicit operator Indices(AllIndices all) => new Indices(all);
		public static implicit operator Indices(ManyIndices many) => new Indices(many);
		public static implicit operator Indices(IndexName[] many) => Indices.Many(many);
		public static implicit operator Indices(IndexName index) => Indices.Single(index);
		public static implicit operator Indices(Type type) => Indices.Single(type);

		string IUrlParameter.GetString(IConnectionConfigurationValues settings)
		{
			return this.Match(
				all => "_all",
				many =>
				{
					var nestSettings = settings as IConnectionSettingsValues;
					if (nestSettings == null)
						throw new Exception("Tried to pass field name on querysting but it could not be resolved because no nest settings are available");
					var infer = new ElasticInferrer(nestSettings);
					var indices = this.Item2.Indices.Select(i => infer.IndexName(i));
					return string.Join(",", indices);
				}
			);

		}
	}
}