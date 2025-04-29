using System;
using com.ktgame.core;
using com.ktgame.config.core;

namespace com.ktgame.services.remote_config
{
	public interface IRemoteConfigService : IService, IInitializable
	{
		event Action OnFetchSuccess;

		event Action OnFetchError;

		IConfigBlueprint ConfigBlueprint { get; }
        
		IConfigProvider ConfigProvider { get; }
        
		void Fetch();
        
		IConfigValue GetValue(string configKey);
	}
}
