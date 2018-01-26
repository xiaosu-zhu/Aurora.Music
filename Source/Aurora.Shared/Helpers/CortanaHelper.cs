// Copyright (c) Aurora Studio. All rights reserved.
//
// Licensed under the MIT License. See LICENSE in the project root for license information.
using System;
using System.Threading.Tasks;

namespace Aurora.Shared.Helpers
{
    public static class CortanaHelper
    {
        public static async Task EditPhraseListAsync(string setName, string listName, string[] phraseList)
        {

            if (Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinitionManager.InstalledCommandDefinitions.TryGetValue(setName, out Windows.ApplicationModel.VoiceCommands.VoiceCommandDefinition commandSetEnUs))
            {
                await commandSetEnUs.SetPhraseListAsync(listName, phraseList);
            }
        }
    }
}
