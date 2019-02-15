using System;
using System.IO;

namespace KellermanSoftware.Common
{
    public class SettingsLogic
    {
        private SettingsConfig _config;
        private IniReaderWriter _ini = new IniReaderWriter();
        private string _directory;
        RC4Encryption _crypt = new RC4Encryption();

        public SettingsLogic(SettingsConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("config");

            if (String.IsNullOrEmpty(config.CompanyName))
                throw new ArgumentNullException("config.CompanyName");

            if (String.IsNullOrEmpty(config.ProjectName))
                throw new ArgumentNullException("config.ProjectName");

            _config = config;
        }

        public string GetSetting(string section, string name)
        {
            string filePath = GetSettingFilename(section);
            string setting= _ini.GetSetting(section, name, filePath);

            if (String.IsNullOrEmpty(setting) || String.IsNullOrEmpty(_config.EncryptionPassword))
                return setting;

            return _crypt.Decrypt(setting, _config.EncryptionPassword);
        }

        public void SaveSetting(string section, string name, string value)
        {
            value = value ?? string.Empty;
            string filePath = GetSettingFilename(section);

            if (!String.IsNullOrEmpty(value) && !String.IsNullOrEmpty(_config.EncryptionPassword))
                value = _crypt.Encrypt(value, _config.EncryptionPassword);

            _ini.SaveSetting(section, name, value, filePath);
        }

        private void Setup()
        {
            _directory = Path.Combine(FileUtil.GetAppDataDirectory(), FileUtil.FilterFileName(_config.CompanyName));
            _directory = Path.Combine(_directory, FileUtil.FilterFileName(_config.ProjectName));

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);
        }

        private string GetSettingFilename(string section)
        {
            return Path.Combine(_directory, FileUtil.FilterFileName(section + "_Settings") + ".ini");
        }
    }
}
