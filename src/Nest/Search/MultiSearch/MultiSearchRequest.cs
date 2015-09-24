﻿using System;
using System.Collections.Generic;
using System.Linq;
using Elasticsearch.Net;
using Newtonsoft.Json;

namespace Nest
{
	public partial interface IMultiSearchRequest
	{

		[JsonConverter(typeof(VerbatimDictionaryKeysJsonConverter))]
		IDictionary<string, ISearchRequest> Operations { get; set; }
	}

	public partial class MultiSearchRequest
	{
		public IDictionary<string, ISearchRequest> Operations { get; set; }
	}

	[DescriptorFor("Msearch")]
	public partial class MultiSearchDescriptor
	{
		private IMultiSearchRequest Self => this;

		internal IDictionary<string, ISearchRequest> _operations = new Dictionary<string, ISearchRequest>();

		IDictionary<string, ISearchRequest> IMultiSearchRequest.Operations
		{
			get { return _operations; }
			set { _operations = value; }
		}

		public MultiSearchDescriptor Search<T>(string name, Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector) where T : class
		{
			name.ThrowIfNull("name");
			searchSelector.ThrowIfNull("searchSelector");
			var descriptor = searchSelector(new SearchDescriptor<T>());
			if (descriptor == null)
				return this;
			this._operations.Add(name, descriptor);
			return this;
		}

		public MultiSearchDescriptor Search<T>(Func<SearchDescriptor<T>, SearchDescriptor<T>> searchSelector) where T : class
		{
			return this.Search(Guid.NewGuid().ToString(), searchSelector);
		}
	}
}
