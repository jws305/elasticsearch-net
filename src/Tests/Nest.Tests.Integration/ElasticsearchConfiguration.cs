﻿using System;
using System.Diagnostics;
using Elasticsearch.Net.Connection.Thrift;
using Elasticsearch.Net;
using System.Net;
using Newtonsoft.Json;

namespace Nest.Tests.Integration
{
	public static class ElasticsearchConfiguration
	{
		public static readonly string DefaultIndex = Test.Default.DefaultIndex + "-" + Process.GetCurrentProcess().Id.ToString();

		private static string _currentVersion;
		public static string CurrentVersion
		{
			get
			{
				if (string.IsNullOrEmpty(_currentVersion))
					_currentVersion = GetCurrentVersion();

				return _currentVersion;
			}
		}

		public static Uri CreateBaseUri(int? port = null)
		{
			var host = Test.Default.Host;
			if (port == null && Process.GetProcessesByName("fiddler").HasAny())
				host = "ipv4.fiddler";

			var uri = new UriBuilder("http", host, port.GetValueOrDefault(9200)).Uri;
			return uri;
		}
		public static ConnectionSettings Settings(int? port = null, Uri hostOverride = null)
		{

			return new ConnectionSettings(hostOverride ?? CreateBaseUri(port), ElasticsearchConfiguration.DefaultIndex)
				.SetMaximumAsyncConnections(Test.Default.MaximumAsyncConnections)
				.UsePrettyResponses()
				.ExposeRawResponse();
		}

		public static readonly ElasticClient Client = new ElasticClient(Settings());
		public static readonly ElasticClient ClientNoRawResponse = new ElasticClient(Settings().ExposeRawResponse(false));
		public static readonly ElasticClient ClientThatTrows = new ElasticClient(Settings().ThrowOnElasticsearchServerExceptions());
		public static readonly ElasticClient ThriftClient = new ElasticClient(Settings(9500), new ThriftConnection(Settings(9500)));
		public static string NewUniqueIndexName()
		{
			return DefaultIndex + "_" + Guid.NewGuid().ToString();
		}

		public static string GetCurrentVersion()
		{
			var uri = ElasticsearchConfiguration.CreateBaseUri();
			var json = new WebClient().DownloadString(uri);
			var response = JsonConvert.DeserializeObject<dynamic>(json);
			return  response.version.number;
		}
	}
}