using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class DialogueDataLoader
{
    public DialogueDataModel Load(string dialogueId)
    {
        var asset = Resources.Load<TextAsset>($"Dialogue/{dialogueId}");
        if (asset == null)
        {
            Debug.LogError($"Missing dialogue JSON: {dialogueId}");
            return null;
        }

        return JsonConvert.DeserializeObject<DialogueDataModel>(asset.text);
    }
}
