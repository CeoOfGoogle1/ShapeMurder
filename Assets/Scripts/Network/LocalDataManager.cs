using UnityEngine;
using UnityEngine.SocialPlatforms;

public class LocalDataManager : MonoBehaviour
{
    private static LocalDataManager instance;
    public static LocalDataManager Instance
    {
        get
        {
            if (instance == null)
            {
                instance = FindAnyObjectByType<LocalDataManager>();
            }
            return instance;
        }
    }

    public string ClientName;
    public Color ClientColor;
    
    private void Awake()
    {
        if (instance != null && instance != this) 
        { 
            Destroy(gameObject); 
            return; 
        }
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void ChangeClientColor(Color color)
    {
        ClientColor = color;
    }

    public void ChangeClientName(string name)
    {
        ClientName = name;
    }
}
