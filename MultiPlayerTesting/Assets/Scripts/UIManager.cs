using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Netcode;

public class UIManager : MonoBehaviour
{
    
    [SerializeField]
    private Button startServerButton;
    [SerializeField]
    private Button startHostButton;
    [SerializeField]
    private Button startClientButton;
    [SerializeField]
    private TextMeshProUGUI playersInGameText;
    bool isHosting = false;
    public PlayersManager playerManager;
    public GameObject buttonsUI;
    private void Awake()
    {
        Cursor.visible = true;
    }
    private void Update()
    {
        if (isHosting == true)
        {
            playersInGameText.text = $"Players in game: {playerManager.PlayersInGame}";
        }
        else if (playerManager.PlayersInGame != 0)
        {
            isHosting = true;
        }

    }
    private void Start()
    {
        playersInGameText.text = null;
        startHostButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartHost())
            {
                Debug.Log("Host started");
                buttonsUI.SetActive(false);
            }
            else
            {
                Debug.Log("Could not start host");
            }
        });
        startServerButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartServer())
            {
                Debug.Log("Server started");
                buttonsUI.SetActive(false);
            }
            else
            {
                Debug.Log("Server could not be started");
            }
        });
        startClientButton.onClick.AddListener(() =>
        {
            if (NetworkManager.Singleton.StartClient())
            {
                Debug.Log("Client started");
                buttonsUI.SetActive(false);
            }
            else
            {
                Debug.Log("Client could not be started");
            }
        });
    }
}
