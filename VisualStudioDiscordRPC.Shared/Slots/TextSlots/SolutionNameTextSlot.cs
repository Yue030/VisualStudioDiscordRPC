﻿using EnvDTE;
using VisualStudioDiscordRPC.Shared.Localization.Models;
using VisualStudioDiscordRPC.Shared.Localization;
using VisualStudioDiscordRPC.Shared.Observers;
using System.IO;

namespace VisualStudioDiscordRPC.Shared.Slots.TextSlots
{
    public class SolutionNameTextSlot : TextSlot
    {
        private readonly VsObserver _vsObserver;
        private readonly LocalizationService<LocalizationFile> _localizationService;

        private Solution _solution;

        public SolutionNameTextSlot(VsObserver vsObserver, LocalizationService<LocalizationFile> localizationService)
        {
            _vsObserver = vsObserver;
            _localizationService = localizationService;

            _solution = vsObserver.DTE.Solution;
        }

        public override void Enable()
        {
            _vsObserver.SolutionChanged += OnSolutionChanged;
        }

        public override void Disable()
        {
            _vsObserver.SolutionChanged -= OnSolutionChanged;
        }

        private void OnSolutionChanged(Solution solution)
        {
            _solution = solution;
            Update();
        }

        protected override string GetData()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (_solution == null)
            {
                return _localizationService.Current.NoActiveSolution;
            }

            string solutionName = Path.GetFileNameWithoutExtension(_solution.FullName);
            return string.Format("{0} {1}", _localizationService.Current.Solution, solutionName);
        }
    }
}