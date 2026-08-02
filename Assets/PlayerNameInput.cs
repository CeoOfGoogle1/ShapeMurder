using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerNameInput : MonoBehaviour
{
    private TMP_InputField nameInputField;

    void Awake()
    {
        nameInputField = GetComponent<TMP_InputField>();    
    }

    private void OnEnable()
    {
        nameInputField.onValueChanged.AddListener(LocalDataManager.Instance.ChangeClientName);
    }

    private void OnDisable()
    {
        if(LocalDataManager.Instance != null)
        {
            nameInputField.onValueChanged.RemoveListener(LocalDataManager.Instance.ChangeClientName);
        }
    }
}
