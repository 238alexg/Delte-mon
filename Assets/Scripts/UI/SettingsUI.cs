using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SettingsUI : UIScreen {

    [SerializeField] InputField PlayerName;
    [SerializeField] Slider ScrollSpeed;
    [SerializeField] Slider MusicVolume;
    [SerializeField] Slider FXVolume;
    [SerializeField] Toggle PorkToggle;

    [SerializeField] Image MaleImage, FemaleImage;

    // Open settings menu on settings button push
    public override void Open()
    {
        // Set UI tools to current settings of the user
        PlayerName.text = GameManager.Inst.playerName;
        ScrollSpeed.value = 1 / UIManager.Inst.scrollSpeed;
        MusicVolume.value = MusicManager.Inst.maxVolume;
        FXVolume.value = SoundEffectManager.Inst.source.volume;
        PorkToggle.isOn = GameManager.Inst.pork;

        // Select character to current gender selection of user
        MaleImage.color = PlayerMovement.Inst.isMale ? Color.white : Color.grey;
        FemaleImage.color = PlayerMovement.Inst.isMale ? Color.grey : Color.white;

        EnableUI();
        Animator.SetBool("SlideIn", true);
        PlayerMovement.Inst.StopMoving();

        base.Open();
    }

    // Close settings menu on back button push
    public override void Close()
    {
        base.Close();
        PlayerMovement.Inst.ResumeMoving();
    }

    // Setting function: Raise/lower scroll speed and save
    public void ChangeTextScrollSpeed(BaseEventData evdata)
    {
        UIManager.Inst.scrollSpeed = 1 / evdata.selectedObject.GetComponent<Slider>().value;
        UIManager.Inst.StartMessage("This is how fast messages will appear in the future. Tap while animating for faster text");
    }
}
