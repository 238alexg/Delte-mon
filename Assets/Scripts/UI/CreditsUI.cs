using System.Collections;
using BattleDelts.UI;
using UnityEngine;
using UnityEngine.UI;

public class CreditsUI : UIScreen {
    
    public Scrollbar CreditsScroll;

    // Open the credits UI
    public override void Open()
    {
        StartCoroutine(animateCredits());
        base.Open();
    }

    // Animates credits downwards and plays credits music
    IEnumerator animateCredits()
    {
        yield return StartCoroutine(MusicManager.Inst.fadeOutAudio());
        yield return StartCoroutine(MusicManager.Inst.fadeInAudio("Credits"));
        yield return new WaitForSeconds(1.5f);

        // Scroll credits down
        while (CreditsScroll.value > 0)
        {
            CreditsScroll.value -= 0.00018f;
            yield return new WaitForSeconds(0.00018f);
        }
    }

    // Closes Credits Screen
    public void CloseCredits()
    {
        // Reset Credits to top of scrollable area
        CreditsScroll.value = 1;

        // Close credits
        UIManager.Inst.StartMessage(null, AnimateUIClose());

        // Fade out of music and resume scene music
        UIManager.Inst.StartMessage(null, MusicManager.Inst.fadeOutAudio());
        UIManager.Inst.StartMessage(null, MusicManager.Inst.fadeInAudio(GameManager.Inst.curSceneName));
    }
}
