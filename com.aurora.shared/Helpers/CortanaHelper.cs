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
