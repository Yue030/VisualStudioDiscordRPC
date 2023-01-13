﻿using EnvDTE;
using VisualStudioDiscordRPC.Shared.Localization.Models;
using VisualStudioDiscordRPC.Shared.Localization;
using VisualStudioDiscordRPC.Shared.Observers;
using VisualStudioDiscordRPC.Shared.Services.Models;

namespace VisualStudioDiscordRPC.Shared.Slots.TextSlots
{
    public class ProjectNameSlot : TextSlot
    {
        private LocalizationService<LocalizationFile> _localizationService;
        private VsObserver _vsObserver;

        private Project _project;

        public ProjectNameSlot(VsObserver vsObserver)
        {
            _localizationService = ServiceRepository.Default.GetService<LocalizationService<LocalizationFile>>();
            _vsObserver = vsObserver;
        }

        public override void Enable()
        {
            _vsObserver.ProjectChanged += OnProjectChanged;
        }

        public override void Disable()
        {
            _vsObserver.ProjectChanged -= OnProjectChanged;
        }

        private void OnProjectChanged(Project project)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            _project = project;
            Update();
        }

        protected override string GetData()
        {
            if (_project == null)
            {
                return _localizationService.Current.NoActiveProject;
            }

            return string.Format(ConstantStrings.ActiveProjectFormat, _localizationService.Current.Project, _project.Name);
        }
    }
}