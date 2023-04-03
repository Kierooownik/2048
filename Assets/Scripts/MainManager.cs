using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MainManager : MonoBehaviour
{
    public static MainManager Instance;
    public int BoardSize;
    [SerializeField] private TMP_Dropdown _dropdownBoardSize;
    [SerializeField] private List<int> _optionValues;
    public int CurrentBoardSize;

    private void Awake()
    {
        if(Instance != null)
        {
            Destroy(gameObject);
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

    }
    private void Update()
    {
        if(SceneManager.GetActiveScene().buildIndex == 0)
        {
            GetBoardSize();
            BoardSize = CurrentBoardSize;
        }
    }

    public void GetBoardSize()
    {
        switch (_dropdownBoardSize.value)
        {
            case 0:
                CurrentBoardSize = _optionValues[0];
                break;
            case 1:
                CurrentBoardSize = _optionValues[1];
                break;
            case 2:
                CurrentBoardSize = _optionValues[2];
                break;
        }
    }
}
