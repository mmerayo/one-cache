To use the AppFabric DistributedCaching:
In the Startable configuration file define a section:
	<configSections>
		...
		<section name="dataCacheClient" type="Microsoft.ApplicationServer.Caching.DataCacheClientSection, Microsoft.ApplicationServer.Caching.Core, Version=1.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35" allowLocation="true" allowDefinition="Everywhere" />
		...
	 </configSections>

Now define the Appfabric hosts
	<dataCacheClient>
		<hosts>
			<host name="localhost" cachePort="22233" />
		</hosts>
	</dataCacheClient>

Thats it!!
