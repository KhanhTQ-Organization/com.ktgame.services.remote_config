using com.ktgame.config.core;

#if FIREBASE_REMOTE_CONFIG
using Firebase.RemoteConfig;

namespace com.ktgame.services.remote_config.provider
{
    public readonly struct FirebaseConfigValue : IConfigValue
    {
        private readonly ConfigValue _value;

        public FirebaseConfigValue(string id)
        {
              _value = FirebaseRemoteConfig.DefaultInstance.GetValue(id);
        }

        #region Implement IConfigValue

        public float Float => (float)_value.DoubleValue;

        public double Double => _value.DoubleValue;

        public int Int => (int)_value.DoubleValue;

        public long Long => _value.LongValue;

        public string String => _value.StringValue;

        public bool Boolean => _value.BooleanValue;

        #endregion
    }
}
#endif