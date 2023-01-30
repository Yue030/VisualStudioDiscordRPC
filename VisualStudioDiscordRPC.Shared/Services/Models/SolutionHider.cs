﻿using EnvDTE;
using VisualStudioDiscordRPC.Shared.Observers;
using VisualStudioDiscordRPC.Shared.Utils;

namespace VisualStudioDiscordRPC.Shared.Services.Models
{
    public class SolutionHider
    {
        private readonly VsObserver _vsObserver;
        private readonly DiscordRpcController _discordRpcController;

        private Solution _lastOpenedSolution;

        public SolutionHider(VsObserver vsObserver, DiscordRpcController discordRpcController) 
        {
            _vsObserver = vsObserver;
            _discordRpcController = discordRpcController;
        }

        public void Start()
        {
            _vsObserver.SolutionChanged += OnSolutionChanged;
        }

        public void Stop()
        {
            _vsObserver.SolutionChanged -= OnSolutionChanged;
        }

        public void SetCurrentSolutionVisible(bool visible)
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();

            if (_lastOpenedSolution == null)
            {
                return;
            }

            string fullSolutionName = _lastOpenedSolution.FullName;
            SettingsHelper.SetSolutionVisible(fullSolutionName, visible);

            UpdateRpcVisibility();
        }

        private void OnSolutionChanged(Solution solution)
        {
            _lastOpenedSolution = solution;

            UpdateRpcVisibility();
        }

        private void UpdateRpcVisibility()
        {
            Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
            
            string fullSolutionName = _lastOpenedSolution.FullName;

            bool visible = SettingsHelper.IsSolutionVisible(fullSolutionName);
            _discordRpcController.Visible = visible;
        }
    }
}