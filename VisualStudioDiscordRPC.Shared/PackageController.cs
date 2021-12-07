﻿using DiscordRPC;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.IO;
using VisualStudioDiscordRPC.Shared.AssetMap.Interfaces;
using VisualStudioDiscordRPC.Shared.AssetMap.Models;
using VisualStudioDiscordRPC.Shared.AssetMap.Models.Assets;
using VisualStudioDiscordRPC.Shared.AssetMap.Models.Loaders;
using VisualStudioDiscordRPC.Shared.Localization.Models;

namespace VisualStudioDiscordRPC.Shared
{
    public class PackageController : IDisposable
    {
        private readonly DTE _instance;
        private readonly DiscordRpcClient _client;
        private readonly RichPresence _presence;

        public readonly LocalizationManager<LocalizationFile> LocalizationManager;

        private readonly IAssetMap<ExtensionAsset> _extensionsAssetMap;
        private readonly ExtensionAssetComparer _extensionAssetComparer;

        private readonly string _installationPath;
        private int _version;

        private readonly Dictionary<int, int> _versions = new Dictionary<int, int>()
        {
            { 16, 2019 },
            { 17, 2022 }
        };

        private int GetVersionMajor(string version)
        {
            return int.Parse(version.Split('.')[0]);
        }

        private string GetLocalFilePath(string filename)
        {
            return Path.Combine(_installationPath, filename);
        }

        public PackageController(DTE instance, string installationPath)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _instance = instance;
            _instance.Events.WindowEvents.WindowActivated += WindowEvents_WindowActivated;

            _version = _versions[GetVersionMajor(_instance.Version)];

            _installationPath = installationPath;

            // Extension asset map settings
            _extensionsAssetMap = new AssetMap<ExtensionAsset>();

            var extensionAssetLoader = new JsonAssetsLoader<ExtensionAsset>();
            _extensionsAssetMap.Assets = extensionAssetLoader.LoadAssets(GetLocalFilePath("extensions_assets_map.json"));

            _extensionAssetComparer = new ExtensionAssetComparer();

            // Localization manager settings
            LocalizationManager = new LocalizationManager<LocalizationFile>(GetLocalFilePath(Settings.Default.TranslationsPath));
            LocalizationManager.LocalizationChanged += LocalizationManager_LocalizationChanged;

            // Discord Rich Presense client settings
            _client = new DiscordRpcClient(Settings.Default.ApplicationID);
            _client.Initialize();

            _presence = new RichPresence()
            {
                Assets = new Assets()
            };

            LocalizationManager.SelectLanguage(Settings.Default.Language);
            UpdateRichPresence();
        }

        private void LocalizationManager_LocalizationChanged()
        {
            UpdateText(true);
        }

        public void UpdateText(bool update)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            Document activeDocument = _instance.ActiveDocument;

            if (activeDocument != null)
            {
                _presence.Details = string.Format(ConstantStrings.ActiveFileFormat,
                    new object[] { LocalizationManager.Current.File, activeDocument.Name });

                _presence.State = string.Format(ConstantStrings.ActiveProjectFormat,
                    new object[] { LocalizationManager.Current.Project, activeDocument.ActiveWindow.Project.Name });

                _presence.Timestamps = Timestamps.Now;
            }
            else
            {
                _presence.Details = LocalizationManager.Current.NoActiveFile;
                _presence.State = LocalizationManager.Current.NoActiveProject;
                _presence.Timestamps = null;
            }

            if (update)
            {
                _client.SetPresence(_presence);
            }
        }

        public void UpdateIcon(bool update)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            if (_instance.ActiveDocument != null)
            {
                string fileExtension = Path.GetExtension(_instance.ActiveDocument.Name);

                _extensionAssetComparer.RequiredExtension = fileExtension;

                ExtensionAsset extensionAsset =
                        _extensionsAssetMap.GetAsset(_extensionAssetComparer) ?? ExtensionAsset.Default;

                _presence.Assets.LargeImageKey = extensionAsset.Key;
                _presence.Assets.LargeImageText = extensionAsset.Name;

                _presence.Assets.SmallImageKey = 
                    string.Format(ConstantStrings.VisualStudioVersionAssetKey, _version);
                _presence.Assets.SmallImageText = 
                    string.Format(ConstantStrings.VisualStudioVersion, _version);
            }
            else
            {
                _presence.Assets.LargeImageKey =
                    string.Format(ConstantStrings.VisualStudioVersionAssetKey, _version);
                _presence.Assets.LargeImageText =
                    string.Format(ConstantStrings.VisualStudioVersion, _version);

                _presence.Assets.SmallImageKey = string.Empty;
                _presence.Assets.SmallImageText = string.Empty;
            }
            
            if (update)
            {
                _client.SetPresence(_presence);
            }
        }

        public void UpdateRichPresence()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            UpdateText(false);
            UpdateIcon(false);

            _client.SetPresence(_presence);
        }

        private void WindowEvents_WindowActivated(Window GotFocus, Window LostFocus)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            
            UpdateRichPresence();
        }

        public void Dispose()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            _instance.Events.WindowEvents.WindowActivated -= WindowEvents_WindowActivated;
            LocalizationManager.LocalizationChanged -= LocalizationManager_LocalizationChanged;

            _client.Dispose();
        }
    }
}