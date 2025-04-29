using System;
using System.Collections.Generic;
using com.ktgame.config.core;
using UnityEngine;

namespace com.ktgame.services.remote_config.provider
{
	public class FirebaseConfigProvider : MonoBehaviour, IConfigProvider
	{
		public event Action OnFetchSuccess;
		public event Action OnFetchError;
		public event Action OnSetDefaultComplete;
		
		private IDictionary<string, object> _defaults;
		
		public FirebaseConfigProvider(ConfigPlayerPrefCache configPlayerPrefCache) { }

		public IConfigValue GetValue(string id)
		{
			return GetValue(id);
		}

		public void SetDefaultValues(IConfigBlueprint config)
		{
			_defaults = config.Export();
			foreach (var kvp in _defaults)
			{
				switch (kvp.Value)
				{
					case int intValue:
						PlayerPrefs.SetInt(kvp.Key, intValue);
						break;
					case float floatValue:
						PlayerPrefs.SetFloat(kvp.Key, floatValue);
						break;
					case string stringValue:
						PlayerPrefs.SetString(kvp.Key, stringValue);
						break;
					case bool boolValue:
						PlayerPrefs.SetInt(kvp.Key, boolValue ? 1 : 0);
						break;
				}
			}
		}

		public void Fetch()
		{
			OnFetchSuccess?.Invoke();
		}
	}
}
