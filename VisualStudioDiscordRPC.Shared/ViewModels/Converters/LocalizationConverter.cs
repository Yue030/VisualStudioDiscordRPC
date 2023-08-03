﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;
using VisualStudioDiscordRPC.Shared.Localization;
using VisualStudioDiscordRPC.Shared.Localization.Models;
using VisualStudioDiscordRPC.Shared.Services.Models;
using VisualStudioDiscordRPC.Shared.Slots;

namespace VisualStudioDiscordRPC.Shared.ViewModels.Converters
{
    public class LocalizationConverter : IValueConverter
    {
        private readonly LocalizationService<LocalizationFile> _localizationService;

        public LocalizationConverter()
        {
            _localizationService =
                ServiceRepository.Default.GetService<LocalizationService<LocalizationFile>>();
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            string objectTypeName = value.GetType().Name;
            if (_localizationService.Current.LocalizedValues.TryGetValue(objectTypeName, out var localizedName))
            {
                return localizedName;
            }

            return objectTypeName;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
