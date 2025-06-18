using System;
using com.ktgame.config.core;
using com.ktgame.core;
using com.ktgame.services.firebase;

#if FIREBASE_REMOTE_CONFIG
using com.ktgame.services.remote_config.provider;
#endif

using Cysharp.Threading.Tasks;
using UnityEngine;

namespace com.ktgame.services.remote_config
{
	[Service(typeof(IRemoteConfigService))]
	public class RemoteConfigService : MonoBehaviour, IRemoteConfigService
	{
		public int Priority { get; }
		public bool Initialized { get; set; }

		public event Action OnFetchSuccess;
		public event Action OnFetchError;
		public IConfigBlueprint ConfigBlueprint { get; private set; }
		public IConfigProvider ConfigProvider { get; private set; }

		private RemoteConfigServiceSettings _settings;

		public async UniTask OnInitialize(IArchitecture architecture)
		{
			_settings = RemoteConfigServiceSettings.Instance;
			ConfigBlueprint = new ConfigBlueprint();
			foreach (var configData in _settings.Configs)
			{
				switch (configData.Type)
				{
					case ValueType.Int:
						ConfigBlueprint.SetInt(configData.Name, int.Parse(configData.DefaultValue));
						break;
					case ValueType.Float:
						ConfigBlueprint.SetFloat(configData.Name, float.Parse(configData.DefaultValue));
						break;
					case ValueType.String:
						ConfigBlueprint.SetString(configData.Name, configData.DefaultValue);
						break;
					case ValueType.Boolean:
						ConfigBlueprint.SetBool(configData.Name, bool.Parse(configData.DefaultValue));
						break;
				}

				break;
			}

#if FIREBASE_REMOTE_CONFIG
			var firebaseService = architecture.GetService<IFirebaseService>();
			await UniTask.WaitUntil(() => firebaseService.Initialized);
			ConfigProvider = new FirebaseConfigProvider(new ConfigPlayerPrefCache());
			ConfigProvider.OnFetchSuccess += () => OnFetchSuccess?.Invoke();
			ConfigProvider.OnFetchError += () => OnFetchError?.Invoke();
			ConfigProvider.OnSetDefaultComplete += OnSetDefaultComplete;
			ConfigProvider.SetDefaultValues(ConfigBlueprint);
#else
			ConfigProvider = new NullConfigProvider();
			ConfigProvider.OnFetchSuccess += OnFetchSuccess;
			ConfigProvider.OnFetchError += OnFetchError;
			ConfigProvider.OnSetDefaultComplete += OnSetDefaultComplete;
			ConfigProvider.SetDefaultValues(ConfigBlueprint);
#endif
		}

		public void Fetch()
		{
			ConfigProvider.Fetch();
		}

		public IConfigValue GetValue(string configKey)
		{
			return ConfigProvider.GetValue(configKey);
		}

		private void OnSetDefaultComplete()
		{
			Initialized = true;

			if (_settings.AutoFetching)
			{
				ConfigProvider?.Fetch();
			}
		}
	}
}
