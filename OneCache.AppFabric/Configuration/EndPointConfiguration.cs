namespace OneCache.AppFabric.Configuration
{
	public sealed class EndPointConfiguration
	{
		public EndPointConfiguration(string hostName, int port)
		{
			Port = port;
			HostName = hostName;
		}

		public string HostName { get; private set; }
		public int Port { get; private set; }
	}
}