using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Displays a terms and conditions agreement on first launch or when updated.
/// Prevents the user from continuing until all agreements are accepted.
/// </summary>
public class AcceptAgreementFirstBoot : MonoBehaviour
{

    [SerializeField] string agreementLastChangedDate; // Used as a unique key to track agreement version
    [SerializeField] Toggle terms, privacy;           // User must agree to both to proceed
    [SerializeField] Button continueBtn;              // Only enabled when both toggles are checked

    /// <summary>
    /// Checks if the agreement has already been accepted.
    /// If not, disables the continue button and resets toggles.
    /// </summary>
    void Start()
    {
        string alreadyAgreed = PlayerPrefs.GetString(agreementLastChangedDate, "no");

        if (alreadyAgreed == "yes")
        {
            // Agreement already accepted — skip this window
            Debug.Log("User has already agreed to the latest terms and conditions...");
            GetComponent<Window>().Hide();
            return;
        }

        // First-time agreement required — prepare UI to force interaction
        Debug.Log("User has NOT agreed to the latest terms and conditions...");

        if (terms == null || privacy == null || continueBtn == null) { return; }

        terms.isOn = false;
        privacy.isOn = false;
        continueBtn.interactable = false;
    }

    /// <summary>
    /// Called whenever a toggle is changed.
    /// Enables the continue button only if both toggles are on.
    /// </summary
    public void ToggleChanged()
    {
        if (terms.isOn && privacy.isOn)
        {
            continueBtn.interactable = true;
        }
        else
        {
            continueBtn.interactable = false;
        }
    }

    /// <summary>
    /// Stores acceptance and dismisses the agreement window.
    /// Called when the user agrees and continues.
    /// </summary>
    public void AcceptTermsConditions()
    {
        PlayerPrefs.SetString(agreementLastChangedDate, "yes");

        GetComponent<Window>().Hide();
    }

}
