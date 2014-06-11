﻿namespace Common.DistributedCaching
{
	internal interface ICacheConfiguration<out TConfiguration>
	{
		TConfiguration Object { get; }
		bool ConnectOnStartUp { get; }
	}
}