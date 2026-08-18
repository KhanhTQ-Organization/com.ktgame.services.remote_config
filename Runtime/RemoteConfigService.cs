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
		public int Priority => 1;
		public bool Initialized { get; set; }

		public event Action OnFetchSuccess;
		public event Action OnFetchError;
		public IConfigBlueprint ConfigBlueprint { get; private set; }
		public IConfigProvider ConfigProvider { get; private set; }

		private RemoteConfigServiceSettings _settings;
		public bool IsFetchSuccess { get; private set; }

		public async UniTask OnInitialize(IArchitecture architecture)
		{
			_settings = RemoteConfigServiceSettings.Instance;
			ConfigBlueprint = new ConfigBlueprint();
			foreach (var configData in _settings.Configs)
			{
				switch (configData.Type)
				{
					case ValueType.Int:
						if (int.TryParse(configData.DefaultValue, out int intVal))
							ConfigBlueprint.SetInt(configData.Name, intVal);
						else
						{
							Debug.LogWarning($"[RemoteConfig] Invalid Int DefaultValue for '{configData.Name}'. Falling back to 0.");
							ConfigBlueprint.SetInt(configData.Name, 0);
						}
						break;
					case ValueType.Float:
						if (float.TryParse(configData.DefaultValue, out float floatVal))
							ConfigBlueprint.SetFloat(configData.Name, floatVal);
						else
						{
							Debug.LogWarning($"[RemoteConfig] Invalid Float DefaultValue for '{configData.Name}'. Falling back to 0f.");
							ConfigBlueprint.SetFloat(configData.Name, 0f);
						}
						break;
					case ValueType.String:
						ConfigBlueprint.SetString(configData.Name, configData.DefaultValue ?? "");
						break;
					case ValueType.Boolean:
						if (bool.TryParse(configData.DefaultValue, out bool boolVal))
							ConfigBlueprint.SetBool(configData.Name, boolVal);
						else
						{
							Debug.LogWarning($"[RemoteConfig] Invalid Boolean DefaultValue for '{configData.Name}'. Falling back to false.");
							ConfigBlueprint.SetBool(configData.Name, false);
						}
						break;
				}
			}

#if FIREBASE_REMOTE_CONFIG
            var firebaseService = architecture.GetService<IFirebaseService>();
            await UniTask.WaitUntil(() => firebaseService.Initialized);
            ConfigProvider = new FirebaseConfigProvider();
            ConfigProvider.OnFetchSuccess += () =>
            {
                IsFetchSuccess = true;
                OnFetchSuccess?.Invoke();
            };
            ConfigProvider.OnFetchError += () =>
            {
                IsFetchSuccess = false;
                OnFetchError?.Invoke();
            };
            ConfigProvider.OnSetDefaultComplete += OnSetDefaultComplete;
            ConfigProvider.SetDefaultValues(ConfigBlueprint);
#else
			ConfigProvider = new NullConfigProvider();
			ConfigProvider.OnFetchSuccess += () =>
			{
				IsFetchSuccess = true;
				OnFetchSuccess?.Invoke();
			};
			ConfigProvider.OnFetchError += () =>
			{
				IsFetchSuccess = false;
				OnFetchError?.Invoke();
			};
			ConfigProvider.OnSetDefaultComplete += OnSetDefaultComplete;
			ConfigProvider.SetDefaultValues(ConfigBlueprint);
#endif
		}

		public void Fetch()
		{
			IsFetchSuccess = false;
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
