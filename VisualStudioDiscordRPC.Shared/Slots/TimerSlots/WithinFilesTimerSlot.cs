﻿using EnvDTE;
using VisualStudioDiscordRPC.Shared.Observers;

namespace VisualStudioDiscordRPC.Shared.Slots.TimerSlots
{
    public class WithinFilesTimerSlot : TimerSlot
    {
        private VsObserver _vsObserver;

        public WithinFilesTimerSlot(VsObserver vsObserver)
        {
            _vsObserver = vsObserver;
        }

        public override void Enable()
        {
            _vsObserver.DocumentChanged += OnDocumentChanged;
        }

        public override void Disable()
        {
            _vsObserver.DocumentChanged -= OnDocumentChanged;
        }

        private void OnDocumentChanged(Document document)
        {
            Update();
        }
    }
}