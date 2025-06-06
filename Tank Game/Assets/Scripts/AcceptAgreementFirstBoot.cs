using System;
using UnityEngine;
using UnityEngine.UI;

public class AcceptAgreementFirstBoot : MonoBehaviour
{
    [SerializeField] string agreementLastChangedDate;
    [SerializeField] Toggle terms, privacy;
    [SerializeField] Button continueBtn;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        string alreadyAgreed = PlayerPrefs.GetString(agreementLastChangedDate, "no");
        if (alreadyAgreed == "yes")
        {
            Debug.Log("User has already agreed to the latest terms and conditions...");
            GetComponent<Window>().Hide();
            return;
        }

        Debug.Log("User has NOT agreed to the latest terms and conditions...");
        if (terms == null || privacy == null || continueBtn == null) { return; }
        terms.isOn = false;
        privacy.isOn = false;
        continueBtn.interactable = false;
    }

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

    public void AcceptTermsConditions()
    {
        PlayerPrefs.SetString(agreementLastChangedDate, "yes");

        GetComponent<Window>().Hide();
    }

}
